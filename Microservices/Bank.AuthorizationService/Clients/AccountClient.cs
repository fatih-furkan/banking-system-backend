using Bank.AuthorizationService.Models.Dtos.ClientDtos;
using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.AuthorizationService.Clients;

public class AccountClient
{
    private readonly HttpClient _httpClient;

    public AccountClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
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
}