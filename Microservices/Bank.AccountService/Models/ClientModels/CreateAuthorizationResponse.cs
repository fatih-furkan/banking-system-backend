using Bank.Shared.Enums;

namespace Bank.AccountService.Models.ClientModels;

public class CreateAuthorizationResponse
{
    public DateTime? TransactionDate { get; set; }

    public decimal? Balance { get; set; }

    public decimal? TransactionAmount { get; set; }
}