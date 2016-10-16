using System.Collections.Generic;
using System.Linq;
using Heathmill.FixAT.Domain;
using Heathmill.FixAT.Server;
using NUnit.Framework;

namespace Heathmill.FixAT.UnitTests
{
    public class TestRepositoryMatchNotifications
    {
        [Test]
        public void TwoTradesMatchingProducesTwoMatchNotifications()
        {
            var matcher = new StandardOrderMatcher();
            var orderRepository = new StandardOrderRepository(matcher);
            var matchRecorder = new MatchRecorder();
            orderRepository.OrdersMatched += matchRecorder.OnMatch;
            
            var ask = FakeOrder.CreateOrderFromString(1, "Limit;TEST;Ask;10@10");
            var bid = FakeOrder.CreateOrderFromString(1, "Limit;TEST;Bid;10@10");

            AddOrderToRepository(ask, orderRepository);
            AddOrderToRepository(bid, orderRepository);

            orderRepository.MatchOrders(bid.Contract);

            Assert.AreEqual(1, matchRecorder.Matches.Count);
            var matches = matchRecorder.Matches[0];
            Assert.AreEqual(2, matches.OrderMatches.Count());
        }

        private static void AddOrderToRepository(IOrder o, IOrderRepository r)
        {
            r.AddOrder(o.ID,
                       o.Contract,
                       o.OrderType,
                       o.MarketSide,
                       o.Price,
                       o.Quantity,
                       o.ClOrdID,
                       o.Account);
        }

        private class MatchRecorder
        {
            public MatchRecorder()
            {
                Matches = new List<OrdersMatchedEventArgs>();
            }

            public void OnMatch(OrdersMatchedEventArgs e)
            {
                Matches.Add(e);
            }

            public List<OrdersMatchedEventArgs> Matches { get; private set; }
        }


    }
}
