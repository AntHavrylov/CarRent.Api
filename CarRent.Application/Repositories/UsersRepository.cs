﻿using CarRent.Application.DataBase;
using CarRent.Application.Models;
using Dapper;
using Microsoft.Extensions.Logging;

namespace CarRent.Application.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<CarsRepository> _logger;

    public UsersRepository(IDbConnectionFactory dbConnectionFactory,
        ILogger<CarsRepository> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    public async Task<bool> CreateAsync(User user, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        var result = await connection.ExecuteAsync(new CommandDefinition($"""
            insert into {DbConstants.UsersTableName} (id,name,email)
            values (@id,@name,@email)
            """,user,cancellationToken: token));
        _logger.LogInformation("User {UserId} create {OpResult}",user.Id, result > 0 ? "success" : "fail");
        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token); 
        await connection.ExecuteAsync(new CommandDefinition($"""
            delete from {DbConstants.OrdersTableName}
            where user_id = @id;
            """, new { id }, cancellationToken: token));

        await connection.ExecuteAsync(new CommandDefinition($"""
            delete from { DbConstants.RatingsTableName }
            where user_id = @id;
            """, new { id} , cancellationToken: token));

        var result = await connection.ExecuteAsync(new CommandDefinition($"""
            delete from {DbConstants.UsersTableName}
            where id = @id;
            """, new { id} , cancellationToken: token));

        _logger.LogInformation("User {UserId} delete {OpResult}", id, result > 0 ? "success" : "fail");
        return result > 0;
    }

    public async Task<bool> ExistsByEmailAndIdAsync(Guid id, string email, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition($"""
            select count(1) from {DbConstants.UsersTableName}
            where email = @email
            and id != @id
            """,new { id, email} , cancellationToken:token ));
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition($"""
            select count(1) from {DbConstants.UsersTableName} 
            where id = @id
            """, new { id }, cancellationToken: token));
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        return await connection.QueryAsync<User>(new CommandDefinition($"""
            select * from {DbConstants.UsersTableName}
            """, cancellationToken: token));
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        var result = await connection.QuerySingleAsync<User>(new CommandDefinition($"""
            select * from {DbConstants.UsersTableName}
            where id = @id
            """, new { id }, cancellationToken: token));
        _logger.LogInformation("User {UserId} retrieve {OpResult}", id, result is not null ? "success" : "fail");
        return result;
    }

    public async Task<bool> UpdateAsync(User user, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
                
        var result = await connection.ExecuteAsync(new CommandDefinition($"""
            update {DbConstants.UsersTableName} 
            set name = @Name,email = @Email
            where id = @id
            """, user , cancellationToken: token));
        
        _logger.LogInformation("User {UserId} Update {OpResult}", user.Id, result > 0 ? "success" : "fail");
        return result > 0;
    }
}
