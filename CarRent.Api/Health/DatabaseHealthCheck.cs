using CarRent.Application.DataBase;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CarRent.Api.Health;

public class DatabaseHealthCheck : IHealthCheck
{
    public const string Name = "Database";
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(
        IDbConnectionFactory dbConnectionFactory
        , ILogger<DatabaseHealthCheck> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try 
        {
            _ = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }catch (Exception ex) 
        {
            const string errorMessage = "Database in unhealthy";
            _logger.LogError(errorMessage, ex);
            return HealthCheckResult.Unhealthy(errorMessage, ex);
        }
    }
}
