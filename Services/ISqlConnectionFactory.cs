using System.Data;
using Microsoft.Data.SqlClient;

namespace DotNetProjectForAntigravity.Services;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection(string connectionString);
}

public class SqlConnectionFactory : ISqlConnectionFactory
{
    public IDbConnection CreateConnection(string connectionString)
    {
        // Return SqlConnection as IDbConnection
        return new SqlConnection(connectionString);
    }
}
