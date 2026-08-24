using System.Text.Json.Serialization;
using Bank.CardService.Clients;
using Bank.CardService.Data;
using Bank.CardService.Sagas;
using Bank.CardService.Services;
using Bank.CardService.Services.Internal;
using Bank.Shared;
using Bank.Shared.Constants;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

builder.Services.AddScoped<CardService>();
builder.Services.AddScoped<CreateCardSaga>();
builder.Services.AddScoped<CardCreator>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
    });

builder.Services.AddHttpClient<CustomerClient>(client =>
{
    string baseAddress =
        builder.Configuration["Services:CustomerService"]
        ?? throw new InvalidOperationException(
            Constants.ExceptionMessages.CustomerServiceUrlError
        );

    client.BaseAddress = new Uri(baseAddress);
});

builder.Services.AddHttpClient<AccountClient>(client =>
{
    string baseAddress =
        builder.Configuration["Services:AccountService"]
        ?? throw new InvalidOperationException(
            Constants.ExceptionMessages.AccountServiceUrlError
        );

    client.BaseAddress = new Uri(baseAddress);
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.Run();
