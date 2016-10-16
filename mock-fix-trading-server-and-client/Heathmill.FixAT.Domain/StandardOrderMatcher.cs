using System;
using System.Collections.Generic;
using System.Linq;

namespace Heathmill.FixAT.Domain
{
    public class StandardOrderMatcher : IOrderMatcher
    {
        // TODO Currently does not handle AoN / IoC style orders or support crossed markets
        public List<OrderMatch> Match(IEnumerable<IOrder> sortedBids,
                                      IEnumerable<IOrder> sortedAsks)
        {
            if (sortedBids == null) throw new ArgumentNullException("sortedBids");
            if (sortedAsks == null) throw new ArgumentNullException("sortedAsks");

            var bids = sortedBids.ToList();
            var asks = sortedAsks.ToList();
            
            if (bids.Count == 0 || asks.Count == 0)
                return new List<OrderMatch>(); // Nothing to match so no matches

            var bestBidPrice = bids[0].Price;
            var bestAskPrice = asks[0].Price;

            if (bestBidPrice > bestAskPrice)
                throw new DomainException("Crossed market when matching orders");

            if (bestBidPrice != bestAskPrice)
                return new List<OrderMatch>(); // No matching orders, so return empty

            var potentialBidMatches = bids.TakeWhile(o => o.Price == bestBidPrice).ToArray();
            var potentialAskMatches = asks.TakeWhile(o => o.Price == bestAskPrice).ToArray();
            var potentialBidMatchQ = potentialBidMatches.Sum(o => o.Quantity);
            var potentialAskMatchQ = potentialAskMatches.Sum(o => o.Quantity);
            var matchQ = Math.Min(potentialBidMatchQ, potentialAskMatchQ);
            
            var matches = MatchOrders(potentialBidMatches, matchQ);
            matches.AddRange(MatchOrders(potentialAskMatches, matchQ));
            return matches;
        }

        private static List<OrderMatch> MatchOrders(IEnumerable<IOrder> potentialMatches,
                                                    decimal toMatch)
        {
            var matches = new List<OrderMatch>();
            foreach (var o in potentialMatches)
            {
                if (toMatch <= 0)
                    break;

                matches.Add(o.Quantity > toMatch
                                ? CreatePartialMatch(o, toMatch)
                                : CreateFullMatch(o));

                toMatch -= o.Quantity;
            }
            return matches;
        }

        private static OrderMatch CreatePartialMatch(IOrder o, decimal matched)
        {
            return new OrderMatch
            {
                Contract = o.Contract,
                ClOrdID = o.ClOrdID,
                OriginalOrderQuantity = o.OriginalQuantity,
                MatchedQuantity = matched,
                MatchType = MatchType.Partial,
                OrderID = o.ID,
                Price = o.Price,
                RemainingQuantity = o.Quantity - matched,
                MarketSide = o.MarketSide,
                Account = o.Account
            };
        }

        private static OrderMatch CreateFullMatch(IOrder o)
        {
            return new OrderMatch
            {
                Contract = o.Contract,
                ClOrdID = o.ClOrdID,
                OriginalOrderQuantity = o.OriginalQuantity,
                MatchedQuantity = o.Quantity,
                MatchType = MatchType.Full,
                OrderID = o.ID,
                Price = o.Price,
                RemainingQuantity = 0m,
                MarketSide = o.MarketSide,
                Account = o.Account
            };
        }

    }
}
