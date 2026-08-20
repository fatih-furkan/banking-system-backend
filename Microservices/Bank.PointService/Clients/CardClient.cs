using System.Text.Json;
using Bank.PointService.Models.ClientModels;
using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.PointService.Clients;

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
                errorResponse?.Error ?? Errors.CardClientError,
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
                Errors.CardServiceResponseError,
                StatusCodes.Status502BadGateway
            );
        }

        return ServiceResult<bool>.Success(exists);
    }
    
    public async Task<ServiceResult<CardExistsByCardNoResponse>> CardExistsByCardNoAsync(
        string cardNo)
    {
        string encodedCardToken = Uri.EscapeDataString(cardNo);

        using var response = await _httpClient.GetAsync(
            $"/api/card/{encodedCardToken}/exists-card-no"
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

            return ServiceResult<CardExistsByCardNoResponse>.Failure(
                errorResponse?.Error ?? Errors.CardClientError,
                (int)response.StatusCode
            );
        }

        CardExistsByCardNoResponse? responseContent;

        try
        {
            responseContent = await response.Content.ReadFromJsonAsync<CardExistsByCardNoResponse>();
        }
        catch (JsonException)
        {
            throw new GeneralException(
                Errors.CardServiceResponseError,
                StatusCodes.Status502BadGateway
            );
        }

        return ServiceResult<CardExistsByCardNoResponse>.Success(responseContent);
    }

}