using SqlSugar;

namespace SqliteSugarDemo.Entities
{
    [SugarTable("Users")]
    public class User
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        
        [SugarColumn(ColumnName = "Name", IsNullable = false)]
        public string Name { get; set; }
        
        [SugarColumn(ColumnName = "Age")]
        public int Age { get; set; }
        
        [SugarColumn(ColumnName = "Email")]
        public string Email { get; set; }
        
        [SugarColumn(ColumnName = "CreateTime")]
        public DateTime CreateTime { get; set; }
    }
} 