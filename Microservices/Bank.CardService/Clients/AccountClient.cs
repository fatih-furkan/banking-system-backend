using Bank.CardService.Models.Dtos.ClientDtos;
using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.CardService.Clients;

public class AccountClient
{
    private readonly HttpClient _httpClient;

    public AccountClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    //returns accountNo.
    public async Task<ServiceResult<CreateAccountResponse>> CreateAccountAsync(long customerId, string branchCode)
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
        
        if (!response.IsSuccessStatusCode)
        {
            string errorBody =
                await response.Content.ReadAsStringAsync();

            return ServiceResult<CreateAccountResponse>.Failure(
                Errors.AccountCreateError
            );
        }

        CreateAccountResponse? result =
            await response.Content.ReadFromJsonAsync<CreateAccountResponse>(
            );

        if (result is null)
        {
            throw new GeneralException(
                Errors.GetAccountError
            );
        }

        return ServiceResult<CreateAccountResponse>.Success(result);
    }
    
    public async Task<bool> AccountExistsAsync(string accountNo)
    {
        return await _httpClient.GetFromJsonAsync<bool>(
            $"/api/account/{accountNo}/exists"
        );
    }

    public async Task AssignStatusAsync(string accountNo, string status)
    {
        var body = new
        {
            Status = status
        };
        
        using var response = await _httpClient
            .PostAsJsonAsync($"/api/account/{accountNo}/assign-status", body);
        response.EnsureSuccessStatusCode();
        return;
    }
}