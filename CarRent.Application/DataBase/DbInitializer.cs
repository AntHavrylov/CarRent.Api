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
                create table if not exists products(
                id UUID primary key,
                name TEXT not null,
                description TEXT);
                """);

        await connection.ExecuteAsync("""
                create unique index concurrently if not exists products_name_idx
                on products
                using btree(name);
                """);

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
    }
}
