# EFCore.DbComment

## Using for postgres and XmlDoc

```
/// <summary>User</summary>
public class User
{
    /// <summary>Full name</summary>
    public string Name { get; set; }
}
```

In DbContect.OnModelCreating insert
```
var commentModel = EFCore.DbComment.CommentModel.CreateFromXmlDocFile(builder.Model);
EFCore.DbComment.PgComment.AddCommentsToModel(commentModel);
```
Don't forget to enable XML documentation

## Using for postgres and [Description] attr

```
[Description("User")]
public class User
{
    [Description("Full name")]
    public string Name { get; set; }
}
```

In DbContect.OnModelCreating insert
```
var commentModel = EFCore.DbComment.CommentModel.CreateFromDescriptionAttr(builder.Model);
EFCore.DbComment.PgComment.AddCommentsToModel(commentModel);
```
