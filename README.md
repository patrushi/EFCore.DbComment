# EFCore.DbComment

## Using for postgres and XmlDoc

In DbContect.OnModelCreating insert
```
var commentModel = EFCore.DbComment.CommentModel.CreateFromXmlDocFile(builder.Model);
EFCore.DbComment.PgComment.AddCommentsToModel(commentModel);
```
Don't forget to enable XML documentation

## Using for postgres and [Description] attr

In DbContect.OnModelCreating insert
```
var commentModel = EFCore.DbComment.CommentModel.CreateFromDescriptionAttr(builder.Model);
EFCore.DbComment.PgComment.AddCommentsToModel(commentModel);
```
