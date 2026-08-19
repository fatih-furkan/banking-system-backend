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
    
    public static readonly Error LimitNotFoundError =
        new(
            ErrorCode.LimitNotFoundErr,
            Constants.ExceptionMessages.LimitNotFoundError);
    
    public static readonly Error InsufficientLimitError =
        new(
            ErrorCode.InsufficientLimitErr,
            Constants.ExceptionMessages.InsufficientLimitError);
    
    public static readonly Error AuthCannotBeCreatedError =
        new(
            ErrorCode.AuthCannotBeCreatedErr,
            Constants.ExceptionMessages.AuthCannotBeCreatedError);

    public static readonly Error UseChargeLimitError =
        new(
            ErrorCode.UseChargeLimitErr,
            Constants.ExceptionMessages.UseChargeLimitError);
    
    public static readonly Error DepositCompensateError =
        new(
            ErrorCode.DepositCompensateErr,
            Constants.ExceptionMessages.DepositCompensateError);
    
    public static readonly Error AuthClientError =
        new(
            ErrorCode.AuthClientErr,
            Constants.ExceptionMessages.AuthClientError);
    
    
    public static readonly Error AuthCompensateError =
        new(
            ErrorCode.AuthCompensateErr,
            Constants.ExceptionMessages.AuthCompensateError);
    
    public static readonly Error ChargeLimitCreateError =
        new(
            ErrorCode.ChargeLimitCreateErr,
            Constants.ExceptionMessages.ChargeLimitCreateError);
    
    public static readonly Error SpendingLimitCreateError =
        new(
            ErrorCode.SpendingLimitCreateErr,
            Constants.ExceptionMessages.SpendingLimitCreateError);
    
    public static readonly Error CardClientError =
        new(
            ErrorCode.CardClientErr,
            Constants.ExceptionMessages.CardClientError);
    
    public static readonly Error AccountSaleError =
        new(
            ErrorCode.AccountSaleErr,
            Constants.ExceptionMessages.AccountSaleError);
    
    public static readonly Error LimitAlreadyExistsError =
        new(
            ErrorCode.LimitAlreadyExistsErr,
            Constants.ExceptionMessages.LimitAlreadyExistsError);
    
    public static readonly Error CustomerNotFoundError =
        new(
            ErrorCode.CustomerNotFoundErr,
            Constants.ExceptionMessages.CustomerNotFoundError);
    
    public static readonly Error AuthorizationCreateError =
        new(
            ErrorCode.AuthorizationCreateErr,
            Constants.ExceptionMessages.AuthorizationCreateError);
    
    public static readonly Error TransactionAlreadyExistsError =
        new(
            ErrorCode.TransactionAlreadyExistsErr,
            Constants.ExceptionMessages.TransactionAlreadyExistsError);
    
    public static readonly Error AccountClientError =
        new(
            ErrorCode.AccountClientErr,
            Constants.ExceptionMessages.AccountClientError);
    
    public static readonly Error CardNotFoundError =
        new(
            ErrorCode.CardNotFoundErr,
            Constants.ExceptionMessages.CardNotFoundError);
    
    public static readonly Error CustomerClientError =
        new(
            ErrorCode.CustomerClientErr,
            Constants.ExceptionMessages.CustomerClientError);
    
    public static readonly Error AuthorizationClientError =
        new(
            ErrorCode.AuthorizationClientErr,
            Constants.ExceptionMessages.AuthorizationClientError);
    
    public static readonly Error CustomerServiceResponseError =
        new(
            ErrorCode.CustomerServiceResponseErr,
            Constants.ExceptionMessages.CustomerServiceResponseError);
    
    public static readonly Error CardServiceResponseError =
        new(
            ErrorCode.CardServiceResponseErr,
            Constants.ExceptionMessages.CardServiceResponseError);
    
    public static readonly Error AccountServiceResponseError =
        new(
            ErrorCode.AccountServiceResponseErr,
            Constants.ExceptionMessages.AccountServiceResponseError);
    
    public static readonly Error ChargeLimitCompensateError =
        new(
            ErrorCode.ChargeLimitCompensateErr,
            Constants.ExceptionMessages.ChargeLimitCompensateError);
    
    public static readonly Error AccountRefundError =
        new(
            ErrorCode.AccountRefundErr,
            Constants.ExceptionMessages.AccountRefundError);
    
    public static readonly Error InvalidRefundTypeError =
        new(
            ErrorCode.InvalidRefundTypeErr,
            Constants.ExceptionMessages.InvalidRefundTypeError);
    
    public static readonly Error TransactionNotExistError =
        new(
            ErrorCode.TransactionNotExistErr,
            Constants.ExceptionMessages.TransactionNotExistError);
    
    public static readonly Error AmountRefundTypeMismatchError =
        new(
            ErrorCode.AmountRefundTypeMismatchErr,
            Constants.ExceptionMessages.AmountRefundTypeMismatchError);
    
    public static readonly Error AuthorizationNotFoundError =
        new(
            ErrorCode.AuthorizationNotFoundErr,
            Constants.ExceptionMessages.AuthorizationNotFoundError);
    
    public static readonly Error AssignStatusError =
        new(
            ErrorCode.AssignStatusErr,
            Constants.ExceptionMessages.AssignStatusError);
    
    public static readonly Error TransactionAlreadyRefundedError =
        new(
            ErrorCode.TransactionAlreadyRefundedErr,
            Constants.ExceptionMessages.TransactionAlreadyRefundedError);
    
    public static readonly Error AccountRefundCompensationError =
        new(
            ErrorCode.AccountRefundCompensationErr,
            Constants.ExceptionMessages.AccountRefundCompensationError);
    
    public static readonly Error MerchantNameMismatchError =
        new(
            ErrorCode.MerchantNameMismatchErr,
            Constants.ExceptionMessages.MerchantNameMismatchError);
    
    public static readonly Error CardTokenMismatchError =
        new(
            ErrorCode.CardTokenMismatchErr,
            Constants.ExceptionMessages.CardTokenMismatchError);
    
    public static readonly Error RefundAmountTooMuchError =
        new(
            ErrorCode.RefundAmountTooMuchErr,
            Constants.ExceptionMessages.RefundAmountTooMuchError);
    
    public static readonly Error RefundedAmountUpdateError =
        new(
            ErrorCode.RefundedAmountUpdateErr,
            Constants.ExceptionMessages.RefundedAmountUpdateError);
    
    public static readonly Error PointAccountNotFoundError =
        new(
            ErrorCode.PointAccountNotFoundErr,
            Constants.ExceptionMessages.PointAccountNotFoundError);
    
    public static readonly Error PointClientError =
        new(
            ErrorCode.PointClientErr,
            Constants.ExceptionMessages.PointClientError);
    
    public static readonly Error PointAccountCreateError =
        new(
            ErrorCode.PointAccountCreateErr,
            Constants.ExceptionMessages.PointAccountCreateError);
    
    public static readonly Error SpendingLimitCompensateError =
        new(
            ErrorCode.SpendingLimitCompensateErr,
            Constants.ExceptionMessages.SpendingLimitCompensateError);
    
    public static readonly Error PointServiceError =
        new(
            ErrorCode.PointServiceErr,
            Constants.ExceptionMessages.PointServiceError);
    
    public static readonly Error AddPointError =
        new(
            ErrorCode.AddPointErr,
            Constants.ExceptionMessages.AddPointError);
    
    public static readonly Error CampaignClientError =
        new(
            ErrorCode.CampaignClientErr,
            Constants.ExceptionMessages.CampaignClientError);
}