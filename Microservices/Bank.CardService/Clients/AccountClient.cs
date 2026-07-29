using Bank.CardService.Models.Dtos.ClientDtos;

namespace Bank.CardService.Clients;

public class AccountClient
{
    private readonly HttpClient _httpClient;

    public AccountClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> CreateAccount(long customerId, string branchCode)
    {
        var body = new
        {
            customerId = customerId,
            branchCode = branchCode,
            status = "1"
        };
        
        using var response =  await _httpClient.PostAsJsonAsync(
            $"/api/account",
            body
        );

        response.EnsureSuccessStatusCode();
        var result = await response.Content
            .ReadFromJsonAsync<CreateAccountResponse>();

        return result?.AccountNo
               ?? throw new InvalidOperationException(
                   "AccountService must return a  valid account no."
               );
    }

}