using CarRent.Application.DataBase;
using CarRent.Application.Models;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace CarRent.Application.Repositories;

public class OrdersRepository : IOrdersRepository
{
    private const string tableName = "orders";
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<OrdersRepository> _logger;

    public OrdersRepository(IDbConnectionFactory connectionFactory,
        ILogger<OrdersRepository> logger)
    {
        _dbConnectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<bool> CancelByUserIdAsync(Guid userId, Guid orderId, CancellationToken token)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        var result = await connection.ExecuteAsync(new CommandDefinition($"""
            delete from {tableName}
            where id = @orderId 
            and user_id = @userId
            """, new { userId, orderId }, cancellationToken: token));
        _logger.LogInformation("User {UserId}, Order {OrderId} cancel {OpResult} ", userId, orderId, result > 0 ? "success" : "fail");
        return result > 0;
    }

    public async Task<bool> CreateAsync(Order order, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        var result = await connection.ExecuteAsync(new CommandDefinition($"""
            insert into {tableName}
            (id,user_id,car_id,date_from,date_to)
            values
            (@Id,@UserId,@CarId,@DateFrom,@DateTo)
            """, order, cancellationToken: token));
        _logger.LogInformation("Order {OrderId} create {OpResult}", order.Id, result > 0 ? "success" : "fail");
        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        var result = await connection.ExecuteAsync(new CommandDefinition($"""
            delete from {tableName}
            where id = @id
            """, new { id }, cancellationToken: token));
        _logger.LogInformation("Order {OrderId} delete {OpResult}", id, result > 0 ? "success" : "fail");
        return result > 0;
    }

    public async Task<bool> ExistsByCarIdAndDateAsync(Guid id, DateTime from, DateTime to, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition($"""
            select count(1) from {tableName}
            where 
            (car_id = @id and (
                (date_from >= @from and date_from <= @to) or
                (date_to >= @from and date_to <= @to) or
                (date_from <= @from and date_to >= @to)            
            ))
            """, new { id, from, to }, cancellationToken: token));
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition($"""
            select count(1) from {tableName} 
            where id = @id
            """, new { id }, cancellationToken: token));
    }

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        var result = await connection.QueryAsync<Order>(new CommandDefinition($"""
            select * from {tableName}
            """, cancellationToken: token));
        return result;
    }

    public async Task<IEnumerable<Order>> GetAllByUserIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        var result = await connection.QueryAsync(new CommandDefinition($"""
            select * from {tableName}
            where user_id = @id
            order by date_from asc
            """, new { id }, cancellationToken: token));
        return result.Select(r => new Order()
        { 
            Id = r.id,
            UserId = r.user_id,
            CarId = r.car_id,
            DateFrom = r.date_from,
            DateTo = r.date_to,
        });
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        var result = await connection.QuerySingleOrDefaultAsync<Order>(new CommandDefinition($"""
            select * from {tableName}
            where id = @id
            """, new { id }, cancellationToken: token));
        return result;
    }

    public async Task<bool> UpdateAsync(Order order, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        var transaction = connection.BeginTransaction();
        var deleted = await connection.ExecuteAsync(new CommandDefinition($"""
                delete from {tableName}
                where id = @id
                """,
            new { id = order.Id },
            cancellationToken: token));
        var result = await connection.ExecuteAsync(new CommandDefinition($"""
            insert into {tableName}
            (id,user_id,car_id,date_from,date_to)
            values
            (@Id,@UserId,@CarId,@DateFrom,@DateTo)
            """, order, cancellationToken: token));
        transaction.Commit();
        _logger.LogInformation("Order {OrderId} update {OpResult}", order.Id, result > 0 ? "success" : "fail");
        return result > 0;
    }
}
