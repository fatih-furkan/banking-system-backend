namespace Bank.Shared.Enums;

public enum ErrorCode
{
    GetAccountErr,
    AccountNotFoundErr,
    AuthorizationServiceResponseErr,

    UserNotExistErr,
    UnauthorizedChannelErr,
    PrecisionErr,
    NegativeAmountErr,
    InsufficientFundsErr,
    InvalidStatusErr,

    AuthorizationGetErr,
    TransactionStatusLongErr,
    CustomerIdShortErr,
    CardTokenLongErr,
    InvalidOtcErr,
    InvalidOtsErr,
    TransactionDescriptionLongErr,
    AccountNoLengthErr,
    InvalidBalanceErr,
    InvalidTransactionAmountErr,

    BranchCodeLongErr,
    BranchCodeEmptyErr,
    StatusLongErr,
    StatusEmptyErr,

    InvalidNumberErr,
    NumberContainsCharErr,
    AccountCreateErr,
    CardSagaErr,

    GetCustomerErr,
    TcErr,
    TcAssignedErr,
    UnexpectedErr,
    GetCardErr,
    CardNotExistErr,
    CustomerNotExistErr
}