using System.Text.Json;
using Bank.CustomerService.Models.ClientModels;
using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.CustomerService.Clients;

public class PointClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PointClient> _logger;

    public PointClient(HttpClient httpClient, ILogger<PointClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ServiceResult<string>>
        CreatePointAccountAsync(
            CreatePointAccountRequest request,
            CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/point",
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

            return ServiceResult<string>.Failure(
                errorResponse?.Error ?? Errors.PointClientError,
                (int)response.StatusCode
            );
        }
        
        CreatePointAccountResponse? result;

        try
        {
            result =
                await response.Content
                    .ReadFromJsonAsync<CreatePointAccountResponse>(
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

        return ServiceResult<string>.Success(result.AccountNo);
    }
    
    public async Task<ServiceResult<Unit>> AssignStatusAsync(
        string pointAccountNo,
        string status)
    {
        var body = new
        {
            Status = status
        };

        using var response = await _httpClient.PostAsJsonAsync(
            $"/api/point/{Uri.EscapeDataString(pointAccountNo)}/assign-status",
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
                errorResponse?.Error ?? Errors.PointClientError,
                (int)response.StatusCode
            );
        }

        return ServiceResult<Unit>.Success(new Unit());
    }

    public async Task<ServiceResult<Unit>> CompensateCreatePointAccountAsync(
        string pointAccountNo)
    {

        using var response = await _httpClient.PostAsJsonAsync(
            $"/api/point/compensate-create-point-account",
            pointAccountNo
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
                errorResponse?.Error ?? Errors.PointClientError,
                (int)response.StatusCode
            );
        }

        return ServiceResult<Unit>.Success(new Unit());
    }
}