using System.Text.Json;
using Bank.CustomerService.Models.ClientModels;
using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.CustomerService.Clients;

public class AccountClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AccountClient> _logger;

    public AccountClient(HttpClient httpClient, ILogger<AccountClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ServiceResult<CreateChargeLimitResponse>>
        CreateChargeLimitAsync(
            CreateChargeLimitRequest request,
            CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/charge-limit",
            request,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
        {
            ErrorResponse? errorResponse = null;

            try
            {
                errorResponse =
                    await response.Content.ReadFromJsonAsync<ErrorResponse>(
                        cancellationToken
                    );
            }
            catch (JsonException)
            {
            }

            return ServiceResult<CreateChargeLimitResponse>.Failure(
                errorResponse?.Error ?? Errors.ChargeLimitCreateError,
                (int)response.StatusCode
            );
        }

        CreateChargeLimitResponse? result;

        try
        {
            result =
                await response.Content
                    .ReadFromJsonAsync<CreateChargeLimitResponse>(
                        cancellationToken
                    );
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

        return ServiceResult<CreateChargeLimitResponse>.Success(result);
    }
}