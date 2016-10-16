using System;
using System.Collections.Generic;
using System.Linq;
using Heathmill.FixAT.Domain;
using NUnit.Framework;

namespace Heathmill.FixAT.UnitTests
{
    public class TestDefaultOrderSorter
    {
        [Test,
         TestCase("Limit;TEST;Bid;20@10", "Limit;TEST;Bid;20@11", false),
         TestCase("Limit;TEST;Bid;20@10", "Limit;TEST;Bid;20@9", true),
         TestCase("Limit;TEST;Bid;20@10", "Limit;TEST;Bid;100@11", false),
         TestCase("Limit;TEST;Bid;20@10", "Limit;TEST;Bid;100@9", true)
        ]
        public void TwoBidsAreSortedByDescendingPrice(string b1, string b2, bool bid1Better)
        {
            // Note that quantity, last update time, ID don't come into it yet
            const int id1 = 1;
            const int id2 = 2;
            var bids = new[]
                {
                    FakeOrder.CreateOrderFromString(id1, b1),
                    FakeOrder.CreateOrderFromString(id2, b2)
                };
            var sorted = new SortedSet<IOrder>(bids, new OrderStack.StandardOrderComparer());
            Assert.AreEqual(2, sorted.Count, "Sorting has changed the number of orders!");
            if (bid1Better)
            {
                Assert.AreEqual(id1, sorted.ElementAt(0).ID, "Bid 1 should be first in the list");
                Assert.AreEqual(id2, sorted.ElementAt(1).ID, "Bid 2 should be second in the list");
            }
            else
            {
                Assert.AreEqual(id2, sorted.ElementAt(0).ID, "Bid 2 should be first in the list");
                Assert.AreEqual(id1, sorted.ElementAt(1).ID, "Bid 1 should be second in the list");
            }
        }

        [Test,
         TestCase("Limit;TEST;Ask;20@10", "Limit;TEST;Ask;20@11", true),
         TestCase("Limit;TEST;Ask;20@10", "Limit;TEST;Ask;20@9", false),
         TestCase("Limit;TEST;Ask;20@10", "Limit;TEST;Ask;100@11", true),
         TestCase("Limit;TEST;Ask;20@10", "Limit;TEST;Ask;100@9", false)
        ]
        public void TwoAsksAreSortedByAscendingPrice(string a1, string a2, bool ask1Better)
        {
            // Note that quantity, last update time, ID don't come into it yet
            const int id1 = 1;
            const int id2 = 2;
            var asks = new[]
                {
                    FakeOrder.CreateOrderFromString(id1, a1),
                    FakeOrder.CreateOrderFromString(id2, a2)
                };
            var sorted = new SortedSet<IOrder>(asks, new OrderStack.StandardOrderComparer());
            Assert.AreEqual(2, sorted.Count, "Sorting has changed the number of orders!");
            if (ask1Better)
            {
                Assert.AreEqual(id1, sorted.ElementAt(0).ID, "Ask 1 should be first in the list");
                Assert.AreEqual(id2, sorted.ElementAt(1).ID, "Ask 2 should be second in the list");
            }
            else
            {
                Assert.AreEqual(id2, sorted.ElementAt(0).ID, "Ask 2 should be first in the list");
                Assert.AreEqual(id1, sorted.ElementAt(1).ID, "Ask 1 should be second in the list");
            }
        }

        [Test,
         TestCase("Limit;TEST;Ask;20@10", "Limit;TEST;Ask;20@10", true),
         TestCase("Limit;TEST;Ask;20@10", "Limit;TEST;Ask;20@10", false),
         TestCase("Limit;TEST;Bid;20@10", "Limit;TEST;Bid;20@10", true),
         TestCase("Limit;TEST;Bid;20@10", "Limit;TEST;Bid;20@10", false)
        ]
        public void OrdersAreSortedOnLastUpdateTimeIfPriceEqual(string order1,
                                                                string order2,
                                                                bool o1IsEarlier)
        {
            const int id1 = 1;
            const int id2 = 2;
            var earlyDate = new DateTime(2013, 01, 01, 10, 0, 0);
            var lateDate = new DateTime(2013, 01, 01, 10, 0, 1);
            var o1 = FakeOrder.CreateOrderFromString(id1, order1);
            var o2 = FakeOrder.CreateOrderFromString(id2, order2);
            o1.LastUpdateTime = o1IsEarlier ? earlyDate : lateDate;
            o2.LastUpdateTime = o1IsEarlier ? lateDate : earlyDate;
            Assert.AreEqual(o1.Price, o2.Price, "Orders must have same price for this test");

            var orders = new[] {o1, o2};
            var sorted = new SortedSet<IOrder>(orders, new OrderStack.StandardOrderComparer());
            Assert.AreEqual(2, sorted.Count, "Sorting has changed the number of orders!");
            if (o1IsEarlier)
            {
                Assert.AreEqual(id1, sorted.ElementAt(0).ID, "Order 1 should be first in the list");
                Assert.AreEqual(id2, sorted.ElementAt(1).ID, "Order 2 should be second in the list");
            }
            else
            {
                Assert.AreEqual(id2, sorted.ElementAt(0).ID, "Order 2 should be first in the list");
                Assert.AreEqual(id1, sorted.ElementAt(1).ID, "Order 1 should be second in the list");
            }
        }

        [Test,
         TestCase("Limit;TEST;Ask;30@10", "Limit;TEST;Ask;20@10", true),
         TestCase("Limit;TEST;Ask;20@10", "Limit;TEST;Ask;30@10", false),
         TestCase("Limit;TEST;Bid;30@10", "Limit;TEST;Bid;20@10", true),
         TestCase("Limit;TEST;Bid;20@10", "Limit;TEST;Bid;30@10", false)
        ]
        public void OrdersAreSortedOnQuantityIfPriceAndLastUpdateEqual(string order1,
                                                                       string order2,
                                                                       bool o1HasLargerQuantity)
        {
            const int id1 = 1;
            const int id2 = 2;
            var t = new DateTime(2013, 01, 01, 10, 0, 0);
            var o1 = FakeOrder.CreateOrderFromString(id1, order1);
            var o2 = FakeOrder.CreateOrderFromString(id2, order2);
            o1.LastUpdateTime = t;
            o2.LastUpdateTime = t;
            Assert.AreEqual(o1.Price, o2.Price, "Orders must have same price for this test");
            Assert.AreEqual(o1.LastUpdateTime,
                            o2.LastUpdateTime,
                            "Orders must have same last update time for this test");

            var orders = new[] {o1, o2};
            var sorted = new SortedSet<IOrder>(orders, new OrderStack.StandardOrderComparer());
            Assert.AreEqual(2, sorted.Count, "Sorting has changed the number of orders!");
            if (o1HasLargerQuantity)
            {
                Assert.AreEqual(id1, sorted.ElementAt(0).ID, "Order 1 should be first in the list");
                Assert.AreEqual(id2, sorted.ElementAt(1).ID, "Order 2 should be second in the list");
            }
            else
            {
                Assert.AreEqual(id2, sorted.ElementAt(0).ID, "Order 2 should be first in the list");
                Assert.AreEqual(id1, sorted.ElementAt(1).ID, "Order 1 should be second in the list");
            }
        }

        [Test,
         TestCase("Limit;TEST;Ask;20@10", "Limit;TEST;Ask;20@10"),
         TestCase("Limit;TEST;Bid;20@10", "Limit;TEST;Bid;20@10")
        ]
        public void OrdersAreSortedByIDIfAllElseIsEqual(string order1,
                                                        string order2)
        {
            const int id1 = 1;
            const int id2 = 2;
            var orders = new List<FakeOrder>
                {
                    FakeOrder.CreateOrderFromString(id1, order1),
                    FakeOrder.CreateOrderFromString(id2, order2)
                };
            var t = DateTime.UtcNow;
            orders.ForEach(o => o.LastUpdateTime = t);
            var sorted = new SortedSet<IOrder>(orders, new OrderStack.StandardOrderComparer());
            Assert.AreEqual(2, sorted.Count, "Sorting has changed the number of orders!");
            Assert.AreEqual(id1, sorted.ElementAt(0).ID, "Order 1 should be first in the list");
            Assert.AreEqual(id2, sorted.ElementAt(1).ID, "Order 2 should be second in the list");
        }

        [Test,
         TestCase(true, false),
         TestCase(false, true),
         TestCase(true, true)
        ]
        public void AttemptingToSortANullOrderThrows(bool order1Null, bool order2Null)
        {
            var o1 = order1Null ? null : FakeOrder.CreateOrderFromString(1, "Limit;TEST;Bid;20@10");
            var o2 = order2Null ? null : FakeOrder.CreateOrderFromString(2, "Limit;TEST;Bid;20@11");
            Assert.Throws<InvalidOperationException>(
                () => new SortedSet<IOrder>(new[] {o1, o2}, new OrderStack.StandardOrderComparer()));
        }

        [Test]
        public void AttemptingToSortABidAndAnAskThrows()
        {
            var o1 = FakeOrder.CreateOrderFromString(1, "Limit;TEST;Bid;20@10");
            var o2 = FakeOrder.CreateOrderFromString(2, "Limit;TEST;Ask;20@10");
            Assert.Throws<InvalidOperationException>(
                () => new SortedSet<IOrder>(new[] {o1, o2}, new OrderStack.StandardOrderComparer()));
        }

    }
}
