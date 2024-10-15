using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

record WeatherForecast(Guid Id, DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record WeatherInput(int limit = 5);

public static class RouteGroupBuilderExtensions
{
    private static readonly string[] Summaries = {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
    
    private const int MinTemperature = -20;
    private const int MaxTemperature = 55;
    
    private static RouteGroupBuilder MapUuidGeneration(this RouteGroupBuilder group, Func<Guid> generator)
    {
        group.MapPost("/", async (IDbConnection connection, WeatherInput input) =>
        {
            var forecast = Enumerable.Range(1, input.limit).Select(index =>
                    new WeatherForecast
                    (
                        generator(),
                        DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        Random.Shared.Next(MinTemperature, MaxTemperature),
                        Summaries[Random.Shared.Next(Summaries.Length)]
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
                        generator(),
                        DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        Random.Shared.Next(MinTemperature, MaxTemperature),
                        Summaries[Random.Shared.Next(Summaries.Length)]
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

    public static RouteGroupBuilder MapUUIDNext(this RouteGroupBuilder group) =>
        group.MapUuidGeneration(UUIDNext.Uuid.NewSequential);
    
    public static RouteGroupBuilder MapUUIDNextSQLDatabase(this RouteGroupBuilder group) =>
        group.MapUuidGeneration(() => UUIDNext.Uuid.NewDatabaseFriendly(UUIDNext.Database.SqlServer));

    public static RouteGroupBuilder MapMedo(this RouteGroupBuilder group) =>
        group.MapUuidGeneration(Medo.Uuid7.NewMsSqlUniqueIdentifier);
    
    public static RouteGroupBuilder MapUuidExtensions(this RouteGroupBuilder group) =>
        group.MapUuidGeneration(() => UuidExtensions.Uuid7.Guid());
    
}