using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Npgsql;
using NpgsqlTypes;

public interface IStorage
{
    Task Insert(IEnumerable<WeatherForecast> forecasts);
    Task BulkInsert(IEnumerable<WeatherForecast> forecasts);
}

public class SqlStorage(SqlConnection connection) : IStorage
{
    public async Task Insert(IEnumerable<WeatherForecast> forecasts)
    {
        await connection.ExecuteAsync(
            "INSERT INTO WeatherData (id, date, temperature, summary) VALUES (@Id, @Date, @TemperatureC, @Summary)",
            forecasts);
    }

    public async Task BulkInsert(IEnumerable<WeatherForecast> forecasts)
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("id", typeof(Guid));
        dataTable.Columns.Add("date", typeof(DateOnly));
        dataTable.Columns.Add("temperature", typeof(int));
        dataTable.Columns.Add("summary", typeof(string));

        foreach (var item in forecasts)
        {
            dataTable.Rows.Add(item.Id, item.Date, item.TemperatureC, item.Summary);
        }

        // Write to sql database
        await connection.OpenAsync();
        using var bulk = new SqlBulkCopy(connection);

        bulk.DestinationTableName = "WeatherData";
        bulk.BatchSize = 1000;
        bulk.ColumnMappings.Add("id", "id");
        bulk.ColumnMappings.Add("date", "date");
        bulk.ColumnMappings.Add("temperature", "temperature");
        bulk.ColumnMappings.Add("summary", "summary");
        await bulk.WriteToServerAsync(dataTable);
    }
}

public class PostgresStorage(NpgsqlConnection connection) : IStorage
{
    public async Task Insert(IEnumerable<WeatherForecast> forecasts)
    {
        await connection.ExecuteAsync(
            "insert into weather_data (id, date, temperature, summary) values (@Id, @Date, @TemperatureC, @Summary)",
            forecasts);
    }

    public async Task BulkInsert(IEnumerable<WeatherForecast> forecasts)
    {
        await connection.OpenAsync();
        
        await using var writer =
            await connection.BeginBinaryImportAsync("copy weather_data from stdin (format binary)");
        foreach (var forecast in forecasts)
        {
            await writer.StartRowAsync();
            await writer.WriteAsync(forecast.Id, NpgsqlDbType.Uuid);
            await writer.WriteAsync(forecast.Date, NpgsqlDbType.Date);
            await writer.WriteAsync(forecast.TemperatureC, NpgsqlDbType.Numeric);
            await writer.WriteAsync(forecast.Summary, NpgsqlDbType.Varchar);
        }

        await writer.CompleteAsync();
    }
}