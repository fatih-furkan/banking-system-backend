using System.Text;
using Bank.CardService.Clients;
using Bank.CardService.Data;
using Bank.CardService.Models.Dtos;
using Bank.CardService.Models.Entities;
using Bank.Shared;
using Bank.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace Bank.CardService.Services.Internal;

public class CardCreator
{
    private readonly AppDbContext _context;
    private readonly AccountClient _accountClient;

    public CardCreator(AppDbContext context, CustomerClient customerClient, AccountClient accountClient)
    {
        _context = context;
        _accountClient = accountClient;
    }
    //should only be called from the saga.
    public async Task<ServiceResult<CreateCardResponse>> CreateCardAsync(CreateCardRequest request)
    {
        
        var accountExistsResult = await _accountClient.AccountExistsAsync(request.AccountNo);

        if (!accountExistsResult.IsSuccess)
        {
            return ServiceResult<CreateCardResponse>.Failure(Errors.AccountClientError);
        }
        
        if (accountExistsResult.Data == false)
        {
            return ServiceResult<CreateCardResponse>.Failure(Errors.AccountNotFoundError);
        }
        
        var (success, cardNo) = await GenerateCardNoAsync();
        if (!success)
        {
            throw new InvalidOperationException();
        }
        
        string cardToken = GenerateCardToken(cardNo);

        var card = new Card
        {
            CardToken = cardToken,
            CardNo = cardNo,
            CardAccountNo = request.AccountNo,
            CustomerId = request.CustomerId.Value
        };
        
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();
        
        CreateCardResponse response = new CreateCardResponse
        {
            CardAccountNo = card.CardAccountNo,
            CardToken = card.CardToken,
            CustomerId = card.CustomerId
        };
        return ServiceResult<CreateCardResponse>.Success(response);
    }
    
    private string GenerateCardToken(string cardNo)
    {
        
        byte[] dataBytes = Encoding.UTF8.GetBytes(cardNo);
        return Convert.ToBase64String(dataBytes);
    }

    private async Task<(bool,string)> GenerateCardNoAsync()
    {
        StringBuilder cardNo = new StringBuilder("99999999");
        var sequenceValue = await GetNextCardNoSequenceValueAsync();
        cardNo.Append(sequenceValue);
        try
        {
            int luhn = CalculateLuhnCheckDigit(cardNo.ToString());
            cardNo.Append(luhn);
            return (true,cardNo.ToString());
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
            return (false, "");
        }
        
    }
    
    private static int CalculateLuhnCheckDigit(string numberWithoutCheckDigit)
    {
        if (string.IsNullOrWhiteSpace(numberWithoutCheckDigit))
            throw new ArgumentException(Constants.ExceptionMessages.InvalidNumber);

        int sum = 0;
        bool shouldDouble = true;

        for (int i = numberWithoutCheckDigit.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(numberWithoutCheckDigit[i]))
                throw new ArgumentException(Constants.ExceptionMessages.NumberContainsChar);

            int digit = numberWithoutCheckDigit[i] - '0';

            if (shouldDouble)
            {
                digit *= 2;

                if (digit > 9)
                    digit -= 9;
            }

            sum += digit;
            shouldDouble = !shouldDouble;
        }

        return (10 - (sum % 10)) % 10;
    }
    
    private async Task<string> GetNextCardNoSequenceValueAsync()
    {
        var connection = _context.Database.GetDbConnection();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT CARD_NO_SEQ.NEXTVAL FROM DUAL";

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var result = await command.ExecuteScalarAsync();
        var sequenceValue = Convert.ToInt64(result);
        var formattedSeq = sequenceValue.ToString("D7");

        return formattedSeq;
    }
}