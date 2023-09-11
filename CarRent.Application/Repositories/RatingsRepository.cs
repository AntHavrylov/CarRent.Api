using CarRent.Application.DataBase;
using CarRent.Application.Models;
using Dapper;
using Microsoft.Extensions.Logging;

namespace CarRent.Application.Repositories;

public class RatingsRepository : IRatingsRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<OrdersRepository> _logger;

    public RatingsRepository(IDbConnectionFactory dbConnectionFactory,
        ILogger<OrdersRepository> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    public async Task<bool> RateCarAsync(Guid userId, Guid carId, int rating, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        var result = await connection.ExecuteAsync(new CommandDefinition("""
            insert into ratings(user_id, car_id, rating) 
            values (@userId, @carId, @rating)
            on conflict (user_id, car_id) do update 
                set rating = @rating
            """, new { userId, carId, rating }, cancellationToken: token));

        _logger.LogInformation("Rating from {UserId} for car {CarId} create {OpResult}", userId, carId, result > 0 ? "success" : "fail");
        return result > 0;

    }

    public async Task<bool> ExistsCarRatingForUser(Guid userId, Guid carId, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition($"""
            select count(1) from {DbConstants.OrdersTableName} 
            where user_id = @userId 
            and car_id = @carId
            """, new { userId, carId }, cancellationToken: token));
    }

    public async Task<bool> DeleteRatingAsync(Guid userId, Guid carId, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        var result = await connection.ExecuteAsync(new CommandDefinition($"""
                delete from {DbConstants.RatingsTableName}
                where user_id = @userId
                and car_id = @carId
                """, new { userId, carId }, cancellationToken: token));
        _logger.LogInformation("Rating from {UserId} for car {CarId} delete {OpResult}", userId, carId, result > 0 ? "success" : "fail");
        return result > 0;
    }

    public async Task<float?> GetRatingAsync(Guid carId, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        return await connection.QuerySingleOrDefaultAsync<float?>(new CommandDefinition($"""
            select round(avg(r.rating), 1) from {DbConstants.RatingsTableName}
            where car_id = @carId
            """, new { carId }, cancellationToken: token));
    }

    public async Task<(float? Rating, int? UserRating)> GetRatingAsync(Guid userId, Guid carId, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        return await connection.QuerySingleOrDefaultAsync<(float?, int?)>(new CommandDefinition($"""
            select round(avg(rating), 1), 
                   (select rating 
                    from {DbConstants.RatingsTableName} 
                    where car_id = @carId 
                      and user_id = @userId
                    limit 1) 
            from {DbConstants.RatingsTableName}
            where car_id = @carId
            """, new { carId, userId }, cancellationToken: token));
    }

    public async Task<IEnumerable<CarRating>> GetRatingsForUserAsync(Guid userId, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        var result = await connection.QueryAsync(new CommandDefinition($"""
            select rating, car_id, user_id
            from {DbConstants.RatingsTableName}             
            where user_id = @userId
            """, new { userId }, cancellationToken: token));
        return result.Select(r => new CarRating()
        {
            UserId = r.user_id,
            CarId = r.car_id,
            Rating = r.rating,
        });
    }


}
