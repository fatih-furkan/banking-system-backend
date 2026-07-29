using Bank.Shared.Enums;

namespace Bank.Shared.Constants;

public static class Errors
{
    public static readonly Error GetAccountError =
        new(
            ErrorCode.GetAccountErr,
            Constants.ExceptionMessages.GetAccountError);

    public static readonly Error AccountNotFoundError =
        new(
            ErrorCode.AccountNotFoundErr,
            Constants.ExceptionMessages.AccountNotFoundError);

    public static readonly Error AuthorizationServiceResponseError =
        new(
            ErrorCode.AuthorizationServiceResponseErr,
            Constants.ExceptionMessages.AuthorizationServiceResponseError);

    public static readonly Error UserNotExistError =
        new(
            ErrorCode.UserNotExistErr,
            Constants.ExceptionMessages.UserNotExistError);

    public static readonly Error UnauthorizedChannelError =
        new(
            ErrorCode.UnauthorizedChannelErr,
            Constants.ExceptionMessages.UnauthorizedChannel);

    public static readonly Error PrecisionError =
        new(
            ErrorCode.PrecisionErr,
            Constants.ExceptionMessages.PrecisionError);

    public static readonly Error NegativeAmountError =
        new(
            ErrorCode.NegativeAmountErr,
            Constants.ExceptionMessages.NegativeAmountError);

    public static readonly Error InsufficientFundsError =
        new(
            ErrorCode.InsufficientFundsErr,
            Constants.ExceptionMessages.InsufficientFundsError);

    public static readonly Error InvalidStatusError =
        new(
            ErrorCode.InvalidStatusErr,
            Constants.ExceptionMessages.InvalidStatusError);

    public static readonly Error AuthorizationGetError =
        new(
            ErrorCode.AuthorizationGetErr,
            Constants.ExceptionMessages.AuthorizationGetError);

    public static readonly Error TransactionStatusLongError =
        new(
            ErrorCode.TransactionStatusLongErr,
            Constants.ExceptionMessages.TransactionStatusLong);

    public static readonly Error CustomerIdShortError =
        new(
            ErrorCode.CustomerIdShortErr,
            Constants.ExceptionMessages.CustomerIdShort);

    public static readonly Error CardTokenLongError =
        new(
            ErrorCode.CardTokenLongErr,
            Constants.ExceptionMessages.CardTokenLong);

    public static readonly Error InvalidOtcError =
        new(
            ErrorCode.InvalidOtcErr,
            Constants.ExceptionMessages.InvalidOtc);

    public static readonly Error InvalidOtsError =
        new(
            ErrorCode.InvalidOtsErr,
            Constants.ExceptionMessages.InvalidOts);

    public static readonly Error TransactionDescriptionLongError =
        new(
            ErrorCode.TransactionDescriptionLongErr,
            Constants.ExceptionMessages.TransactionDescriptionLong);

    public static readonly Error AccountNoLengthError =
        new(
            ErrorCode.AccountNoLengthErr,
            Constants.ExceptionMessages.AccountNoLengthError);

    public static readonly Error InvalidBalanceError =
        new(
            ErrorCode.InvalidBalanceErr,
            Constants.ExceptionMessages.InvalidBalance);

    public static readonly Error InvalidTransactionAmountError =
        new(
            ErrorCode.InvalidTransactionAmountErr,
            Constants.ExceptionMessages.InvalidTransactionAmount);

    public static readonly Error BranchCodeLongError =
        new(
            ErrorCode.BranchCodeLongErr,
            Constants.ExceptionMessages.BranchCodeLong);

    public static readonly Error BranchCodeEmptyError =
        new(
            ErrorCode.BranchCodeEmptyErr,
            Constants.ExceptionMessages.BranchCodeEmpty);

    public static readonly Error StatusLongError =
        new(
            ErrorCode.StatusLongErr,
            Constants.ExceptionMessages.StatusLong);

    public static readonly Error StatusEmptyError =
        new(
            ErrorCode.StatusEmptyErr,
            Constants.ExceptionMessages.StatusEmpty);

    public static readonly Error InvalidNumberError =
        new(
            ErrorCode.InvalidNumberErr,
            Constants.ExceptionMessages.InvalidNumber);

    public static readonly Error NumberContainsCharError =
        new(
            ErrorCode.NumberContainsCharErr,
            Constants.ExceptionMessages.NumberContainsChar);

    public static readonly Error AccountCreateError =
        new(
            ErrorCode.AccountCreateErr,
            Constants.ExceptionMessages.AccountCreateError);

    public static readonly Error CardSagaError =
        new(
            ErrorCode.CardSagaErr,
            Constants.ExceptionMessages.CardSagaError);

    public static readonly Error GetCustomerError =
        new(
            ErrorCode.GetCustomerErr,
            Constants.ExceptionMessages.GetCustomerError);

    public static readonly Error TcError =
        new(
            ErrorCode.TcErr,
            Constants.ExceptionMessages.TcError);

    public static readonly Error TcAssignedError =
        new(
            ErrorCode.TcAssignedErr,
            Constants.ExceptionMessages.TcAssignedError);

    public static readonly Error UnexpectedError =
        new(
            ErrorCode.UnexpectedErr,
            Constants.ExceptionMessages.UnexpectedError);

    public static readonly Error GetCardError =
        new(
            ErrorCode.GetCardErr,
            Constants.ExceptionMessages.GetCardError);
    
    public static readonly Error CardNotExistError =
        new(
            ErrorCode.CardNotExistErr,
            Constants.ExceptionMessages.CardNotExistError);
    
    public static readonly Error CustomerNotExistError =
        new(
            ErrorCode.CustomerNotExistErr,
            Constants.ExceptionMessages.CustomerNotExistError);
}