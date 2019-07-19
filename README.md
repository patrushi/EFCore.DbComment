EFCore.DbComment

Add comment for objects (table, column) from code to database.
You can use it with xml comment summary or Description clr attr.

# Using for postgres and XmlDoc

```
/// <summary>User</summary>
public class User
{
    /// <summary>Full name</summary>
    public string Name { get; set; }
}
```

In DbContext.OnModelCreating(ModelBuilder builder) insert
```
var commentModel = EFCore.DbComment.CommentModel.CreateFromXmlDocFile(builder.Model, this.GetType().Assembly);
AddCommentsToModel(builder, commentModel);

/// <summary>Add comments to EFCore model</summary>
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
```

And then you can do ```dotnet ef migrations add ...```

Don't forget to enable XML documentation

# Using for postgres and [Description] attr

```
[Description("User")]
public class User
{
    [Description("Full name")]
    public string Name { get; set; }
}
```

In DbContext.OnModelCreating(ModelBuilder builder) insert
```
var commentModel = EFCore.DbComment.CommentModel.CreateFromDescriptionAttr(builder.Model);
AddCommentsToModel(builder, commentModel);
```

# Links
* Nuget - https://www.nuget.org/packages/EFCore.DbComment/
