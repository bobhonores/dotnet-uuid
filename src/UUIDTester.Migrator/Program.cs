using System.Net.Sockets;
using System.Reflection;
using DbUp;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Polly;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .AddCommandLine(args)
    .Build();

var database = configuration["database"] ?? "sqlserver";

switch (database)
{
    case "sqlserver":
        Policy
            .Handle<SocketException>()
            .WaitAndRetry([
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)
            ])
            .Execute(() =>
            {
                var connectionString = configuration.GetConnectionString("SqlConnection");
                
                EnsureDatabase.For.SqlDatabase(connectionString);

                var result = DeployChanges.To
                    .SqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), filter => filter.Contains("mssql_scripts"))
                    .LogToConsole()
                    .Build();
    
                var outcome = result.PerformUpgrade();

                if (!outcome.Successful)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(outcome.Error);
                    Console.ResetColor();
                    return -1;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Success");
                Console.ResetColor();
                return 0;
            });
        break;
    case "postgres":
        Policy
            .Handle<SocketException>(ex => ex.Message.Contains("No connection could be made"))
            .WaitAndRetry([
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)
            ])
            .Execute(() =>
            {
                var connectionString = configuration.GetConnectionString("PostgresConnection");
        
                EnsureDatabase.For.PostgresqlDatabase(connectionString);

                var result = DeployChanges.To
                    .PostgresqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), filter => filter.Contains("postgres_scripts"))
                    .LogToConsole()
                    .Build();
    
                var outcome = result.PerformUpgrade();

                if (!outcome.Successful)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(outcome.Error);
                    Console.ResetColor();
                    return -1;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Success");
                Console.ResetColor();
                return 0;
            });
        break;
    default:
        throw new ArgumentException($"Unsupported database: {database}");
}


    