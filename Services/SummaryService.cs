using System.Text.Json;
using DotNetProjectForAntigravity.Data;

namespace DotNetProjectForAntigravity.Services;

public class SummaryService
{
    private readonly string _summaryDirectory;
    private readonly JsonSerializerOptions _jsonOptions;

    public SummaryService(IWebHostEnvironment environment)
    {
        _summaryDirectory = Path.Combine(environment.ContentRootPath, "Data", "Summaries");
        Directory.CreateDirectory(_summaryDirectory);
        
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task SaveSummaryAsync(DailySummary summary)
    {
        var fileName = $"summary_{summary.ExecutionDate:yyyy-MM-dd_HHmmss}.json";
        var filePath = Path.Combine(_summaryDirectory, fileName);
        
        var json = JsonSerializer.Serialize(summary, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<List<DailySummary>> GetAllSummariesAsync()
    {
        var summaries = new List<DailySummary>();
        var files = Directory.GetFiles(_summaryDirectory, "summary_*.json")
                            .OrderByDescending(f => f);

        foreach (var file in files)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var summary = JsonSerializer.Deserialize<DailySummary>(json, _jsonOptions);
                if (summary != null)
                {
                    summaries.Add(summary);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading summary file {file}: {ex.Message}");
            }
        }

        return summaries.OrderByDescending(s => s.ExecutionDate).ToList();
    }

    public async Task<DailySummary?> GetSummaryByIdAsync(string id)
    {
        var summaries = await GetAllSummariesAsync();
        return summaries.FirstOrDefault(s => s.Id == id);
    }

    public async Task<DailySummary?> GetLatestSummaryAsync()
    {
        var summaries = await GetAllSummariesAsync();
        return summaries.FirstOrDefault();
    }

    public async Task<List<DailySummary>> GetSummariesByDateRangeAsync(DateTime start, DateTime end)
    {
        var summaries = await GetAllSummariesAsync();
        return summaries.Where(s => s.ExecutionDate >= start && s.ExecutionDate <= end).ToList();
    }

    public async Task<Dictionary<string, object>> GetStatisticsAsync()
    {
        var summaries = await GetAllSummariesAsync();
        
        return new Dictionary<string, object>
        {
            ["TotalRuns"] = summaries.Count,
            ["SuccessfulRuns"] = summaries.Count(s => s.Status == "Success"),
            ["FailedRuns"] = summaries.Count(s => s.Status == "Failed"),
            ["AverageTestsPerRun"] = summaries.Any() ? summaries.Average(s => s.TotalTests) : 0,
            ["AveragePassRate"] = summaries.Any() && summaries.Sum(s => s.TotalTests) > 0 
                ? (double)summaries.Sum(s => s.PassedTests) / summaries.Sum(s => s.TotalTests) * 100 
                : 0,
            ["LastRunDate"] = summaries.FirstOrDefault()?.ExecutionDate ?? DateTime.MinValue
        };
    }
}
