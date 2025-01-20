using SqlSugar;

namespace SqliteSugarDemo.Entities
{
    [SugarTable("Departments")]
    public class Department
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        
        [SugarColumn(ColumnName = "Name", IsNullable = false)]
        public string Name { get; set; }
        
        [SugarColumn(ColumnName = "Description")]
        public string Description { get; set; }
        
        [SugarColumn(ColumnName = "CreateTime")]
        public DateTime CreateTime { get; set; }
    }
} 