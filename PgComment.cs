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
    /// <summary>Comments for Postgres</summary>
    public static class PgComment
    {
        /// <summary>
        /// Add comments to EFCore model
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void AddCommentsToModel(ModelBuilder modelBuilder, CommentModel commentModel)
        {
            foreach (var entity in commentModel.EntityCommentList)
            {
                // comments on table
                var entityTypeBuilder = modelBuilder.Entity(entity.EntityType.ClrType);
                if (!string.IsNullOrEmpty(entity.Comment)) entityTypeBuilder.ForNpgsqlHasComment(entity.Comment);
            
                // comments on columns
                foreach (var property in entity.EntityPropertyList)
                {
                    var propertyTypeBuilder = entityTypeBuilder.Property(property.Property.Name);
                    if (!string.IsNullOrEmpty(property.Comment)) propertyTypeBuilder.ForNpgsqlHasComment(property.Comment);
                }
            }
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
                    result.Add($"COMMENT ON TABLE {entity.EntityType.Relational().TableName} IS '{entity.Comment}'");
                }
            
                // comments on columns
                foreach (var property in entity.EntityPropertyList)
                {
                    if (!string.IsNullOrEmpty(property.Comment))
                    {
                        result.Add($"COMMENT ON COLUMN {property.Property.DeclaringEntityType.Relational().TableName}.{property.Property.Relational().ColumnName} IS '{property.Comment}'");
                    }
                }
            }

            return result;
        }
    }
}