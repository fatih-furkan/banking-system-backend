using System.Text;
using Bank.CardService.Clients;
using Bank.CardService.Data;
using Bank.CardService.Models.Dtos;
using Bank.CardService.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Bank.Shared;

namespace Bank.CardService.Services;

public class CardService
{
    private readonly AppDbContext _context;
    private readonly CustomerClient _customerClient;
    private readonly AccountClient _accountClient;

    public CardService(AppDbContext context, 
        CustomerClient customerClient, 
        AccountClient accountClient)
    {
        _context = context;
        _customerClient = customerClient;
        _accountClient = accountClient;
    }

    public async Task<List<Card>> GetAllCardsAsync()
    {
        return await _context.Cards.ToListAsync();
    }
    
    public async Task<Card?> GetCardByCardTokenAsync(string cardToken)
    {
        return await _context.Cards.FindAsync(cardToken);
    }
    
    public async Task<bool> CheckExistenceByCardTokenAsync(string cardToken)
    {
        return await _context.Cards.AnyAsync(card => card.CardToken == cardToken);
    }

    public async Task<bool> CardBelongsToCustomer(string cardToken, long customerId)
    {
        return await _context.Cards.AnyAsync(card => card.CardToken == cardToken && card.CustomerId == customerId);
    }

    //should only be called from the saga.
    public async Task<ServiceResult<CreateCardResponse>> CreateCardAsync(CreateCardRequest request)
    {
        bool customerExists = await _customerClient.CustomerExistsAsync(request.CustomerId);
        if (!customerExists)
        {
            return ServiceResult<CreateCardResponse>.Failure("Customer does not exist!");
        }
        
        bool accountExists = await _accountClient.AccountExistsAsync(request.AccountNo);
        if (!accountExists)
        {
            return ServiceResult<CreateCardResponse>.Failure("Account does not exist!");
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
            CustomerId = request.CustomerId
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


    public async Task<bool> DeleteCard(string cardToken)
    {
        var card = await _context.Cards.FirstOrDefaultAsync();
        if (card != null)
        {
            _context.Cards.Remove(card);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
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

    private static int CalculateLuhnCheckDigit(string numberWithoutCheckDigit)
    {
        if (string.IsNullOrWhiteSpace(numberWithoutCheckDigit))
            throw new ArgumentException("Number is empty");

        int sum = 0;
        bool shouldDouble = true;

        for (int i = numberWithoutCheckDigit.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(numberWithoutCheckDigit[i]))
                throw new ArgumentException("The alleged number contains non numeric characters.");

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
}