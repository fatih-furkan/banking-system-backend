using Bank.AccountService.Models.ClientModels;
using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.AccountService.Clients;

public class AuthorizationClient
{
    private readonly HttpClient _httpClient;

    public AuthorizationClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CreateAuthorizationResponse> CreateAuthorizationAsync(CreateAuthorizationRequest request)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/authorization",
            request
        );

        
        var responseBody = await response.Content.ReadAsStringAsync();
        
        response.EnsureSuccessStatusCode();

        var authorization =
            await response.Content.ReadFromJsonAsync<CreateAuthorizationResponse>();

        return authorization
               ?? throw new GeneralException(
                   Errors.AuthorizationServiceResponseError
               );
    }
}