using System.Text.Json;
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
    public async Task<ServiceResult<CreateAccountResponse>> CreateAccountAsync(
        long customerId,
        string branchCode)
    {
        var body = new
        {
            CustomerId = customerId,
            BranchCode = branchCode
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "/api/account",
            body
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

            return ServiceResult<CreateAccountResponse>.Failure(
                errorResponse?.Error ?? Errors.AccountCreateError,
                (int)response.StatusCode
            );
        }

        CreateAccountResponse? result;

        try
        {
            result = await response.Content
                .ReadFromJsonAsync<CreateAccountResponse>();
        }
        catch (JsonException)
        {
            throw new GeneralException(
                Errors.AccountServiceResponseError,
                StatusCodes.Status502BadGateway
            );
        }

        if (result is null)
        {
            throw new GeneralException(
                Errors.AccountServiceResponseError,
                StatusCodes.Status502BadGateway
            );
        }

        return ServiceResult<CreateAccountResponse>.Success(result);
    }
    
    public async Task<ServiceResult<bool>> AccountExistsAsync(
        string accountNo)
    {
        string encodedAccountNo = Uri.EscapeDataString(accountNo);

        using var response = await _httpClient.GetAsync(
            $"/api/account/{encodedAccountNo}/exists"
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
                errorResponse?.Error ?? Errors.AccountClientError,
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
                Errors.AccountServiceResponseError,
                StatusCodes.Status502BadGateway
            );
        }

        return ServiceResult<bool>.Success(exists);
    }

    public async Task<ServiceResult<Unit>> AssignStatusAsync(
        string accountNo,
        string status)
    {
        string encodedAccountNo = Uri.EscapeDataString(accountNo);

        var body = new
        {
            Status = status
        };

        using var response = await _httpClient.PatchAsJsonAsync(
            $"/api/account/{encodedAccountNo}/assign-status",
            body
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

            return ServiceResult<Unit>.Failure(
                errorResponse?.Error ?? Errors.AccountClientError,
                (int)response.StatusCode
            );
        }

        return ServiceResult<Unit>.Success(new Unit());
    }
}