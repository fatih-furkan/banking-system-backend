using System.Text.Json;
using Bank.AuthorizationService.Models.Dtos;
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
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/account/sale",
            request
        );

        if (!response.IsSuccessStatusCode)
        {
            string errorBody =
                await response.Content.ReadAsStringAsync();

            _logger.LogWarning(
                "Account sale failed. StatusCode: {StatusCode}, Response: {ResponseBody}",
                response.StatusCode,
                errorBody
            );
            
            ErrorResponse? errorResponse =
                JsonSerializer.Deserialize<ErrorResponse>(
                    errorBody,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                );

            return ServiceResult<AccountSaleResponse>.Failure(
                errorResponse?.Error ?? Errors.AccountSaleError,
                (int)response.StatusCode
            );
        }

        AccountSaleResponse? accountSaleResponse;

        try
        {
            accountSaleResponse =
                await response.Content
                    .ReadFromJsonAsync<AccountSaleResponse>();
        }
        catch (JsonException exception)
        {
            throw new GeneralException(
                Errors.UnexpectedError,
                StatusCodes.Status502BadGateway
            );
        }

        if (accountSaleResponse is null)
        {
            throw new GeneralException(
                Errors.UnexpectedError,
                StatusCodes.Status502BadGateway
            );
        }

        return ServiceResult<AccountSaleResponse>.Success(
            accountSaleResponse
        );
    }
    
    public async Task<ServiceResult<CompensateAccountSaleResponse>>
        AccountSaleCompensateAsync(CompensateAccountSaleRequest request)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/account/compensate-sale",
            request
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

            return ServiceResult<CompensateAccountSaleResponse>.Failure(
                errorResponse?.Error ?? Errors.AccountSaleError,
                (int)response.StatusCode
            );
        }

        CompensateAccountSaleResponse? result;

        try
        {
            result =
                await response.Content
                    .ReadFromJsonAsync<CompensateAccountSaleResponse>();
        }
        catch (JsonException)
        {
            throw new GeneralException(
                Errors.UnexpectedError,
                StatusCodes.Status502BadGateway
            );
        }

        if (result is null)
        {
            throw new GeneralException(
                Errors.UnexpectedError,
                StatusCodes.Status502BadGateway
            );
        }

        return ServiceResult<CompensateAccountSaleResponse>.Success(result);
    }
    
    public async Task<ServiceResult<long?>> GetCustomerIdAsync(
        string accountNo)
    {
        string encodedAccountNo = Uri.EscapeDataString(accountNo);

        using var response = await _httpClient.GetAsync(
            $"/api/account/{encodedAccountNo}"
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

            return ServiceResult<long?>.Failure(
                errorResponse?.Error ?? Errors.GetAccountError,
                (int)response.StatusCode
            );
        }

        GetByAccountNoResponse? result;

        try
        {
            result =
                await response.Content
                    .ReadFromJsonAsync<GetByAccountNoResponse>();
        }
        catch (JsonException)
        {
            throw new GeneralException(
                Errors.UnexpectedError,
                StatusCodes.Status502BadGateway
            );
        }

        if (result is null)
        {
            throw new GeneralException(
                Errors.UnexpectedError,
                StatusCodes.Status502BadGateway
            );
        }

        return ServiceResult<long?>.Success(
            result.CustomerId
        );
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
    
    public async Task<ServiceResult<AccountRefundResponse>> AccountRefundAsync(
        AccountRefundRequest request)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/account/refund",
            request
        );

        if (!response.IsSuccessStatusCode)
        {
            string errorBody =
                await response.Content.ReadAsStringAsync();

            _logger.LogWarning(
                "Account refund failed. StatusCode: {StatusCode}, Response: {ResponseBody}",
                response.StatusCode,
                errorBody
            );
            
            ErrorResponse? errorResponse =
                JsonSerializer.Deserialize<ErrorResponse>(
                    errorBody,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                );

            return ServiceResult<AccountRefundResponse>.Failure(
                errorResponse?.Error ?? Errors.AccountRefundCompensationError,
                (int)response.StatusCode
            );
        }

        AccountRefundResponse? accountRefundResponse;

        try
        {
            accountRefundResponse =
                await response.Content
                    .ReadFromJsonAsync<AccountRefundResponse>();
        }
        catch (JsonException exception)
        {
            throw new GeneralException(
                Errors.UnexpectedError,
                StatusCodes.Status502BadGateway
            );
        }

        if (accountRefundResponse is null)
        {
            throw new GeneralException(
                Errors.UnexpectedError,
                StatusCodes.Status502BadGateway
            );
        }

        return ServiceResult<AccountRefundResponse>.Success(
            accountRefundResponse
        );
    }

    public async Task<ServiceResult<AccountRefundResponse>> AccountRefundCompensateAsync(
        CompensateAccountRefundRequest request)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/account/compensate-refund",
            request
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

            return ServiceResult<AccountRefundResponse>.Failure(
                errorResponse?.Error ?? Errors.AccountRefundCompensationError,
                (int)response.StatusCode
            );
        }

        AccountRefundResponse? result;

        try
        {
            result =
                await response.Content
                    .ReadFromJsonAsync<AccountRefundResponse>();
        }
        catch (JsonException)
        {
            throw new GeneralException(
                Errors.UnexpectedError,
                StatusCodes.Status502BadGateway
            );
        }

        if (result is null)
        {
            throw new GeneralException(
                Errors.UnexpectedError,
                StatusCodes.Status502BadGateway
            );
        }

        return ServiceResult<AccountRefundResponse>.Success(result);
    }
}