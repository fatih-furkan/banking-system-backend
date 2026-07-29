namespace Bank.CardService.Clients;

public class CustomerClient
{
    private readonly HttpClient _httpClient;

    public CustomerClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> CustomerExistsAsync(long customerId)
    {
        return await _httpClient.GetFromJsonAsync<bool>(
            $"/api/customer/{customerId}/exists"
        );
    }
    
}