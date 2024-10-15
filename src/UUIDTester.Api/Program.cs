using Microsoft.Data.SqlClient;
using Dapper;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDbConnection>(_ => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

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
    .MapUUIDNextSQLDatabase()
    .WithTags("uuidnext-database");

app.MapGroup("/medo")
    .MapMedo()
    .WithTags("medo");

app.MapGroup("/uuid-extensions")
    .MapUuidExtensions()
    .WithTags("uuid-extensions");

app.Run();
