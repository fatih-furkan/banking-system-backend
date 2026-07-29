/*using Bank.AccountService.Models.ClientModels;
using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.AccountService.Clients;

public class LimitClient
{
    private readonly HttpClient _httpClient;

    public LimitClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ServiceResult<UseChargeLimitResponse>> UseChargeLimitAsync(UseChargeLimitRequest request)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/limit/use-charge-limit",
            request
        );

        
        var responseBody = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            string errorBody =
                await response.Content.ReadAsStringAsync();

            return ServiceResult<UseChargeLimitResponse>.Failure(
                Errors.AccountCreateError
            );
        }

        var result =
            await response.Content.ReadFromJsonAsync<UseChargeLimitResponse>();

        if (result is null)
        {
            throw new GeneralException(
                Errors.UseChargeLimitError
            );
        }

        return ServiceResult<UseChargeLimitResponse>.Success(result);
    }
    
    public async Task<ServiceResult<CompensateUseChargeLimitResponse>> CompensateUseChargeLimitAsync(UseChargeLimitRequest request)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/limit/compensate-use-charge-limit",
            request
        );

        
        var responseBody = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            string errorBody =
                await response.Content.ReadAsStringAsync();

            return ServiceResult<CompensateUseChargeLimitResponse>.Failure(
                Errors.AccountCreateError
            );
        }

        var result =
            await response.Content.ReadFromJsonAsync<CompensateUseChargeLimitResponse>();

        if (result is null)
        {
            throw new GeneralException(
                Errors.UseChargeLimitError
            );
        }

        return ServiceResult<CompensateUseChargeLimitResponse>.Success(result);
    }
}*/