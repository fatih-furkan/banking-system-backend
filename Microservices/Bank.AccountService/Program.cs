using Bank.AccountService.Clients;
using Bank.AccountService.Data;
using Bank.AccountService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseOracle(
        builder.Configuration.GetConnectionString("OracleDb"),
        oracleOptions =>
        {
            oracleOptions.UseOracleSQLCompatibility(
                OracleSQLCompatibility.DatabaseVersion21
            );
        }
    );
});

builder.Services.AddScoped<AccountService>();

builder.Services.AddHttpClient<CustomerClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:7004");
});

builder.Services.AddHttpClient<CardClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:7002");
});

builder.Services.AddHttpClient<AuthorizationClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:7006");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
