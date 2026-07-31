using System.Text.Json;
using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.AuthorizationService.Clients;

public class CardClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CardClient> _logger;

    public CardClient(HttpClient httpClient, ILogger<CardClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ServiceResult<string>> FindAccountNoByCardNoAsync(
        string cardNo)
    {
        using var response = await _httpClient.GetAsync(
            $"/api/card/find-account-no/{Uri.EscapeDataString(cardNo)}"
        );

        string responseBody = await response.Content.ReadAsStringAsync();

        _logger.LogInformation(
            "Card service response: Status={StatusCode}, ContentType={ContentType}, Body={Body}",
            response.StatusCode,
            response.Content.Headers.ContentType,
            responseBody
        );

        if (!response.IsSuccessStatusCode)
        {
            return ServiceResult<string>.Failure(
                Errors.CardClientError
            );
        }

        string? accountNo =
            JsonSerializer.Deserialize<string>(responseBody);

        if (string.IsNullOrWhiteSpace(accountNo))
        {
            return ServiceResult<string>.Failure(
                Errors.CardClientError
            );
        }

        return ServiceResult<string>.Success(accountNo);
    }
    
    public async Task<ServiceResult<bool>> CardExistsAsync(
        string cardToken)
    {
        string encodedCardToken = Uri.EscapeDataString(cardToken);

        using var response = await _httpClient.GetAsync(
            $"/api/card/{encodedCardToken}/exists"
        );

        if (!response.IsSuccessStatusCode)
        {
            return ServiceResult<bool>.Failure(
                Errors.CardClientError,
                (int)response.StatusCode
            );
        }

        bool exists = await response.Content.ReadFromJsonAsync<bool>();

        return ServiceResult<bool>.Success(exists);
    }

}