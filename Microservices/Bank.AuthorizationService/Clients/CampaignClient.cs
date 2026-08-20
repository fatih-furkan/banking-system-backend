using System.Text.Json;
using Bank.AuthorizationService.Models.Dtos.ClientDtos;
using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.AuthorizationService.Clients;

public class CampaignClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CampaignClient> _logger;

    public CampaignClient(HttpClient httpClient, ILogger<CampaignClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<ServiceResult<List<Campaign>>> GetCampaigns(
        string? status = null,
        DateTime? targetDate = null)
    {
        var parameters = new List<string>();

        if (!string.IsNullOrWhiteSpace(status))
        {
            parameters.Add(
                $"status={Uri.EscapeDataString(status)}"
            );
        }

        if (targetDate.HasValue)
        {
            parameters.Add(
                $"targetDate={targetDate.Value:yyyy-MM-dd}"
            );
        }

        string query = string.Join("&", parameters);

        string url = "/api/campaigns";

        if (query.Length > 0)
        {
            url += "?" + query;
        }
        
        using var response =
            await _httpClient.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
        {
            string errorBody =
                await response.Content.ReadAsStringAsync();

            _logger.LogWarning(
                "Get campaign failed. StatusCode: {StatusCode}, Response: {ResponseBody}",
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

            return ServiceResult<List<Campaign>>.Failure(
                errorResponse?.Error ?? Errors.CampaignClientError,
                (int)response.StatusCode
            );
        }

        GetCampaignsResponse? getCampaignsResponse;

        try
        {
            getCampaignsResponse =
                await response.Content
                    .ReadFromJsonAsync<GetCampaignsResponse>();
        }
        catch (JsonException exception)
        {
            throw new GeneralException(
                Errors.UnexpectedError,
                StatusCodes.Status502BadGateway
            );
        }

        if (getCampaignsResponse is null)
        {
            throw new GeneralException(
                Errors.UnexpectedError,
                StatusCodes.Status502BadGateway
            );
        }

        return ServiceResult<List<Campaign>>.Success(
            getCampaignsResponse.Data
        );
    }
}