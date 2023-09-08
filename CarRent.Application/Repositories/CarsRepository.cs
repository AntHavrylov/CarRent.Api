﻿using CarRent.Application.DataBase;
using CarRent.Application.Models;
using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace CarRent.Application.Repositories
{
    public class CarsRepository : ICarsRepository
    {
        private const string tableName = "cars";
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly ILogger<CarsRepository> _logger;

        public CarsRepository(IDbConnectionFactory dbConnectionFactory, 
            ILogger<CarsRepository> logger)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _logger = logger;
        }

        public async Task<bool> CreateAsync(Car car, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            var result = await connection.ExecuteAsync(
                new CommandDefinition($"""
                insert into {tableName} (id, yearOfProduction, brand, model, slug, engineType, bodyType)
                values(@Id,@YearOfProduction,@Brand, @Model, @Slug, @EngineType, @BodyType)
                """, car, cancellationToken: token));

            _logger.LogInformation("Car '{CarSlug}' with id {CarId} create {result}",
                car.Slug, car.Id, result > 0 ? "success" : "fail");
            return result > 0;
        }
        
        public async Task<bool> UpdateAsync(Car car, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            var transaction = connection.BeginTransaction();
            var deleted = await connection.ExecuteAsync(new CommandDefinition($"""
                delete from {tableName}
                where id = @id
                """,
                new {id = car.Id},
                cancellationToken: token));
            var result = await connection.ExecuteAsync(
                new CommandDefinition($"""
                insert into {tableName} (id, yearOfProduction, brand, model, slug, engineType, bodyType)
                values(@Id,@YearOfProduction,@Brand, @Model, @Slug, @EngineType, @BodyType)
                """, car, cancellationToken: token));
            transaction.Commit();
            _logger.LogInformation("Car '{CarSlug}' with id {CarId} update {result}",
                car.Slug, car.Id, result > 0 ? "success" : "fail");
            return result > 0;
        }
           
        public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.ExecuteScalarAsync<bool>(new CommandDefinition($"""
            select count(1) from {tableName} where id = @id
            """, new { id }, cancellationToken: token));
        }

        public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            var result = await connection.ExecuteAsync(new CommandDefinition($"""
                delete from {tableName}
                where id = @id
                """, new { id }, cancellationToken: token));
            _logger.LogInformation("Car with id {CarId} delete {result}",
                id, result > 0 ? "success" : "fail");
            return result > 0;
        }

        public async Task<IEnumerable<Car>> GetAllAsync(GetAllCarsOptions options, CancellationToken token = default)
        {
            var orderClause = string.Empty;
            if (options.SortField is not null)
            {
                orderClause = $"""
                    order by {options.SortField} {(options.SortOrder == SortOrder.Ascending ? "asc" : "desc")}
                    """;
            }

            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            var result = await connection.QueryAsync(new CommandDefinition($"""
                select * from {tableName}             
                where (@slug is null or slug like ('%' || @slug || '%')) 
                and (@yearOfProduction is null or yearofproduction = @yearOfProduction)
                {orderClause}
                limit @pageSize
                offset @pageOffset
                """, new 
                {
                    slug = options.Slug,
                    yearOfProduction = options.YearOfProduction,
                    pageOffset = (options.Page -1 )* options.PageSize,
                    pageSize = options.PageSize
                },
                cancellationToken: token));

            return result.Select(c => new Car() 
            {
                Id = c.id, 
                YearOfProduction = c.yearofproduction,
                Brand = c.brand,
                Model = c.model,
                EngineType = (EngineType)c.enginetype,
                BodyType = (BodyType)c.bodytype,
            });
        }

        public async Task<Car?> GetByIdAsync(Guid id, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);                   

            var result = await connection.QuerySingleAsync<Car>(new CommandDefinition($"""
                select * from {tableName}
                where id = @id
                """
                , new { id }, cancellationToken: token));
            _logger.LogInformation("Car id {CarId} retrieved", id);
            return result;
        }

        public async Task<int> GetCountAsync(GetAllCarsOptions options, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            var result = await connection.QuerySingleAsync<int>(new CommandDefinition($"""
                select count(1) from {tableName}
                where (@slug is null or slug like ('%' || @slug || '%')) 
                and (@yearOfProduction is null or yearofproduction = @yearOfProduction)
                """, 
                new { slug = options.Slug , yearOfProduction = options.YearOfProduction },
                cancellationToken: token));
            return (int)result;
        }
               
    }
}
