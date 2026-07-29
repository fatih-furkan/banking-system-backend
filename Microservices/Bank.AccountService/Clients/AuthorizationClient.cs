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

    public async Task<ServiceResult<Unit>> AssignStatusAsync(string guid, string status)
    {
        
        var body = new
        {
            Status = status
        };
        
        using var response = await _httpClient.PostAsJsonAsync(
            $"/api/authorization/{guid}/assign-status",
            body
        );
        
        if (!response.IsSuccessStatusCode)
        {
            string errorBody =
                await response.Content.ReadAsStringAsync();

            return ServiceResult<Unit>.Failure(
                Errors.AuthClientError
            );
        }

        return ServiceResult<Unit>.Success(new Unit());

    }
}