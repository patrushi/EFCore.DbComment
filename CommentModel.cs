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
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static CommentModel CreateFromXmlDocFile(IModel model, string filePath = null)
        {
            var xmlDocFile = filePath ?? Path.Combine(AppContext.BaseDirectory, $"{typeof(CommentModel).Assembly.GetName().Name}.xml");

            var resultModel = CreateFromModel(model);
            
            var xdoc = XDocument.Load(xmlDocFile);
            var commentDict = xdoc.Element("doc").Element("members").Elements("member")
                .ToDictionary(e => e.Attribute("name")?.Value, e => e.Element("summary")?.Value?.Trim());
            
            foreach (var entity in resultModel.EntityCommentList)
            {
                entity.Comment = GetEntityComment(commentDict, entity.EntityType.ClrType);

                foreach (var property in entity.EntityPropertyList)
                {
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
    }
}