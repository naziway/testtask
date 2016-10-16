
namespace Heathmill.FixAT.Domain
{
    public static class MarketSideExtensions
    {
        public static MarketSide Opposite(this MarketSide side)
        {
            return side == MarketSide.Bid ? MarketSide.Ask : MarketSide.Bid;
        }
    }
}
