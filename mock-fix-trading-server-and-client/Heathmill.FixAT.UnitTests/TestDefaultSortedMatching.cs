using System;
using System.Collections.Generic;
using System.Linq;
using Heathmill.FixAT.Domain;
using NUnit.Framework;

namespace Heathmill.FixAT.UnitTests
{
    [TestFixture]
    public class TestDefaultSortedMatching
    {
        [Test]
        public void NoOrdersMeansNoMatches()
        {
            var empty = new List<IOrder>();
            var matcher = CreateMatcher();
            var matches = matcher.Match(empty, empty);
            Assert.IsEmpty(matches, "There should not be any matches");
        }

        [Test,
         TestCase(MarketSide.Bid),
         TestCase(MarketSide.Ask)]
        public void OneOrderShouldNotMatch(MarketSide side)
        {
            var orderString = string.Format("Limit;TEST;{0};10@10", side);
            var stack = BuildStack(new[] {orderString});
            var sortedBids = stack.Item1;
            var sortedAsks = stack.Item2;
            var matcher = CreateMatcher();
            var matches = matcher.Match(sortedBids, sortedAsks);
            Assert.IsEmpty(matches, "There should not be any matches");
        }

        [Test,
         TestCase(MarketSide.Bid),
         TestCase(MarketSide.Ask)]
        public void TwoOrdersOnTheSameSideShouldNotMatch(MarketSide side)
        {
            var orderString = string.Format("Limit;TEST;{0};10@10", side);
            var stack = BuildStack(new[] {orderString, orderString});
            var sortedBids = stack.Item1;
            var sortedAsks = stack.Item2;
            var matcher = CreateMatcher();
            var matches = matcher.Match(sortedBids, sortedAsks);
            Assert.IsEmpty(matches, "There should not be any matches");
        }

        [Test]
        public void BidAndAskAtSamePriceWithSameQuantityForSameContractShouldFullyMatch()
        {
            var stack = BuildStack(new[] {"Limit;TEST;Bid;20@10", "Limit;TEST;Ask;20@10"});
            var sortedBids = stack.Item1;
            var sortedAsks = stack.Item2;
            var matcher = CreateMatcher();
            var matches = matcher.Match(sortedBids, sortedAsks);

            AssertThatAllOrdersFullyMatched(sortedBids, sortedAsks, matches);
        }

        [Test,
         TestCase("Limit;TEST;Bid;20@10", "Limit;TEST;Ask;10@10", true),
         TestCase("Limit;TEST;Bid;10@10", "Limit;TEST;Ask;20@10", false)
        ]
        public void LargeOrderPartiallyMatchedBySmallerOrder(string bid,
                                                             string ask,
                                                             bool bidLargerThanAsk)
        {
            var stack = BuildStack(new[] {bid, ask});
            var sortedBids = stack.Item1;
            var sortedAsks = stack.Item2;
            Assert.AreEqual(1, sortedBids.Count);
            Assert.AreEqual(1, sortedAsks.Count);
            var b = sortedBids.ElementAt(0);
            var a = sortedAsks.ElementAt(0);

            var bidMatchType = bidLargerThanAsk ? MatchType.Partial : MatchType.Full;
            var askMatchType = bidLargerThanAsk ? MatchType.Full : MatchType.Partial;
            var matchQuantity = bidLargerThanAsk ? a.Quantity : b.Quantity;

            var matcher = CreateMatcher();
            var matches = matcher.Match(sortedBids, sortedAsks);
            var expectedMatches = new[]
                {
                    new OrderMatch
                        {
                            MatchType = bidMatchType,
                            Contract = b.Contract,
                            MatchedQuantity = matchQuantity,
                            RemainingQuantity = b.Quantity - matchQuantity,
                            OrderID = b.ID,
                            Price = b.Price,
                            ClOrdID = b.ClOrdID
                        },
                    new OrderMatch
                        {
                            MatchType = askMatchType,
                            Contract = a.Contract,
                            MatchedQuantity = matchQuantity,
                            RemainingQuantity = a.Quantity - matchQuantity,
                            OrderID = a.ID,
                            Price = a.Price,
                            ClOrdID = a.ClOrdID
                        }
                };

            AssertMatchesAsExpected(expectedMatches, matches);
        }

        [Test,
         TestCase("Limit;TEST;Bid;20@10", "Limit;TEST;Ask;10@10", "Limit;TEST;Ask;10@10"),
         TestCase("Limit;TEST;Bid;20@10", "Limit;TEST;Ask;15@10", "Limit;TEST;Ask;5@10"),
         TestCase("Limit;TEST;Bid;20@10", "Limit;TEST;Ask;5@10", "Limit;TEST;Ask;15@10"),
         TestCase("Limit;TEST;Bid;10@10", "Limit;TEST;Bid;10@10", "Limit;TEST;Ask;20@10"),
         TestCase("Limit;TEST;Bid;15@10", "Limit;TEST;Bid;5@10", "Limit;TEST;Ask;20@10"),
         TestCase("Limit;TEST;Bid;5@10", "Limit;TEST;Bid;15@10", "Limit;TEST;Ask;20@10")
        ]
        public void LargerOrderFullyMatchedByTwoSmallerOrders(string o1, string o2, string o3)
        {
            var stack = BuildStack(new[] {o1, o2, o3});
            var sortedBids = stack.Item1;
            var sortedAsks = stack.Item2;

            var bidsQ = sortedBids.Sum(b => b.Quantity);
            var asksQ = sortedAsks.Sum(a => a.Quantity);
            Assert.That(bidsQ == asksQ, "Invalid test, total bid and ask quantities must be equal");

            var matcher = CreateMatcher();
            var matches = matcher.Match(sortedBids, sortedAsks);

            AssertThatAllOrdersFullyMatched(sortedBids, sortedAsks, matches);
        }

        [Test,
         TestCase("Limit;TEST;Bid;10@10", "Limit;TEST;Bid;10@10", "Limit;TEST;Ask;40@10"),
         TestCase("Limit;TEST;Ask;10@10", "Limit;TEST;Ask;10@10", "Limit;TEST;Bid;40@10"),
         TestCase("Limit;TEST;Ask;10@10", "Limit;TEST;Ask;20@10", "Limit;TEST;Bid;40@10")
        ]
        public void LargerOrderPartiallyMatchedByTwoSmallerOrders(string o1,
                                                                  string o2,
                                                                  string o3)
        {
            // o1 and o2 partially match o3
            var stack = BuildStack(new[] { o1, o2, o3 });
            var sortedBids = stack.Item1;
            var sortedAsks = stack.Item2;

            var bidsQ = sortedBids.Sum(b => b.Quantity);
            var asksQ = sortedAsks.Sum(a => a.Quantity);
            Assert.That(bidsQ != asksQ,
                        "Invalid test, total bid and ask quantities must not be equal");
            
            var matcher = CreateMatcher();
            var matches = matcher.Match(sortedBids, sortedAsks);
            
            var matchQ = Math.Min(bidsQ, asksQ);
            var expectedMatches = sortedBids.Concat(sortedAsks).Select(
                o => new OrderMatch
                    {
                        Contract = o.Contract,
                        ClOrdID = o.ClOrdID,
                        MatchedQuantity = Math.Min(o.Quantity, matchQ),
                        RemainingQuantity = Math.Max(o.Quantity - matchQ, 0),
                        OrderID = o.ID,
                        Price = o.Price,
                        MatchType = o.Quantity > matchQ ? MatchType.Partial : MatchType.Full
                    }).ToArray();

            AssertMatchesAsExpected(expectedMatches, matches);
        }

        [Test,
        TestCase("10@10,10@10",       "10@10,10@10"),
        TestCase("10@10,10@10,10@10", "10@10,10@10,10@10"),
        TestCase("20@10,30@10",       "40@10,10@10"),
        TestCase("40@10,10@10",       "20@10,30@10"),
        TestCase("20@10,30@10,50@10", "40@10,60@10"),
        TestCase("20@10,30@10,50@10", "100@10"),
        ]
        public void MultipleOrdersOnEachSideFullyMatch(string bids, string asks)
        {
            var bidStrings = bids.Split(new[] { ',' }).Select(s => "Limit;TEST;Bid;" + s);
            var askStrings = asks.Split(new[] { ',' }).Select(s => "Limit;TEST;Ask;" + s);
            var stack = BuildStack(bidStrings.Concat(askStrings));
            var sortedBids = stack.Item1;
            var sortedAsks = stack.Item2;
            var bidsQ = sortedBids.Sum(b => b.Quantity);
            var asksQ = sortedAsks.Sum(a => a.Quantity);
            Assert.That(bidsQ == asksQ, "Invalid test, total bid and ask quantities must be equal");

            var matcher = CreateMatcher();
            var matches = matcher.Match(sortedBids, sortedAsks);

            AssertThatAllOrdersFullyMatched(sortedBids, sortedAsks, matches);
        }

        [Test,
         TestCase("Limit;TEST;Bid;20@10", "Limit;TEST;Ask;10@10.1"),
         TestCase("Limit;TEST;Bid;10@10", "Limit;TEST;Ask;20@10.1")]
        public void TwoOrdersWithPriceSpreadDoNotMatch(string bid, string ask)
        {
            var stack = BuildStack(new[] { bid, ask });
            var sortedBids = stack.Item1;
            var sortedAsks = stack.Item2;
            Assert.AreEqual(1, sortedBids.Count);
            Assert.AreEqual(1, sortedAsks.Count);
            var b = sortedBids.ElementAt(0);
            var a = sortedAsks.ElementAt(0);
            Assert.That(b.Price < a.Price, "Invalid test, bid price must be less than ask price");

            var matcher = CreateMatcher();
            var matches = matcher.Match(sortedBids, sortedAsks);

            Assert.IsEmpty(matches, "There should not be any matches");
        }

        [Test,
        TestCase("Limit;TEST;Bid;20@10", "Limit;TEST;Ask;10@10", "Limit;TEST;Ask;10@10.1"),
         TestCase("Limit;TEST;Bid;10@10", "Limit;TEST;Ask;20@10", "Limit;TEST;Bid;10@9.9")
        ]
        public void WorsePriceNotMatchedWhenPartialMatchLeavesQuantityOnOppositeSide(string o1,
                                                                                     string o2,
                                                                                     string o3)
        {
            // o3 (ID=3) should be left unmatched
            var stack = BuildStack(new[] {o1, o2, o3});
            var sortedBids = stack.Item1;
            var sortedAsks = stack.Item2;

            var matcher = CreateMatcher();
            var matches = matcher.Match(sortedBids, sortedAsks);

            Assert.AreEqual(2, matches.Count, "Incorrect number of matches");
            Assert.IsNull(matches.SingleOrDefault(m => m.OrderID == 3),
                          "Order 3 should not have matched");
        }

        // TODO Test market order matching if we ever support them
        // Price, priority vs limit orders etc

        private static Tuple<SortedSet<IOrder>, SortedSet<IOrder>> BuildStack(
            IEnumerable<string> orderStrings)
        {
            long id = 0;
            var orders = orderStrings.Select(s => FakeOrder.CreateOrderFromString(++id, s)).ToArray();
            var bids = orders.Where(o => o.MarketSide == MarketSide.Bid);
            var asks = orders.Where(o => o.MarketSide == MarketSide.Ask);
            return Tuple.Create(DefaultSort(bids), DefaultSort(asks));
        }

        private static SortedSet<IOrder> DefaultSort(IEnumerable<IOrder> orders)
        {
            return new SortedSet<IOrder>(orders, new OrderStack.StandardOrderComparer());
        }

        private static StandardOrderMatcher CreateMatcher()
        {
            return new StandardOrderMatcher();
        }

        private static void AssertThatAllOrdersFullyMatched(IEnumerable<IOrder> bids,
                                                            IEnumerable<IOrder> asks,
                                                            ICollection<OrderMatch> matches)
        {
            var expectedMatches = bids.Concat(asks).Select(
                o => new OrderMatch
                    {
                        OrderID = o.ID,
                        Price = o.Price,
                        MatchType = MatchType.Full,
                        MatchedQuantity = o.Quantity,
                        RemainingQuantity = 0m,
                        Contract = o.Contract
                    }).ToArray();

            AssertMatchesAsExpected(expectedMatches, matches);
        }

        private static void AssertMatchesAsExpected(ICollection<OrderMatch> expectedMatches,
                                                    ICollection<OrderMatch> actualMatches)
        {
            Assert.AreEqual(expectedMatches.Count,
                            actualMatches.Count,
                            "Number of actual matches different from number of expected matches");
            foreach (var expectedMatch in expectedMatches)
            {
                try
                {
                    var actual = actualMatches.Single(a => a.OrderID == expectedMatch.OrderID);
                    AssertEqual(expectedMatch, actual);
                }
                catch (InvalidOperationException e)
                {
                    Assert.Fail("Unable to find single actual match for expected match " + e);
                }
            }
        }

        private static void AssertEqual(OrderMatch e, OrderMatch a)
        {
            const string messageBase = "Expected and actual matches differ on ";
            Assert.AreEqual(e.OrderID, a.OrderID, messageBase + "OrderID");
            Assert.AreEqual(e.Price, a.Price, messageBase + "Price");
            Assert.AreEqual(e.MatchedQuantity, a.MatchedQuantity, "MatchedQuantity");
            Assert.AreEqual(e.RemainingQuantity, a.RemainingQuantity, "RemainingQuantity");
            Assert.AreEqual(e.Contract, a.Contract, "Contract");
            Assert.AreEqual(e.MatchType, a.MatchType, "MatchType");
        }
    }
}
