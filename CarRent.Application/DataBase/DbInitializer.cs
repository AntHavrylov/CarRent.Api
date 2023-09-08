using Dapper;

namespace CarRent.Application.DataBase;

public class DbInitializer
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public DbInitializer(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task InitializeAsync(CancellationToken token = default) 
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        await connection.ExecuteAsync("""
            create table if not exists cars(
            id UUID primary key,
            yearOfProduction SMALLINT not null,
            brand TEXT not null,
            model TEXT not null,
            slug TEXT not null,
            engineType SMALLINT not null,
            bodyType SMALLINT not null);
            """);

        await connection.ExecuteAsync("""
                create unique index concurrently if not exists cars_slug_idx
                on cars
                using btree(slug);
                """);

        await connection.ExecuteAsync("""
                create table if not exists users(
                id UUID primary key,
                name VARCHAR(30) not null,
                Email VARCHAR(30) not null);
                """);
    }
}
