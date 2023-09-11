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
            create table if not exists users(
            id UUID primary key,
            name VARCHAR(30) not null,
            email VARCHAR(30) not null);
            """);        

        await connection.ExecuteAsync("""
            create table if not exists orders(
            id UUID primary key,
            user_id UUID not null,
            car_id UUID not null,
            date_from timestamp not null default current_date,
            date_to timestamp not null default DATE '9999-12-31');
            """);

        await connection.ExecuteAsync("""
            create table if not exists ratings (
            user_id uuid,
            car_id uuid,
            rating integer not null,
            primary key (user_id, car_id));
        """);

        await connection.ExecuteAsync("""
            insert into cars (id,yearOfProduction,brand,model,slug,engineType,bodyType)
            values ('25afc9c2-fbdc-408f-9d69-572f1fe7a7b3',2011,'Honda','Accord','honda-accord-2021',1,2)
            ON CONFLICT (id) DO NOTHING;
            """);

        await connection.ExecuteAsync("""
            insert into users (id, name, email)
            values ('e1b2fd4e-58bf-4265-af32-1b871b824625','Anton','ant.havrylov@gmail.com')
            ON CONFLICT (id) DO NOTHING;
            """);
    }
}
