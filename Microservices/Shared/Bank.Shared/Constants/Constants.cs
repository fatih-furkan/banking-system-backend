namespace Bank.Shared.Constants;

public class Constants
{
    public static class Otcs
    {
        public const int Deposit = 10;
        public const int Withdrawal = 20;
        public const int Refund = 30;
        public const int Payment = 40;
        public const int Sale = 50;
    }

    public static class Ots
    {
        public class WithdrawalOts
        {
            public const int CashWithdrawal = 10;
            public const int FastWihtdrawal = 20;
        }

        public class DepositOts
        {
            public const int BranchDeposit = 10;
            public const int AtmDeposit = 20;
        }
        
        public class SaleOts
        {
            public const int Default = 10;
        }

        public class RefundOts
        {
            public const int Complete = 10;
            public const int Partial = 20;
        }
    }

    public static class Limits
    {
        public const long DailyChargeLimit = 10000;
        public const long MonthlyChargeLimit = 100000;
        public const long AnnualChargeLimit = 1000000;
        
        public const long DailySpendingLimit = 20000;
        public const long MonthlySpendingLimit = 200000;
        public const long AnnualSpendingLimit = 2000000;
    }
    public static class ExceptionMessages
    {
        public const string GetAccountError = "Error while getting the accounts";
        public const string AccountNotFoundError = "Account could not be found.";
        public const string AuthorizationServiceResponseError =
            "AuthorizationService response is invalid.";
        public const string UnauthorizedChannel = "Unauthorized channel.";
        public const string PrecisionError = "The amount can have at most two decimal places.";
        public const string NegativeAmountError = "Amount cannot be negative.";
        public const string InsufficientFundsError = "Account does not exist or has insufficient funds.";
        public const string InvalidStatusError = "Status cannot contain letters.";
        public const string AuthorizationGetError = "Error while getting the authorizations";
        public const string TransactionStatusLong = "Transaction status is too long";
        public const string CustomerIdShort = "Customer ID should be greater than 0.";
        public const string CardTokenLong = "Card token is too long.";
        public const string InvalidOtc = "OTC must be in range 0-9999.";
        public const string InvalidOts = "OTS must be in range 0-9999.";
        public const string TransactionDescriptionLong = "Transaction description is too long.";
        public const string AccountNoLengthError = "Account number should be 8 characters length.";
        public const string InvalidBalance = "Balance has an invalid value.";
        public const string InvalidTransactionAmount = "Transaction amount has an invalid value.";
        public const string BranchCodeLong = "Branch code is too long";
        public const string BranchCodeEmpty = "Branch code cannot be empty";
        public const string StatusLong = "Status is too long";
        public const string StatusEmpty = "Status cannot be empty";
        public const string InvalidNumber = "Number value is empty";
        public const string NumberContainsChar = "The alleged number contains non numeric characters.";
        public const string AccountCreateError = "Account could not be created.";
        public const string CardSagaError = "Create card saga failed.";

        public static string AccountCompensationError(string accountId) =>
            $"Account compensation failed. AccountId: {accountId}";

        public const string GetCustomerError = "Customer could not be found.";
        public const string TcError = "The tc is wrong.";
        public const string TcAssignedError = "The tc is already assigned to another user.";
        public const string UnexpectedError = "Unexpected error.";
        public const string GetCardError = "Error while getting the cards";
        public const string CardNotExistError = "Card does not exist.";
        public const string CustomerNotExistError = "The customer does not exist.";
        public const string LimitNotFoundError = "Limit could not be found.";
        public const string InsufficientLimitError = "Customer does not exist or has insufficient limit.";
        public const string AuthCannotBeCreatedError = "Authorization entry could not be created.";
        public const string UseChargeLimitError = "Limits could not be used.";
        public const string ChargeLimitCompensationError = "Charge limit compensation is failed.";
        public const string DepositCompensateError = "Deposit could not be compensated.";
        public const string AuthClientError = "Auth client could not complete the action.";
        public const string AuthCompensateError = "Authorization compensation process is failed.";
        public const string ChargeLimitCreateError = "Charge limit could not be created.";
        public const string SpendingLimitCreateError = "Spending limit could not be created.";
        public const string CardClientError = "Card client got unsuccessful response.";

        public const string CustomerServiceUrlError = "CustomerService URL is not configured.";
        public const string AccountServiceUrlError = "AccountService URL is not configured.";
        public const string CardServiceUrlError = "CardService URL is not configured.";
        public const string AuthorizationServiceUrlError = "AuthorizationService URL is not configured.";
        public const string AccountSaleError = "Account sale did not answer as expected.";
        public const string SpendingLimitCompensationError = "Charge limit compensation is failed.";
        public const string LimitAlreadyExistsError = "A limit entry already exists for this customer.";
        public const string CustomerNotFoundError = "Customer could not be found.";
        public const string AuthorizationCreateError = "Authorization could not be created.";
        public const string AccountSaleCompensationError = "Account sale could not be compensated.";
        public const string TransactionAlreadyExistsError = "This transaction ID already exists.";
        public const string AccountClientError = "Account client returned an unexpected status.";
        public const string CardNotFoundError = "Card could not be found.";
        public const string CustomerClientError = "Customer client returned an unexpected status.";
        public const string AuthorizationClientError = "Authorization client returned an unexpected status.";
        public const string DepositCompensation = "Deposit failed. Starting compensation.";
        public const string CustomerServiceResponseError = "Customer service returned an unexpected status.";
        public const string CardServiceResponseError = "Card service returned an unexpected status.";
        public const string AccountServiceResponseError = "Account service returned an unexpected status.";
        public const string ChargeLimitCompensateError = "Charge limit compensation is failed.";
        public const string AccountRefundError = "Account refund did not answer as expected.";
        public const string InvalidRefundTypeError = "Invalid refund type.";
        public const string TransactionNotExistError = "Transaction does not exist.";
        public const string AmountRefundTypeMismatchError = "Amount is not compatible with the refund type.";
        public const string AuthorizationNotFoundError = "Authorization entry could not be found.";
        public const string AssignStatusError = "Status could not be assigned.";
        public const string TransactionAlreadyRefundedError = "Transaction is already refunded.";
        public const string AccountRefundCompensationError = "Account refund compensation is failed.";
        public const string MerchantNameLengthError = "Merchant name length is not appropriate.";
        public const string MerchantNameMismatchError = "Merchant name is different from the previous record.";
        public const string CardTokenMismatchError = "Card token is different from the previous record.";
        public const string RefundAmountTooMuchError = "The total refund amount is higher than the sale amount.";
        public const string RefundedAmountUpdateError = "Refunded amount could not be updated.";
    }
    
    public static class CompensationOperationTypes
    {
        public const string ReverseDepositBalance =
            "REVERSE_DEPOSIT_BALANCE";

        public const string RestoreWithdrawBalance =
            "RESTORE_WITHDRAW_BALANCE";

        public const string RestoreSaleBalance =
            "RESTORE_SALE_BALANCE";

        public const string RestoreChargeLimit =
            "RESTORE_CHARGE_LIMIT";

        public const string RestoreSpendingLimit =
            "RESTORE_SPENDING_LIMIT";

        public const string CancelAuthorization =
            "CANCEL_AUTHORIZATION";

        public const string RestoreRefundBalance =
            "RESTORE_REFUND_BALANCE";

        public const string CompensateRefundAuthorization =
            "COMPENSATE_REFUND_AUTHORIZATION";
    }
}