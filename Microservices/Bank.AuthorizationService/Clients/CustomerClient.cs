using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.AuthorizationService.Clients;

public class CustomerClient
{
    private readonly HttpClient _httpClient;

    public CustomerClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ServiceResult<bool>> CustomerExistsAsync(long customerId)
    {

        using var response = await _httpClient.GetAsync(
            $"/api/customer/{customerId}/exists"
        );

        if (!response.IsSuccessStatusCode)
        {
            return ServiceResult<bool>.Failure(
                Errors.CustomerClientError,
                (int)response.StatusCode
            );
        }

        bool exists = await response.Content.ReadFromJsonAsync<bool>();

        return ServiceResult<bool>.Success(exists);
    }
}