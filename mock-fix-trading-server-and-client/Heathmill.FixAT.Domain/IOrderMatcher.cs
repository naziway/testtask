using System.Collections.Generic;

namespace Heathmill.FixAT.Domain
{
    public interface IOrderMatcher
    {
        /// <summary>
        /// Attempt to match orders from the bid and ask sides
        /// </summary>
        /// <param name="sortedBids">A sorted list of the bids</param>
        /// <param name="sortedAsks">A sorted list of the asks</param>
        /// <returns>A list of the matches made</returns>
        List<OrderMatch> Match(IEnumerable<IOrder> sortedBids,
                               IEnumerable<IOrder> sortedAsks);
    }
}
