using Bank.PointService.Clients;
using Bank.PointService.Data;
using Bank.PointService.Services;
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

builder.Services.AddScoped<PointService>();

builder.Services.AddHttpClient<CustomerClient>(client =>
{
    string baseAddress =
        builder.Configuration["Services:CustomerService"]
        ?? throw new InvalidOperationException(
            Constants.ExceptionMessages.CustomerServiceUrlError
        );

    client.BaseAddress = new Uri(baseAddress);
});

builder.Services.AddHttpClient<AuthorizationClient>(client =>
{
    string baseAddress =
        builder.Configuration["Services:AuthorizationService"]
        ?? throw new InvalidOperationException(
            Constants.ExceptionMessages.AuthorizationServiceUrlError
        );

    client.BaseAddress = new Uri(baseAddress);
});

builder.Services.AddHttpClient<CardClient>(client =>
{
    string baseAddress =
        builder.Configuration["Services:CardService"]
        ?? throw new InvalidOperationException(
            Constants.ExceptionMessages.CardServiceUrlError
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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.Run();
