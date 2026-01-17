namespace DotNetProjectForAntigravity.Data;

public class BenchmarkResult
{
    public long OriginalExecutionTime { get; set; }
    public bool OriginalSuccess { get; set; }
    public string? OriginalError { get; set; }
    public long OptimizedExecutionTime { get; set; }
    public bool OptimizedSuccess { get; set; }
    public string? OptimizedError { get; set; }
    
    public double ImprovementPercent
    {
        get
        {
            if (OriginalExecutionTime == 0) return 0;
            return ((double)(OriginalExecutionTime - OptimizedExecutionTime) / OriginalExecutionTime) * 100;
        }
    }
}

