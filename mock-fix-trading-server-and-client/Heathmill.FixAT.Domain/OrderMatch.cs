
namespace Heathmill.FixAT.Domain
{
    public enum MatchType
    {
        Partial,
        Full
    }

    // TODO MXS Change this to better match what the fill reports need
    public sealed class OrderMatch
    {
        public long OrderID { get; set; }
        public decimal Price { get; set; }
        public decimal OriginalOrderQuantity { get; set; }
        public decimal MatchedQuantity { get; set; }
        public decimal RemainingQuantity { get; set; }
        public Contract Contract { get; set; }
        public MatchType MatchType { get; set; }
        public string ClOrdID { get; set; }
        public MarketSide MarketSide { get; set; }
        public TradingAccount Account { get; set; }
    }
}
