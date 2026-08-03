using System.Text.Json;
using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.CardService.Clients;

public class CustomerClient
{
    private readonly HttpClient _httpClient;

    public CustomerClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ServiceResult<bool>> CustomerExistsAsync(
        long customerId)
    {
        using var response = await _httpClient.GetAsync(
            $"/api/customer/{customerId}/exists"
        );

        if (!response.IsSuccessStatusCode)
        {
            ErrorResponse? errorResponse = null;

            try
            {
                errorResponse =
                    await response.Content.ReadFromJsonAsync<ErrorResponse>();
            }
            catch (JsonException)
            {
            }

            return ServiceResult<bool>.Failure(
                errorResponse?.Error ?? Errors.CustomerClientError,
                (int)response.StatusCode
            );
        }

        bool exists;

        try
        {
            exists = await response.Content.ReadFromJsonAsync<bool>();
        }
        catch (JsonException)
        {
            throw new GeneralException(
                Errors.CustomerServiceResponseError,
                StatusCodes.Status502BadGateway
            );
        }

        return ServiceResult<bool>.Success(exists);
    }
    
}