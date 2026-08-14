
﻿using System.Text.Json;

﻿// Yapı olarak AuthorizationService mikroservisine bağlanıp harcama limiti (/api/spending-limit) oluşturmakla görevli.

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
}