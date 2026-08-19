using Bank.AuthorizationService.Clients;
using Bank.AuthorizationService.Data;
using Bank.AuthorizationService.Sagas;
using Bank.AuthorizationService.Services;
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

builder.Services.AddHttpClient<CustomerClient>(client =>
{
    string baseAddress =
        builder.Configuration["Services:CustomerService"]
        ?? throw new InvalidOperationException(
            Constants.ExceptionMessages.CustomerServiceUrlError
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

builder.Services.AddHttpClient<AccountClient>(client =>
{
    string baseAddress =
        builder.Configuration["Services:AccountService"]
        ?? throw new InvalidOperationException(
            Constants.ExceptionMessages.AccountServiceUrlError
        );

    client.BaseAddress = new Uri(baseAddress);
});

builder.Services.AddHttpClient<PointClient>(client =>
{
    string baseAddress =
        builder.Configuration["Services:PointService"]
        ?? throw new InvalidOperationException(
            Constants.ExceptionMessages.AccountServiceUrlError
        );

    client.BaseAddress = new Uri(baseAddress);
});

builder.Services.AddHttpClient<CampaignClient>(client =>
{
    string baseAddress =
        builder.Configuration["Services:CampaignService"]
        ?? throw new InvalidOperationException(
            Constants.ExceptionMessages.CampaignServiceUrlError
        );

    client.BaseAddress = new Uri(baseAddress);
});


builder.Services.AddScoped<AuthorizationService>();
builder.Services.AddScoped<SpendingLimitService>();
builder.Services.AddScoped<SpendingLimitSaga>();
builder.Services.AddScoped<AccountSaleSaga>();
builder.Services.AddScoped<AccountRefundSaga>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
