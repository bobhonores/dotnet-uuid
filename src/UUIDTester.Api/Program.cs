using Dapper;
using Microsoft.Data.SqlClient;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<SqlConnection>(_ =>
    new SqlConnection(builder.Configuration.GetConnectionString("SqlConnection")));
builder.Services.AddScoped<NpgsqlConnection>(_ =>
    new NpgsqlConnection(builder.Configuration.GetConnectionString("PostgresConnection")));

builder.Services.AddScoped<SqlStorage>();
builder.Services.AddScoped<PostgresStorage>();

SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGroup("/uuidnext")
    .MapUUIDNext()
    .WithTags("uuidnext");

app.MapGroup("/uuidnext-database")
    .MapUUIDNextDatabase()
    .WithTags("uuidnext-database");

app.MapGroup("/medo")
    .MapMedo()
    .WithTags("medo");

app.MapGroup("/uuid-extensions")
    .MapUuidExtensions()
    .WithTags("uuid-extensions");

app.MapGroup("/nguid-v7")
    .MapNGuidv7()
    .WithTags("nguid-v7");

app.MapGroup("/nguid-v8")
    .MapNGuidv8()
    .WithTags("nguid-v8");

app.Run();
