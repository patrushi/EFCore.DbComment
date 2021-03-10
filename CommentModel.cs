using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore.DbComment
{
    /// <summary>Comments model</summary>
    public class CommentModel
    {
        /// <summary>
        /// Comment fro entity
        /// </summary>
        public class EntityComment
        {
            /// <summary>
            /// Comment
            /// </summary>
            public string Comment { get; set; }
        
            /// <summary>
            /// Type
            /// </summary>
            public IEntityType EntityType { get; set; }
        
            /// <summary>
            /// Attr list
            /// </summary>
            public IList<PropertyComment> EntityPropertyList { get; set; }
        }

        /// <summary>
        /// Attr
        /// </summary>
        public class PropertyComment
        {
            /// <summary>
            /// Comment
            /// </summary>
            public string Comment { get; set; }
        
            /// <summary>
            /// Type
            /// </summary>
            public IProperty Property { get; set; }
        }
        
        /// <summary>
        /// List of entities
        /// </summary>
        public IList<EntityComment> EntityCommentList { get; set; }

        /// <summary>
        /// Create from ef model, without comment
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private static CommentModel CreateFromModel(IModel model)
        {
            var resultModel = new CommentModel();
            
            resultModel.EntityCommentList = model.GetEntityTypes()
                .Select(et => new EntityComment
                {
                    EntityType = et,
                    EntityPropertyList = et.GetProperties()
                        .Select(p => new PropertyComment
                        {
                            Property = p
                        }).ToList()
                })
                .ToList();
            
            return resultModel;
        }

        /// <summary>
        /// Create model with comment from XmlDoc (summary tag)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="xmlAssembly"></param>
        /// <returns></returns>
        public static CommentModel CreateFromXmlDocFile(IModel model, Assembly xmlAssembly)
        {
            return CreateFromXmlDocFile(model, Path.Combine(AppContext.BaseDirectory, $"{xmlAssembly.GetName().Name}.xml"));
        }

        /// <summary>
        /// Create model with comment from XmlDoc (summary tag)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="xmlDocFile"></param>
        /// <returns></returns>
        public static CommentModel CreateFromXmlDocFile(IModel model, string xmlDocFile)
        {
            var resultModel = CreateFromModel(model);
            
            var xdoc = XDocument.Load(xmlDocFile);
            var commentDict = xdoc.Element("doc")!.Element("members")!.Elements("member")
                .ToDictionary(e => e.Attribute("name")?.Value, e => e.Element("summary")?.Value?.Trim());
            
            foreach (var entity in resultModel.EntityCommentList)
            {
                entity.Comment = GetEntityComment(commentDict, entity.EntityType.ClrType);

                foreach (var property in entity.EntityPropertyList)
                {
                    if (property.Property.IsShadowProperty()) continue;
                    property.Comment = GetPropertyComment(commentDict, entity.EntityType.ClrType, property.Property);
                }
            }

            return resultModel;
        }
        
        /// <summary>
        /// Create model with comment from clr attr Description
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static CommentModel CreateFromDescriptionAttr(IModel model)
        {
            var resultModel = CreateFromModel(model);
            
            foreach (var entity in resultModel.EntityCommentList)
            {
                entity.Comment = ((DescriptionAttribute)entity.EntityType.ClrType.GetCustomAttribute(typeof(DescriptionAttribute)))?.Description;

                foreach (var property in entity.EntityPropertyList)
                {
                    property.Comment = ((DescriptionAttribute)property.Property.PropertyInfo.GetCustomAttribute(typeof(DescriptionAttribute)))?.Description;
                }
            }

            return resultModel;
        }
        
        private static string GetEntityComment(Dictionary<string, string> commentDict, Type type)
        {
            if (commentDict.TryGetValue($"T:{type.FullName}", out var comment) && !string.IsNullOrEmpty(comment)) return comment;
            return type.BaseType == null
                ? null
                : GetEntityComment(commentDict, type.BaseType);
        }
        
        private static string GetPropertyComment(Dictionary<string, string> commentDict, Type type, IProperty property)
        {
            if (commentDict.TryGetValue($"P:{type.FullName}.{property.Name}", out var comment) && !string.IsNullOrEmpty(comment)) return comment;
            return type.BaseType == null
                ? null
                : GetPropertyComment(commentDict, type.BaseType, property);
        }

        /// <summary>
        /// Get sql for create comments
        /// </summary>
        /// <returns></returns>
        public static IList<string> GetCommentsForPaste(CommentModel commentModel)
        {
            var result = new List<string>();
            
            foreach (var entity in commentModel.EntityCommentList)
            {
                // comments on table
                if (!string.IsNullOrEmpty(entity.Comment))
                {
                    result.Add($"COMMENT ON TABLE {entity.EntityType.GetTableName()} IS '{entity.Comment}'");
                }
            
                // comments on columns
                foreach (var property in entity.EntityPropertyList)
                {
                    if (!string.IsNullOrEmpty(property.Comment))
                    {
                        var schema = property.Property.DeclaringEntityType.GetSchema();
                        var tableName = property.Property.DeclaringEntityType.GetTableName();
                        result.Add($"COMMENT ON COLUMN {tableName}.{property.Property.GetColumnName(StoreObjectIdentifier.Table(tableName, schema))} IS '{property.Comment}'");
                    }
                }
            }

            return result;
        }
    }
}