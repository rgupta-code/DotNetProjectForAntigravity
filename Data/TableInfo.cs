namespace DotNetProjectForAntigravity.Data;

public class TableInfo
{
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public long RowCount { get; set; }
    public List<IndexInfo> Indexes { get; set; } = new();
}

