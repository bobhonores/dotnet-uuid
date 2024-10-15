using Microsoft.Data.SqlClient;
using Dapper;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDbConnection>(_ => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGroup("/uuidnext")
    .MapUUIDNext()
    .WithTags("uuidnext");

app.MapGroup("/medo")
    .MapMedo()
    .WithTags("medo");

app.Run();

record WeatherForecast(Guid Id, DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record WeatherInput(int limit = 5);

public static class RouteGroupBuilderExtensions
{
    static string[] summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public static RouteGroupBuilder MapUUIDNext(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (IDbConnection connection, WeatherInput input) =>
        {
            var forecast = Enumerable.Range(1, input.limit).Select(index =>
                new WeatherForecast
                (
                    UUIDNext.Uuid.NewSequential(),
                    // UUIDNext.Uuid.NewDatabaseFriendly(Database.SqlServer),
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();

            // Write to sql database
            await connection.ExecuteAsync("INSERT INTO WeatherData (id, date, temperature, summary) VALUES (@Id, @Date, @TemperatureC, @Summary)", forecast);

            return Results.Ok();
        });

        group.MapPost("/bulk", async (IDbConnection connection, WeatherInput input) =>
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("id", typeof(Guid));
            dataTable.Columns.Add("date", typeof(DateOnly));
            dataTable.Columns.Add("temperature", typeof(int));
            dataTable.Columns.Add("summary", typeof(string));

            var forecast = Enumerable.Range(1, input.limit).Select(index =>
                new WeatherForecast
                (
                    // UUIDNext.Uuid.NewSequential(),
                    UUIDNext.Uuid.NewDatabaseFriendly(UUIDNext.Database.SqlServer),
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();

            foreach (var item in forecast)
            {
                dataTable.Rows.Add(item.Id, item.Date, item.TemperatureC, item.Summary);
            }

            // Write to sql database
            var sqlConnection = (SqlConnection)connection;
            await sqlConnection.OpenAsync();
            using var bulk = new SqlBulkCopy(sqlConnection);

            bulk.DestinationTableName = "WeatherData";
            bulk.BatchSize = 1000;
            bulk.ColumnMappings.Add("id", "id");
            bulk.ColumnMappings.Add("date", "date");
            bulk.ColumnMappings.Add("temperature", "temperature");
            bulk.ColumnMappings.Add("summary", "summary");
            await bulk.WriteToServerAsync(dataTable);

            return Results.Ok();
        });

        return group;
    }

    public static RouteGroupBuilder MapMedo(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (IDbConnection connection, WeatherInput input) =>
        {
            var forecast = Enumerable.Range(1, input.limit).Select(index =>
                new WeatherForecast
                (
                    Medo.Uuid7.NewMsSqlUniqueIdentifier(),
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();

            // Write to sql database
            await connection.ExecuteAsync("INSERT INTO WeatherData (id, date, temperature, summary) VALUES (@Id, @Date, @TemperatureC, @Summary)", forecast);

            return Results.Ok();
        });

        group.MapPost("/bulk", async (IDbConnection connection, WeatherInput input) =>
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("id", typeof(Guid));
            dataTable.Columns.Add("date", typeof(DateOnly));
            dataTable.Columns.Add("temperature", typeof(int));
            dataTable.Columns.Add("summary", typeof(string));

            var forecast = Enumerable.Range(1, input.limit).Select(index =>
                new WeatherForecast
                (
                    Medo.Uuid7.NewMsSqlUniqueIdentifier(),
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();

            foreach (var item in forecast)
            {
                dataTable.Rows.Add(item.Id, item.Date, item.TemperatureC, item.Summary);
            }

            // Write to sql database
            var sqlConnection = (SqlConnection)connection;
            await sqlConnection.OpenAsync();
            using var bulk = new SqlBulkCopy(sqlConnection);

            bulk.DestinationTableName = "WeatherData";
            bulk.BatchSize = 1000;
            bulk.ColumnMappings.Add("id", "id");
            bulk.ColumnMappings.Add("date", "date");
            bulk.ColumnMappings.Add("temperature", "temperature");
            bulk.ColumnMappings.Add("summary", "summary");
            await bulk.WriteToServerAsync(dataTable);

            return Results.Ok();
        });

        return group;
    }
}

public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value;
    }

    public override DateOnly Parse(object value)
    {
        return DateOnly.FromDateTime((DateTime)value);
    }
}