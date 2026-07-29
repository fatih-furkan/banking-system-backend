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
        AddChargeLimitAsync(
            CreateChargeLimitRequest request,
            CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/limit",
            request,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
        {
            string errorBody = await response.Content.ReadAsStringAsync(
                cancellationToken
            );

            _logger.LogWarning(
                "Limit service failed to create charge limits. " +
                "StatusCode: {StatusCode}, Response: {ResponseBody}",
                response.StatusCode,
                errorBody
            );

            return ServiceResult<CreateChargeLimitResponse>.Failure(
                Errors.ChargeLimitCreateError
            );
        }

        CreateChargeLimitResponse? result =
            await response.Content.ReadFromJsonAsync<CreateChargeLimitResponse>(
                cancellationToken
            );

        if (result is null)
        {
            throw new GeneralException(
                Errors.UnexpectedError
            );
        }

        return ServiceResult<CreateChargeLimitResponse>.Success(result);
    }
}