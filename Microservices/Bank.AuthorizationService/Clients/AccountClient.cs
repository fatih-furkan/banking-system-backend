using Bank.AuthorizationService.Models.Dtos.ClientDtos;
using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.AuthorizationService.Clients;

public class AccountClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AccountClient> _logger;

    public AccountClient(HttpClient httpClient, ILogger<AccountClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<ServiceResult<AccountSaleResponse>> AccountSaleAsync(AccountSaleRequest request)
    {
        
        using var response =  await _httpClient.PostAsJsonAsync(
            $"/api/account/sale",
            request
        );
        
        if (!response.IsSuccessStatusCode)
        {
            string errorBody =
                await response.Content.ReadAsStringAsync();

            _logger.LogError(errorBody);
            return ServiceResult<AccountSaleResponse>.Failure(
                Errors.AccountSaleError
            );
        }

        AccountSaleResponse? result =
            await response.Content.ReadFromJsonAsync<AccountSaleResponse>(
            );

        if (result is null)
        {
            throw new GeneralException(
                Errors.UnexpectedError
            );
        }

        return ServiceResult<AccountSaleResponse>.Success(result);
    }
    
    public async Task<ServiceResult<long?>> GetCustomerIdAsync(string accountNo)
    {
        
        using var response =  await _httpClient.GetAsync(
            $"/api/account/{accountNo}"
        );
        
        if (!response.IsSuccessStatusCode)
        {
            string errorBody =
                await response.Content.ReadAsStringAsync();

            return ServiceResult<long?>.Failure(
                Errors.GetAccountError
            );
        }

        GetByAccountNoResponse? result =
            await response.Content.ReadFromJsonAsync<GetByAccountNoResponse>(
            );

        if (result is null)
        {
            throw new GeneralException(
                Errors.UnexpectedError
            );
        }

        return ServiceResult<long?>.Success(result.CustomerId);
    }
}