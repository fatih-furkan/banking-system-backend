namespace Bank.Shared;

public class Constants
{
    public static class Otcs
    {
        public const int Deposit = 10;
        public const int Withdrawal = 20;
        public const int Refund = 30;
        public const int Payment = 40;
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
    }
    
}