namespace Bank.AccountService.Clients;

public class CardClient
{
    private readonly HttpClient _httpClient;

    public CardClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> CardExistsAsync(string cardToken)
    {
        return await _httpClient.GetFromJsonAsync<bool>(
            $"/api/card/{cardToken}/exists"
        );
    }

    public async Task<bool> CardBelongsToCustomerAsync(string cardToken, long customerId)
    {
        return await _httpClient.GetFromJsonAsync<bool>(
            $"/api/card/belongs?cardToken={cardToken}&customerId={customerId}"
        );
    }
    
}