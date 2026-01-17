namespace DotNetProjectForAntigravity.Data;

public class DailySummary
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime ExecutionDate { get; set; }
    public string Status { get; set; } = "Unknown";
    public int TotalTests { get; set; }
    public int PassedTests { get; set; }
    public int FailedTests { get; set; }
    public int SkippedTests { get; set; }
    public double TestDurationSeconds { get; set; }
    public string CommitHash { get; set; } = string.Empty;
    public List<string> FilesModified { get; set; } = new();
    public List<string> FilesAdded { get; set; } = new();
    public List<TestResultDetail> TestResults { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
    public Dictionary<string, int> CoverageByService { get; set; } = new();
}

public class TestResultDetail
{
    public string TestName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string Status { get; set; } = "Unknown";
    public double DurationSeconds { get; set; }
    public string? ErrorMessage { get; set; }
}
