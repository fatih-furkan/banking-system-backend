using System.Text.Json;
using Bank.AccountService.Models.ClientModels;
using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.AccountService.Clients;

public class AuthorizationClient
{
    private readonly HttpClient _httpClient;

    public AuthorizationClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ServiceResult<CreateAuthorizationResponse>>
        CreateAuthorizationAsync(CreateAuthorizationRequest request)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/authorization",
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
                // The downstream service returned an invalid error body.
            }

            return ServiceResult<CreateAuthorizationResponse>.Failure(
                errorResponse?.Error
                ?? Errors.AuthorizationServiceResponseError,
                (int)response.StatusCode
            );
        }

        CreateAuthorizationResponse? authorization;

        try
        {
            authorization =
                await response.Content
                    .ReadFromJsonAsync<CreateAuthorizationResponse>();
        }
        catch (JsonException)
        {
            throw new GeneralException(
                Errors.AuthorizationServiceResponseError,
                StatusCodes.Status502BadGateway
            );
        }

        if (authorization is null)
        {
            throw new GeneralException(
                Errors.AuthorizationServiceResponseError,
                StatusCodes.Status502BadGateway
            );
        }

        return ServiceResult<CreateAuthorizationResponse>.Success(
            authorization
        );
    }

    public async Task<ServiceResult<Unit>> AssignStatusAsync(
        string guid,
        string status)
    {
        var body = new
        {
            Status = status
        };

        using var response = await _httpClient.PatchAsJsonAsync(
            $"/api/authorization/{Uri.EscapeDataString(guid)}/assign-status",
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
                errorResponse?.Error ?? Errors.AuthClientError,
                (int)response.StatusCode
            );
        }

        return ServiceResult<Unit>.Success(new Unit());
    }
}