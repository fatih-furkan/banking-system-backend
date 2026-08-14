// Response, return class.

namespace Bank.CustomerService.Models.ClientModels;

public class CreateSpendingLimitResponse
{
    public long CustomerId { get; set; }         // Kaydedilen müşterinin adını, soyadını, TC kimlik numarasını ve durumunu döner(sonuç verisini teslim etmesi) .  Ne yapıyor? En kritik alanlardan biri. Veritabanında otomatik oluşan (Identity / Auto-increment) benzersiz müşteri numarasını (Primary Key) istemciye bildirir.
    public decimal DailyLimit { get; set; }
    public decimal MonthlyLimit { get; set; }
    public decimal AnnualLimit { get; set; }
    
    public decimal CurrentDailyLimit { get; set; }
    public decimal CurrentMonthlyLimit { get; set; }
    public decimal CurrentAnnualLimit { get; set; }
}