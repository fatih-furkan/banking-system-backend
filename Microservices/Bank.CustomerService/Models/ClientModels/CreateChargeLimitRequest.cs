// CreateChargeLimitResponse nesnesinin istek (Request) karşılığıdır. Yani CustomerService, kart/limit servisinden yükleme limiti oluşturmasını isterken bu formatta veri gönderir.

using System.ComponentModel.DataAnnotations;

namespace Bank.CustomerService.Models.ClientModels;

public class CreateChargeLimitRequest
{
    [Required]
    public long? CustomerId { get; set; }
    
    [Required]
    public decimal? DailyLimit { get; set; }
    
    [Required]
    public decimal? MonthlyLimit { get; set; }
    
    [Required]
    public decimal? AnnualLimit { get; set; }
}