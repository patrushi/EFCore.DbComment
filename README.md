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
EFCore.DbComment.PgComment.AddCommentsToModel(builder, commentModel);
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
EFCore.DbComment.PgComment.AddCommentsToModel(builder, commentModel);
```

# Links
* Nuget - https://www.nuget.org/packages/EFCore.DbComment/
