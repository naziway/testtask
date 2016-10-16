using Heathmill.FixAT.Domain;

namespace Heathmill.FixAT.Services
{
    public sealed class OrderData
    {
        public OrderType OrderType { get; set; }
        public string Symbol { get; set; }
        public MarketSide MarketSide { get; set; }
        public decimal Quantity { get; set; }
        public decimal? Price { get; set; }
        public string ClOrdID { get; set; }
        public TradingAccount Account { get; set; }
    }
}
