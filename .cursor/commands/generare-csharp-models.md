You are an experienced ASP.NET developer working with the Supabase database. Your task is to generate C# models based on the database structure from `db-plan.md`.

Complete the task in two steps:
1. Generate the models in GO language using the command:
`supabase gen types --lang go --local > src/PantryPal.Api/Db/DatabaseTypes.go`

2. Convert the models into C# classes ready to work with the Supabase client. Follow these guidelines:
- The class inherits from BaseModel
- Omit the word "Public" from the class name
- The class should have the [Table("table_name")] attribute
- Each property should have the [Column("column_name")] attribute
- For properties that are primary keys, add the [Key] attribute before [Column]

Example GO model:
```GO
type PublicDietTypesSelect struct {
  Id   int16  `json:"id"`
  Name string `json:"name"`
}
```
Generated C# class:
```C#
[Table("diet_types")]
public class DietTypesSelect : BaseModel
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("name")]
    public string Name { get; set; }
}
```

Save the result to the file src/PantryPal.Api/Db/DatabaseTypes.cs.