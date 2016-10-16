
namespace Heathmill.FixAT.Domain
{
    public class TradingAccount
    {
        static TradingAccount()
        {
            None = new TradingAccount(NotSpecifiedString);
        }

        private const string NotSpecifiedString = "NotSpecified";
        public static readonly TradingAccount None;

        public TradingAccount(string account)
        {
            Name = account;
        }

        public static bool IsSet(TradingAccount account)
        {
            return !ReferenceEquals(account, None);
        }

        /// <summary>
        /// The account name
        /// </summary>
        public string Name { get; private set; }
    }
}
