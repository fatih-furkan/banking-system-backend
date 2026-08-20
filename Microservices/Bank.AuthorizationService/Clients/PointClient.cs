using System.Text.Json;
using Bank.AuthorizationService.Models.Dtos.ClientDtos;
using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.AuthorizationService.Clients;

public class PointClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PointClient> _logger;
    
    public PointClient(HttpClient httpClient, ILogger<PointClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<ServiceResult<AddPointResponse>> AddPointAsync(AddPointRequest request)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/point/add-point",
            request
        );

        if (!response.IsSuccessStatusCode)
        {
            string errorBody =
                await response.Content.ReadAsStringAsync();

            _logger.LogWarning(
                "Point could not be added. StatusCode: {StatusCode}, Response: {ResponseBody}",
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

            return ServiceResult<AddPointResponse>.Failure(
                errorResponse?.Error ?? Errors.AddPointError,
                (int)response.StatusCode
            );
        }

        AddPointResponse? addPointResponse;

        try
        {
            addPointResponse =
                await response.Content
                    .ReadFromJsonAsync<AddPointResponse>();
        }
        catch (JsonException exception)
        {
            throw new GeneralException(
                Errors.UnexpectedError,
                StatusCodes.Status502BadGateway
            );
        }

        if (addPointResponse is null)
        {
            throw new GeneralException(
                Errors.UnexpectedError,
                StatusCodes.Status502BadGateway
            );
        }

        return ServiceResult<AddPointResponse>.Success(
            addPointResponse
        );
    }
}