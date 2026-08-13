using System.Text.Json;
using Bank.CustomerService.Models.ClientModels;
using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.CustomerService.Clients;

public class AuthorizationClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthorizationClient> _logger;

    public AuthorizationClient(HttpClient httpClient, ILogger<AuthorizationClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ServiceResult<CreateSpendingLimitResponse>>
        AddSpendingLimitAsync(
            CreateSpendingLimitRequest request,
            CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/spending-limit",
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

            return ServiceResult<CreateSpendingLimitResponse>.Failure(
                errorResponse?.Error ?? Errors.SpendingLimitCreateError,
                (int)response.StatusCode
            );
        }

        CreateSpendingLimitResponse? result;

        try
        {
            result =
                await response.Content
                    .ReadFromJsonAsync<CreateSpendingLimitResponse>(
                        cancellationToken
                    );
        }
        catch (JsonException)
        {
            throw new GeneralException(
                Errors.AuthorizationServiceResponseError,
                StatusCodes.Status502BadGateway
            );
        }

        if (result is null)
        {
            throw new GeneralException(
                Errors.AuthorizationServiceResponseError,
                StatusCodes.Status502BadGateway
            );
        }

        return ServiceResult<CreateSpendingLimitResponse>.Success(result);
    }

    public async Task<ServiceResult<Unit>> CompensateCreateSpendingLimitAsync(long customerId)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/spending-limit/compensate-create-spending-limit",
            customerId
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
                errorResponse?.Error ?? Errors.AuthClientError,
                (int)response.StatusCode
            );
        }

        return ServiceResult<Unit>.Success(new Unit());
    }
}