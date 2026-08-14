using Bank.CampaignService.Data;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore; 

namespace Bank.CampaignService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. AppDbContext ve Oracle EF Core Konfigürasyonu
            // appsettings.json dosyasındaki "OracleDb" connection string'ini kullanır
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseOracle(builder.Configuration.GetConnectionString("OracleDb")));

            // 2. Controller ve API Servislerinin Eklemesi
            builder.Services.AddControllers();

            // 3. Swagger / OpenAPI Konfigürasyonu
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // 4. HTTP Request Pipeline (Middleware) Ayarları
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}