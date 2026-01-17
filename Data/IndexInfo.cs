namespace DotNetProjectForAntigravity.Data;

public class IndexInfo
{
    public string IndexName { get; set; } = string.Empty;
    public string IndexType { get; set; } = string.Empty;
    public string IndexKind { get; set; } = string.Empty;
    public string HealthStatus { get; set; } = string.Empty;
    public decimal FragmentationPercent { get; set; }
}

