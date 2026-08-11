using Bank.PointService.Clients;
using Bank.PointService.Data;
using Bank.PointService.Services;
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


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
