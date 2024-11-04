record Input(int limit = 5);

public record WeatherForecast(Guid Id, DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public static class BaseUuidRouterExtensions
{
    private static readonly string[] Summaries =
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private const int MinTemperature = -20;
    private const int MaxTemperature = 55;

    internal static RouteGroupBuilder MapDbUuidGenerator(this RouteGroupBuilder group, Func<Guid> sqlGenerator,
        Func<Guid>? postgresGenerator = null)
    {
        group.MapGroup("/sql-server")
            .MapUuidGenerator<SqlStorage>(sqlGenerator);

        group.MapGroup("/postgres")
            .MapUuidGenerator<PostgresStorage>(postgresGenerator ?? sqlGenerator);

        return group;
    }

    private static RouteGroupBuilder MapUuidGenerator<T>(this RouteGroupBuilder group, Func<Guid> generator)
        where T : IStorage
    {
        IEnumerable<WeatherForecast> Data(int limit, Func<Guid> uuidGenerator) 
        {
            var rnd = new Random();
            return Enumerable.Range(1, limit).Select(index =>
            {
                var rndDays = rnd.Next(0, index);
                var daysToAdd = Math.Min(Math.Max(rndDays, 0), 720);
                return new WeatherForecast
                (
                    uuidGenerator(),
                    DateOnly.FromDateTime(DateTime.Now.AddDays(daysToAdd)),
                    Random.Shared.Next(MinTemperature, MaxTemperature),
                    Summaries[Random.Shared.Next(Summaries.Length)]
                );
            });
        }
            

        group.MapPost("/", async (T storage, Input input) =>
        {
            var forecastData = Data(input.limit, generator);

            await storage.Insert(forecastData);

            return Results.Ok();
        });

        group.MapPost("/bulk", async (T storage, Input input) =>
        {
            var forecastData = Data(input.limit, generator);

            await storage.BulkInsert(forecastData);

            return Results.Ok();
        });

        return group;
    }
}