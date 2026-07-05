using CarRent.Application.DataBase;
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
                insert into {DbConstants.CarsTableName} (id, yearOfProduction, brand, model, slug, engineType, bodyType)
                values(@Id,@YearOfProduction,@Brand, @Model, @Slug, @EngineType, @BodyType)
                """, car, cancellationToken: token));

            _logger.LogInformation("Car '{CarSlug}' with id {CarId} create {result}",
                car.Slug, car.Id, result > 0 ? "success" : "fail");
            return result > 0;
        }
        
        public async Task<bool> UpdateAsync(Car car, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
                        
            var result = await connection.ExecuteAsync(
                new CommandDefinition($"""
                update {DbConstants.CarsTableName}
                set yearOfProduction = @YearOfProduction,brand = @Brand,
                    model = @Model, slug = @Slug, engineType = @EngineType, bodyType = @BodyType                
                where id = @Id
                """, car, cancellationToken: token));
            
            _logger.LogInformation("Car '{CarSlug}' with id {CarId} update {result}",
                car.Slug, car.Id, result > 0 ? "success" : "fail");
            return result > 0;
        }
           
        public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.ExecuteScalarAsync<bool>(new CommandDefinition($"""
            select count(1) from {DbConstants.CarsTableName} where id = @id
            """, new { id }, cancellationToken: token));
        }

        public async Task<bool> ExistsBySlugAndIdAsync(Guid id, string slug, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.ExecuteScalarAsync<bool>(new CommandDefinition($"""
            select count(1) from {DbConstants.CarsTableName}
            where slug = @slug
            and id != @id
            """, new { id, slug }, cancellationToken: token));
        }

        public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            using var transaction = connection.BeginTransaction();

            await connection.ExecuteAsync(new CommandDefinition($""""
                delete from {DbConstants.OrdersTableName}
                where car_id = @id;
                """", new { id }, transaction, cancellationToken: token));

            await connection.ExecuteAsync(new CommandDefinition($"""
                delete from {DbConstants.RatingsTableName}
                where car_id = @id;
                """, new { id }, transaction, cancellationToken: token));

            var result = await connection.ExecuteAsync(new CommandDefinition($"""
                delete from {DbConstants.CarsTableName}
                where id = @id;
                """, new { id }, transaction, cancellationToken: token));

            transaction.Commit();

            _logger.LogInformation("Car with id {CarId} delete {result}",
                id, result > 0 ? "success" : "fail");
            return result > 0;
        }

        private static readonly Dictionary<string, string> AllowedSortColumns = new(StringComparer.OrdinalIgnoreCase)
        {
            ["id"] = "id",
            ["yearofproduction"] = "yearofproduction",
            ["brand"] = "brand",
            ["model"] = "model",
            ["slug"] = "slug",
            ["enginetype"] = "enginetype",
            ["bodytype"] = "bodytype",
            ["rating"] = "rating",
        };

        public static string? ResolveSortColumn(string? sortField) =>
            sortField is not null && AllowedSortColumns.TryGetValue(sortField, out var column)
                ? column
                : null;

        public async Task<IEnumerable<Car>> GetAllAsync(GetAllCarsOptions options, CancellationToken token = default)
        {
            var orderClause = string.Empty;
            var sortColumn = ResolveSortColumn(options.SortField);
            if (sortColumn is not null)
            {
                orderClause = $"""
                    order by {sortColumn} {(options.SortOrder == SortOrder.Ascending ? "asc" : "desc")}
                    """;
            }

            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            var result = await connection.QueryAsync(new CommandDefinition($"""
                select c.*, round(avg(r.rating), 1) as rating
                from {DbConstants.CarsTableName} c
                left join {DbConstants.RatingsTableName} r on r.car_id = c.id
                where (@slug is null or slug like ('%' || @slug || '%')) 
                and (@yearOfProduction is null or yearofproduction = @yearOfProduction)
                group by id
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
                Rating = (float?) c.rating,
                EngineType = (EngineType)c.enginetype,
                BodyType = (BodyType)c.bodytype,
            });
        }

        public async Task<Car?> GetByIdAsync(Guid id, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

            var result = await connection.QuerySingleOrDefaultAsync(new CommandDefinition($"""
                select c.*, round(avg(r.rating), 1) as rating
                from {DbConstants.CarsTableName} c
                left join {DbConstants.RatingsTableName} r on r.car_id = c.id
                where c.id = @id
                group by c.id
                """
                , new { id }, cancellationToken: token));
            _logger.LogInformation("Car id {CarId} retrieved", id);

            return result is null ? null : new Car()
            {
                Id = result.id,
                YearOfProduction = result.yearofproduction,
                Brand = result.brand,
                Model = result.model,
                Rating = (float?)result.rating,
                EngineType = (EngineType)result.enginetype,
                BodyType = (BodyType)result.bodytype,
            };
        }

        public async Task<int> GetCountAsync(GetAllCarsOptions options, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            var result = await connection.QuerySingleAsync<int>(new CommandDefinition($"""
                select count(1) from {DbConstants.CarsTableName}
                where (@slug is null or slug like ('%' || @slug || '%')) 
                and (@yearOfProduction is null or yearofproduction = @yearOfProduction)
                """, 
                new { slug = options.Slug , yearOfProduction = options.YearOfProduction },
                cancellationToken: token));
            return (int)result;
        }
               
    }
}
