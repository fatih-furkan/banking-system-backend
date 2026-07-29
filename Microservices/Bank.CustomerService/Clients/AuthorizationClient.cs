using Bank.CustomerService.Models.ClientModels;
using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.CustomerService.Clients;

public class AuthorizationClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AccountClient> _logger;

    public AuthorizationClient(HttpClient httpClient, ILogger<AccountClient> logger)
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
            string errorBody = await response.Content.ReadAsStringAsync(
                cancellationToken
            );

            _logger.LogWarning(
                "Limit service failed to create spending limits. " +
                "StatusCode: {StatusCode}, Response: {ResponseBody}",
                response.StatusCode,
                errorBody
            );

            return ServiceResult<CreateSpendingLimitResponse>.Failure(
                Errors.SpendingLimitCreateError
            );
        }

        CreateSpendingLimitResponse? result =
            await response.Content.ReadFromJsonAsync<CreateSpendingLimitResponse>(
                cancellationToken
            );

        if (result is null)
        {
            throw new GeneralException(
                Errors.UnexpectedError
            );
        }

        return ServiceResult<CreateSpendingLimitResponse>.Success(result);
    }
}