using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using DotNetProjectForAntigravity.Data;

namespace DotNetProjectForAntigravity.Services;

public class DatabaseService
{
    private readonly IConfiguration _configuration;
    private readonly ISqlConnectionFactory _connectionFactory;

    public DatabaseService(IConfiguration configuration, ISqlConnectionFactory connectionFactory)
    {
        _configuration = configuration;
        _connectionFactory = connectionFactory;
    }

    private string GetConnectionString()
    {
        return _configuration.GetConnectionString("DefaultConnection") ?? 
               "Server=localhost;Database=master;Integrated Security=true;TrustServerCertificate=true;";
    }

    public async Task<List<TableInfo>> GetTablesAsync()
    {
        var connectionString = GetConnectionString();
        using var connection = _connectionFactory.CreateConnection(connectionString);
        
        var query = @"
            SELECT 
                t.TABLE_SCHEMA as SchemaName,
                t.TABLE_NAME as TableName,
                (SELECT SUM(p.rows) 
                 FROM sys.partitions p 
                 INNER JOIN sys.indexes i ON p.object_id = i.object_id AND p.index_id = i.index_id
                 WHERE i.object_id = OBJECT_ID(QUOTENAME(t.TABLE_SCHEMA) + '.' + QUOTENAME(t.TABLE_NAME))
                 AND i.index_id IN (0,1)) as [RowCount]
            FROM INFORMATION_SCHEMA.TABLES t
            WHERE t.TABLE_TYPE = 'BASE TABLE'
            ORDER BY t.TABLE_SCHEMA, t.TABLE_NAME";

        var tables = await connection.QueryAsync<TableInfo>(query);
        return tables.ToList();
    }

    public async Task<List<IndexInfo>> GetIndexHealthAsync(string schemaName, string tableName)
    {
        var connectionString = GetConnectionString();
        using var connection = _connectionFactory.CreateConnection(connectionString);
        
        var query = @"
            SELECT 
                i.name as IndexName,
                i.type_desc as IndexType,
                CASE 
                    WHEN i.is_primary_key = 1 THEN 'Primary Key'
                    WHEN i.is_unique = 1 THEN 'Unique'
                    ELSE 'Non-Unique'
                END as IndexKind,
                CASE 
                    WHEN s.avg_fragmentation_in_percent > 30 THEN 'Poor'
                    WHEN s.avg_fragmentation_in_percent > 10 THEN 'Fair'
                    ELSE 'Good'
                END as HealthStatus,
                ISNULL(s.avg_fragmentation_in_percent, 0) as FragmentationPercent
            FROM sys.indexes i
            LEFT JOIN sys.dm_db_index_physical_stats(DB_ID(), OBJECT_ID(QUOTENAME(@SchemaName) + '.' + QUOTENAME(@TableName)), NULL, NULL, 'LIMITED') s
                ON i.object_id = s.object_id AND i.index_id = s.index_id
            WHERE i.object_id = OBJECT_ID(QUOTENAME(@SchemaName) + '.' + QUOTENAME(@TableName))
            AND i.type > 0
            ORDER BY i.name";

        var indexes = await connection.QueryAsync<IndexInfo>(query, new { SchemaName = schemaName, TableName = tableName });
        return indexes.ToList();
    }

    public async Task<List<string>> GetStoredProceduresAsync()
    {
        var connectionString = GetConnectionString();
        using var connection = _connectionFactory.CreateConnection(connectionString);
        
        var query = @"
            SELECT 
                ROUTINE_SCHEMA + '.' + ROUTINE_NAME as ProcedureName
            FROM INFORMATION_SCHEMA.ROUTINES
            WHERE ROUTINE_TYPE = 'PROCEDURE'
            ORDER BY ROUTINE_SCHEMA, ROUTINE_NAME";

        var procedures = await connection.QueryAsync<string>(query);
        return procedures.ToList();
    }

    public async Task<string> GetStoredProcedureDefinitionAsync(string procedureName)
    {
        var connectionString = GetConnectionString();
        using var connection = _connectionFactory.CreateConnection(connectionString);
        
        var query = @"
            SELECT OBJECT_DEFINITION(OBJECT_ID(@ProcedureName)) as Definition";

        var result = await connection.QueryFirstOrDefaultAsync<string>(query, new { ProcedureName = procedureName });
        return result ?? string.Empty;
    }

    public async Task<BenchmarkResult> BenchmarkStoredProcedureAsync(string procedureName, string? optimizedProcedure = null)
    {
        var connectionString = GetConnectionString();
        var results = new BenchmarkResult();

        // Benchmark original
        using (var connection = _connectionFactory.CreateConnection(connectionString))
        {
            if (connection is System.Data.Common.DbConnection dbConn) { await dbConn.OpenAsync(); } else { connection.Open(); }
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                await connection.ExecuteAsync(procedureName, commandType: CommandType.StoredProcedure);
                stopwatch.Stop();
                results.OriginalExecutionTime = stopwatch.ElapsedMilliseconds;
                results.OriginalSuccess = true;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                results.OriginalExecutionTime = stopwatch.ElapsedMilliseconds;
                results.OriginalSuccess = false;
                results.OriginalError = ex.Message;
            }
        }

        // Benchmark optimized if provided
        if (!string.IsNullOrEmpty(optimizedProcedure))
        {
            using (var connection = _connectionFactory.CreateConnection(connectionString))
            {
                if (connection is System.Data.Common.DbConnection dbConn) { await dbConn.OpenAsync(); } else { connection.Open(); }
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                try
                {
                    // For now, we'll just execute the optimized SQL directly
                    // In a real scenario, you'd create a temp procedure or use dynamic SQL
                    await connection.ExecuteAsync(optimizedProcedure);
                    stopwatch.Stop();
                    results.OptimizedExecutionTime = stopwatch.ElapsedMilliseconds;
                    results.OptimizedSuccess = true;
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    results.OptimizedExecutionTime = stopwatch.ElapsedMilliseconds;
                    results.OptimizedSuccess = false;
                    results.OptimizedError = ex.Message;
                }
            }
        }

        return results;
    }

    public async Task<List<string>> GetTableColumnsAsync(string schemaName, string tableName)
    {
        var connectionString = GetConnectionString();
        using var connection = _connectionFactory.CreateConnection(connectionString);
        
        var query = @"
            SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = @SchemaName AND TABLE_NAME = @TableName
            ORDER BY ORDINAL_POSITION";

        var columns = await connection.QueryAsync<string>(query, new { SchemaName = schemaName, TableName = tableName });
        return columns.ToList();
    }
}

