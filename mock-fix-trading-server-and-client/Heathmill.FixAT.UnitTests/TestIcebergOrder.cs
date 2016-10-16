using System;
using System.Collections.Generic;
using System.Globalization;
using Heathmill.FixAT.Client;
using Heathmill.FixAT.Client.Model;
using Heathmill.FixAT.Domain;
using Moq;
using NUnit.Framework;

namespace Heathmill.FixAT.UnitTests
{
    public class TestIcebergOrder
    {
        [Test]
        public void IcebergOrderWithZeroQuantityThrows()
        {
            var mockServer = new Mock<IServerFacade>();
            const decimal zeroQuantity = 0m;
            Assert.Throws<ApplicationException>(() => new IcebergOrder(mockServer.Object,
                                                                       "Symbol",
                                                                       "ClOrdID",
                                                                       MarketSide.Bid,
                                                                       zeroQuantity,
                                                                       10m,
                                                                       100m,
                                                                       0m));

            mockServer.Verify(s => s.CreateOrder(It.IsAny<OrderRecord>()), Times.Never());
        }

        [Test]
        public void IcebergOrderWithClipSizeGreaterThanTotalQuantityThrows()
        {
            var mockServer = new Mock<IServerFacade>();
            const decimal quantity = 10m;
            const decimal clipSize = quantity + 1;
            Assert.Throws<ApplicationException>(() => new IcebergOrder(mockServer.Object,
                                                                       "Symbol",
                                                                       "ClOrdID",
                                                                       MarketSide.Bid,
                                                                       quantity,
                                                                       clipSize,
                                                                       100m,
                                                                       0m));

            mockServer.Verify(s => s.CreateOrder(It.IsAny<OrderRecord>()), Times.Never());
        }

        [Test]
        public void WhenAnIcebergOrderActivatesItCreatesCorrectOrderDetails()
        {
            var expected = DefaultFakeOrderRecord();

            var createdOrders = new List<OrderRecord>();
            var mockServer = new Mock<IServerFacade>();
            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>()))
                      .Returns(true)
                      .Callback((OrderRecord o) => createdOrders.Add(o));

            var clipSize = expected.Quantity;
            const decimal priceDelta = 0;
            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipSize,
                                           expected.Price,
                                           priceDelta);

            iceberg.Activate();

            mockServer.Verify(s => s.CreateOrder(It.IsAny<OrderRecord>()),
                              Times.Once());

            Assert.AreEqual(1, createdOrders.Count, "Incorrect number of market orders created");
            var actual = createdOrders[0];

            CompareOrderRecords(expected, actual);
        }

        [Test]
        public void ThrowsIfActivatedWhenPendingMarketOrderAcceptance()
        {
            var expected = DefaultFakeOrderRecord();
            var mockServer = new Mock<IServerFacade>();
            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>())).Returns(true);

            var clipSize = expected.Quantity;
            const decimal priceDelta = 0;
            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipSize,
                                           expected.Price,
                                           priceDelta);

            iceberg.Activate();

            mockServer.Verify(s => s.CreateOrder(It.IsAny<OrderRecord>()),
                              Times.Once());

            Assert.Throws<ApplicationException>(iceberg.Activate);
        }

        [Test]
        public void WhenMarketOrderAcceptedOrderIDOfIcebergIsUpdated()
        {
            var expected = DefaultFakeOrderRecord();
            var mockServer = new Mock<IServerFacade>();
            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>())).Returns(true);

            var clipSize = expected.Quantity;
            const decimal priceDelta = 0;
            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipSize,
                                           expected.Price,
                                           priceDelta);

            iceberg.Activate();

            mockServer.Verify(s => s.CreateOrder(It.IsAny<OrderRecord>()),
                              Times.Once());

            const string orderID = "FakeOrderID";
            iceberg.ActivatedMarketOrderAccepted(orderID);
            Assert.AreEqual(IcebergOrder.ActivationState.Active,
                            iceberg.State,
                            "Order should be active");
            Assert.AreEqual(orderID,
                            iceberg.OrderID,
                            "OrderID not set correct when market order accepted");
        }

        [Test]
        public void ThrowsIfActivatedWhenAlreadyActive()
        {
            var expected = DefaultFakeOrderRecord();
            var mockServer = new Mock<IServerFacade>();
            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>())).Returns(true);

            var clipSize = expected.Quantity;
            const decimal priceDelta = 0;
            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipSize,
                                           expected.Price,
                                           priceDelta);

            iceberg.Activate();

            mockServer.Verify(s => s.CreateOrder(It.IsAny<OrderRecord>()),
                              Times.Once());

            var idGen = new SimpleOrderIDGenerator();
            iceberg.ActivatedMarketOrderAccepted(idGen.GetID());

            Assert.Throws<ApplicationException>(iceberg.Activate);
        }

        [Test]
        public void ThrowsIfSuspendedBeforeMarketOrderAccepted()
        {
            var expected = DefaultFakeOrderRecord();
            var mockServer = new Mock<IServerFacade>();
            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>())).Returns(true);

            var clipSize = expected.Quantity;
            const decimal priceDelta = 0;
            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipSize,
                                           expected.Price,
                                           priceDelta);

            iceberg.Activate();

            mockServer.Verify(s => s.CreateOrder(It.IsAny<OrderRecord>()),
                              Times.Once());

            Assert.Throws<ApplicationException>(iceberg.Suspend);
        }

        [Test]
        public void WhenSuspendedCausesOrderCancel()
        {
            const string orderID = "FakeOrderID";

            var expected = DefaultFakeOrderRecord();
            var mockServer = new Mock<IServerFacade>();

            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>())).Returns(true);
            mockServer.Setup(s =>
                             s.CancelOrder(expected.Symbol, expected.ClOrdID, expected.Side, orderID))
                      .Returns(true);

            var clipSize = expected.Quantity;
            const decimal priceDelta = 0;
            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipSize,
                                           expected.Price,
                                           priceDelta);

            iceberg.Activate();

            mockServer.Verify(s => s.CreateOrder(It.IsAny<OrderRecord>()),
                              Times.Once());

            iceberg.ActivatedMarketOrderAccepted(orderID);
            Assert.AreEqual(IcebergOrder.ActivationState.Active,
                            iceberg.State,
                            "Order should be active");

            iceberg.Suspend();
            mockServer.Verify(
                s => s.CancelOrder(expected.Symbol, expected.ClOrdID, expected.Side, orderID),
                Times.Once());
            Assert.AreEqual(IcebergOrder.ActivationState.PendingSuspension,
                            iceberg.State,
                            "Order should not be active");
        }

        [Test]
        public void SuspendWhenAlreadyPendingSuspensionThrows()
        {
            const string orderID = "FakeOrderID";

            var expected = DefaultFakeOrderRecord();
            var mockServer = new Mock<IServerFacade>();

            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>())).Returns(true);
            mockServer.Setup(s =>
                             s.CancelOrder(expected.Symbol, expected.ClOrdID, expected.Side, orderID))
                      .Returns(true);

            var clipSize = expected.Quantity;
            const decimal priceDelta = 0;
            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipSize,
                                           expected.Price,
                                           priceDelta);

            iceberg.Activate();

            mockServer.Verify(s => s.CreateOrder(It.IsAny<OrderRecord>()),
                              Times.Once());

            iceberg.ActivatedMarketOrderAccepted(orderID);
            Assert.AreEqual(IcebergOrder.ActivationState.Active,
                            iceberg.State,
                            "Order should be active");

            iceberg.Suspend();
            Assert.Throws<ApplicationException>(iceberg.Suspend);

            mockServer.Verify(
                s => s.CancelOrder(expected.Symbol, expected.ClOrdID, expected.Side, orderID),
                Times.Once());
            Assert.AreEqual(IcebergOrder.ActivationState.PendingSuspension,
                            iceberg.State,
                            "Order should not be active");
        }

        [Test]
        public void SuspendWhenAlreadySuspendedThrows()
        {
            const string orderID = "FakeOrderID";

            var expected = DefaultFakeOrderRecord();
            var mockServer = new Mock<IServerFacade>();

            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>())).Returns(true);
            mockServer.Setup(s =>
                             s.CancelOrder(expected.Symbol, expected.ClOrdID, expected.Side, orderID))
                      .Returns(true);

            var clipSize = expected.Quantity;
            const decimal priceDelta = 0;
            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipSize,
                                           expected.Price,
                                           priceDelta);

            iceberg.Activate();

            mockServer.Verify(s => s.CreateOrder(It.IsAny<OrderRecord>()),
                              Times.Once());

            iceberg.ActivatedMarketOrderAccepted(orderID);
            Assert.AreEqual(IcebergOrder.ActivationState.Active,
                            iceberg.State,
                            "Order should be active");

            iceberg.Suspend();
            iceberg.MarketOrderCanceled();

            Assert.AreEqual(IcebergOrder.ActivationState.Suspended,
                            iceberg.State,
                            "Order should be suspended");

            Assert.Throws<ApplicationException>(iceberg.Suspend);

            mockServer.Verify(
                s => s.CancelOrder(expected.Symbol, expected.ClOrdID, expected.Side, orderID),
                Times.Once());
            Assert.AreEqual(IcebergOrder.ActivationState.Suspended,
                            iceberg.State,
                            "Order should be suspended");
        }

        [Test]
        public void SuspendingAnInactiveOrderThrows()
        {
            var expected = DefaultFakeOrderRecord();
            var mockServer = new Mock<IServerFacade>();

            var clipSize = expected.Quantity;
            const decimal priceDelta = 0;
            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipSize,
                                           expected.Price,
                                           priceDelta);

            Assert.Throws<ApplicationException>(iceberg.Suspend);
        }

        [Test]
        public void SuspendingAnAlreadySuspendedOrderThrows()
        {
            const string orderID = "FakeOrderID";

            var expected = DefaultFakeOrderRecord();
            var mockServer = new Mock<IServerFacade>();

            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>())).Returns(true);
            mockServer.Setup(s =>
                             s.CancelOrder(expected.Symbol, expected.ClOrdID, expected.Side, orderID))
                      .Returns(true);

            var clipSize = expected.Quantity;
            const decimal priceDelta = 0;
            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipSize,
                                           expected.Price,
                                           priceDelta);

            iceberg.Activate();

            mockServer.Verify(s => s.CreateOrder(It.IsAny<OrderRecord>()),
                              Times.Once());

            iceberg.ActivatedMarketOrderAccepted(orderID);

            iceberg.Suspend();
            mockServer.Verify(
                s => s.CancelOrder(expected.Symbol, expected.ClOrdID, expected.Side, orderID),
                Times.Once());
            Assert.AreEqual(IcebergOrder.ActivationState.PendingSuspension,
                            iceberg.State,
                            "Order should not be active");

            Assert.Throws<ApplicationException>(iceberg.Suspend);
        }

        [Test]
        public void PartialFillWhileOrderInactiveThrows()
        {
            var expected = DefaultFakeOrderRecord();
            var mockServer = new Mock<IServerFacade>();

            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>())).Returns(true);

            var clipSize = expected.Quantity;
            const decimal priceDelta = 0;
            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipSize,
                                           expected.Price,
                                           priceDelta);

            Assert.AreEqual(IcebergOrder.ActivationState.Suspended,
                            iceberg.State,
                            "Order should not be active");

            Assert.Throws<ApplicationException>(() => iceberg.OnPartialFill(1));
        }

        [Test]
        public void TotalFillWhileOrderInactiveThrows()
        {
            var expected = DefaultFakeOrderRecord();
            var mockServer = new Mock<IServerFacade>();

            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>())).Returns(true);

            var clipSize = expected.Quantity;
            const decimal priceDelta = 0;
            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipSize,
                                           expected.Price,
                                           priceDelta);

            Assert.AreEqual(IcebergOrder.ActivationState.Suspended,
                            iceberg.State,
                            "Order should not be active");

            Assert.Throws<ApplicationException>(iceberg.OnTotalFill);
        }

        [Test]
        public void PartialFillAdjustsCurrentAndRemainingQuantity()
        {
            const decimal totalQ = 20;
            const decimal clipQ = 10;

            var expected = DefaultFakeOrderRecord(quantity: totalQ);
            var mockServer = new Mock<IServerFacade>();
            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>())).Returns(true);

            const decimal priceDelta = 0;
            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipQ,
                                           expected.Price,
                                           priceDelta);

            iceberg.Activate();

            const string orderID = "FakeOrderID";
            iceberg.ActivatedMarketOrderAccepted(orderID);

            const decimal fillQ = 5;
            iceberg.OnPartialFill(fillQ);

            mockServer.Verify(s => s.CreateOrder(It.IsAny<OrderRecord>()),
                              Times.Once());

            Assert.AreEqual(clipQ - fillQ,
                            iceberg.CurrentQuantity,
                            "Current quantity not updated correctly");
            Assert.AreEqual(totalQ - fillQ,
                            iceberg.RemainingQuantity,
                            "Remaining quantity not updated correctly");
        }

        [Test]
        public void PartialFillAdjustsLastTradedTime()
        {
            const decimal totalQ = 20;
            const decimal clipQ = 10;

            var expected = DefaultFakeOrderRecord(quantity: totalQ);
            var mockServer = new Mock<IServerFacade>();
            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>())).Returns(true);

            const decimal priceDelta = 0;
            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipQ,
                                           expected.Price,
                                           priceDelta);

            iceberg.Activate();

            const string orderID = "FakeOrderID";
            iceberg.ActivatedMarketOrderAccepted(orderID);

            const decimal fillQ = 5;

            Assert.IsNull(iceberg.LastTradedTime);
            iceberg.OnPartialFill(fillQ);
            Assert.NotNull(iceberg.LastTradedTime);

            mockServer.Verify(s => s.CreateOrder(It.IsAny<OrderRecord>()),
                              Times.Once());
        }

        [Test]
        [TestCase(10, 10)]
        [TestCase(10, 11)]
        public void PartialFillForGreaterThanOrEqualToCurrentQuantityThrows(decimal orderQty,
                                                                            decimal fillQty)
        {
            var totalQ = orderQty;
            var clipQ = orderQty;

            var expected = DefaultFakeOrderRecord(quantity: totalQ);
            var mockServer = new Mock<IServerFacade>();
            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>())).Returns(true);

            const decimal priceDelta = 0;
            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipQ,
                                           expected.Price,
                                           priceDelta);

            iceberg.Activate();

            const string orderID = "FakeOrderID";
            iceberg.ActivatedMarketOrderAccepted(orderID);

            Assert.Throws<ApplicationException>(() => iceberg.OnPartialFill(fillQty));

            mockServer.Verify(s => s.CreateOrder(It.IsAny<OrderRecord>()),
                              Times.Once());
        }

        [Test]
        public void TotalFillWithNoDeltaReplenishesWithClipSizePriceStaysSame()
        {
            const decimal totalQ = 20;
            const decimal clipQ = 10;

            var expected = DefaultFakeOrderRecord(quantity: totalQ);
            var mockServer = new Mock<IServerFacade>();
            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>())).Returns(true);

            const decimal priceDelta = 0;
            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipQ,
                                           expected.Price,
                                           priceDelta);

            iceberg.Activate();

            var idGen = new SimpleOrderIDGenerator();
            iceberg.ActivatedMarketOrderAccepted(idGen.GetID());

            iceberg.OnTotalFill();
            iceberg.ActivatedMarketOrderAccepted(idGen.GetID());

            mockServer.Verify(s => s.CreateOrder(It.IsAny<OrderRecord>()),
                              Times.Exactly(2));

            Assert.AreEqual(clipQ, iceberg.CurrentQuantity, "Order not refilled to clip size");
            Assert.AreEqual(totalQ - clipQ,
                            iceberg.RemainingQuantity,
                            "Remaining quantity should be total minus one clip");
            Assert.AreEqual(expected.Price, iceberg.CurrentPrice, "Current price not as expected");
            Assert.AreEqual(iceberg.InitialPrice,
                            iceberg.CurrentPrice,
                            "Current price should equal initial price");
        }

        [Test]
        [TestCase(MarketSide.Bid, 1, 10, 9)]
        [TestCase(MarketSide.Bid, -1, 10, 11)]
        [TestCase(MarketSide.Ask, 1, 10, 11)]
        [TestCase(MarketSide.Ask, -1, 10, 9)]
        public void TotalFillWithDeltaAdjustsPriceAsWellAsRefilling(MarketSide side,
                                                                    decimal delta,
                                                                    decimal price,
                                                                    decimal priceAfterFill)
        {
            const decimal totalQ = 20;
            const decimal clipQ = 10;

            var expected = DefaultFakeOrderRecord(quantity: totalQ, price: price, side: side);
            var mockServer = new Mock<IServerFacade>();
            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>())).Returns(true);

            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipQ,
                                           price,
                                           delta);

            iceberg.Activate();

            var idGen = new SimpleOrderIDGenerator();
            iceberg.ActivatedMarketOrderAccepted(idGen.GetID());

            iceberg.OnTotalFill();
            iceberg.ActivatedMarketOrderAccepted(idGen.GetID());

            mockServer.Verify(s => s.CreateOrder(It.IsAny<OrderRecord>()),
                              Times.Exactly(2));

            Assert.AreEqual(clipQ, iceberg.CurrentQuantity, "Order not refilled to clip size");
            Assert.AreEqual(totalQ - clipQ,
                            iceberg.RemainingQuantity,
                            "Remaining quantity should be total minus one clip");

            Assert.AreEqual(priceAfterFill,
                            iceberg.CurrentPrice,
                            "Current price not as expected after fill with delta");
            Assert.AreNotEqual(iceberg.InitialPrice,
                               iceberg.CurrentPrice,
                               "Current price should not equal initial price after fill with delta");
        }

        [Test]
        public void TwoRefillsWithDeltaAdjustsPriceTwiceAndReducesQtyTwice()
        {
            const decimal totalQ = 20;
            const decimal clipQ = 5;
            const decimal delta = 1;

            var expected = DefaultFakeOrderRecord(quantity: totalQ);
            var mockServer = new Mock<IServerFacade>();
            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>())).Returns(true);

            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipQ,
                                           expected.Price,
                                           delta);

            iceberg.Activate();

            var idGen = new SimpleOrderIDGenerator();
            iceberg.ActivatedMarketOrderAccepted(idGen.GetID());

            iceberg.OnTotalFill();
            iceberg.ActivatedMarketOrderAccepted(idGen.GetID());
            iceberg.OnTotalFill();
            iceberg.ActivatedMarketOrderAccepted(idGen.GetID());

            mockServer.Verify(s => s.CreateOrder(It.IsAny<OrderRecord>()),
                              Times.Exactly(3));

            Assert.AreEqual(expected.Price - (2*delta),
                            iceberg.CurrentPrice,
                            "Current price not as expected after two fills with delta");
            Assert.AreEqual(totalQ - (2*clipQ),
                            iceberg.RemainingQuantity,
                            "Remaining quantity not as expected after two fills");
            Assert.AreEqual(clipQ,
                            iceberg.CurrentQuantity,
                            "Current quantity not equal to clip size after two fills");
        }

        [Test]
        public void SecondTotalFillBeforeFirstFillMarketOrderAcceptedThrows()
        {
            const decimal totalQ = 20;
            const decimal clipQ = 5;
            const decimal delta = 1;

            var expected = DefaultFakeOrderRecord(quantity: totalQ);
            var mockServer = new Mock<IServerFacade>();
            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>())).Returns(true);

            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipQ,
                                           expected.Price,
                                           delta);

            iceberg.Activate();

            var idGen = new SimpleOrderIDGenerator();
            iceberg.ActivatedMarketOrderAccepted(idGen.GetID());

            iceberg.OnTotalFill();
            Assert.Throws<ApplicationException>(iceberg.OnTotalFill);

            mockServer.Verify(s => s.CreateOrder(It.IsAny<OrderRecord>()),
                              Times.Exactly(2));
        }

        [Test]
        public void RefillWithDeltaHasZeroPriceFloor()
        {
            const decimal totalQ = 20;
            const decimal clipQ = 10;
            const decimal price = 0.1m;
            const decimal delta = 0.2m;

            var expected = DefaultFakeOrderRecord(quantity: totalQ, price: price);
            var mockServer = new Mock<IServerFacade>();
            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>())).Returns(true);

            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipQ,
                                           price,
                                           delta);

            iceberg.Activate();

            var idGen = new SimpleOrderIDGenerator();
            iceberg.ActivatedMarketOrderAccepted(idGen.GetID());

            iceberg.OnTotalFill();
            iceberg.ActivatedMarketOrderAccepted(idGen.GetID());

            mockServer.Verify(s => s.CreateOrder(It.IsAny<OrderRecord>()),
                              Times.Exactly(2));

            Assert.AreEqual(0,
                            iceberg.CurrentPrice,
                            "Current price not oberying zero floor after fill with delta");
        }

        [Test]
        public void RefillWithLessRemainingQtyThanClipSizeRefillsToRemaining()
        {
            const decimal totalQ = 15;
            const decimal clipQ = 10;
            const decimal remainingQ = totalQ - clipQ;

            var expected = DefaultFakeOrderRecord(quantity: totalQ);
            var mockServer = new Mock<IServerFacade>();
            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>())).Returns(true);

            const decimal priceDelta = 0;
            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipQ,
                                           expected.Price,
                                           priceDelta);

            iceberg.Activate();

            var idGen = new SimpleOrderIDGenerator();
            iceberg.ActivatedMarketOrderAccepted(idGen.GetID());

            iceberg.OnTotalFill();
            iceberg.ActivatedMarketOrderAccepted(idGen.GetID());

            mockServer.Verify(s => s.CreateOrder(It.IsAny<OrderRecord>()),
                              Times.Exactly(2));

            Assert.AreEqual(remainingQ, iceberg.CurrentQuantity, "Order not refilled to correct");
            Assert.AreEqual(remainingQ,
                            iceberg.RemainingQuantity,
                            "Remaining quantity should be total minus one clip");
        }

        [Test]
        public void OrderNotRefilledAfterQuantityExhausted()
        {
            const decimal totalQ = 15;
            const decimal clipQ = 10;

            var expected = DefaultFakeOrderRecord(quantity: totalQ);
            var mockServer = new Mock<IServerFacade>();
            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>())).Returns(true);

            const decimal priceDelta = 0;
            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipQ,
                                           expected.Price,
                                           priceDelta);

            iceberg.Activate();

            var idGen = new SimpleOrderIDGenerator();
            iceberg.ActivatedMarketOrderAccepted(idGen.GetID());

            iceberg.OnTotalFill();
            iceberg.ActivatedMarketOrderAccepted(idGen.GetID());
            iceberg.OnTotalFill();

            mockServer.Verify(s => s.CreateOrder(It.IsAny<OrderRecord>()),
                              Times.Exactly(2));

            Assert.AreEqual(IcebergOrder.ActivationState.Suspended,
                            iceberg.State,
                            "Order should not be active after being exhausted");
            Assert.AreEqual(0, iceberg.CurrentQuantity, "Order should have no quantity left");
            Assert.AreEqual(0,
                            iceberg.RemainingQuantity,
                            "Order should have no quantity left");
        }

        [Test]
        public void PartialFillThenTotalFillOnlyRemovesAClipSizeOfQuantity()
        {
            const decimal totalQ = 25;
            const decimal clipQ = 10;

            var expected = DefaultFakeOrderRecord(quantity: totalQ);
            var mockServer = new Mock<IServerFacade>();
            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>())).Returns(true);

            const decimal priceDelta = 0;
            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipQ,
                                           expected.Price,
                                           priceDelta);

            iceberg.Activate();

            var idGen = new SimpleOrderIDGenerator();
            iceberg.ActivatedMarketOrderAccepted(idGen.GetID());

            iceberg.OnPartialFill(5);
            iceberg.OnTotalFill();

            mockServer.Verify(s => s.CreateOrder(It.IsAny<OrderRecord>()),
                              Times.Exactly(2));

            Assert.AreEqual(clipQ,
                            iceberg.CurrentQuantity,
                            "Order not refilled to correct clip size");
            Assert.AreEqual(totalQ - clipQ,
                            iceberg.RemainingQuantity,
                            "Remaining quantity should be total minus one clip");
        }

        [Test]
        public void CanSetNewValuesIfOrderNotYetActivated()
        {
            const decimal totalQ = 25;
            const decimal clipQ = 10;

            var expected = DefaultFakeOrderRecord(quantity: totalQ);
            var mockServer = new Mock<IServerFacade>();

            const decimal priceDelta = 0;
            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipQ,
                                           expected.Price,
                                           priceDelta);

            mockServer.Verify(s => s.CreateOrder(It.IsAny<OrderRecord>()), Times.Never());

            Assert.IsTrue(iceberg.CanSetNewValues(),
                          "Should be able to set values before activating");
        }

        [Test]
        public void ShouldNotBeAbleToSetValuesAfterActivatingButBeforeMarketOrderAccepted()
        {
            const decimal totalQ = 25;
            const decimal clipQ = 10;

            var expected = DefaultFakeOrderRecord(quantity: totalQ);
            var mockServer = new Mock<IServerFacade>();
            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>())).Returns(true);

            const decimal priceDelta = 0;
            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipQ,
                                           expected.Price,
                                           priceDelta);

            Assert.IsTrue(iceberg.CanSetNewValues(),
                          "Should be able to set values before activating");

            iceberg.Activate();

            Assert.IsFalse(iceberg.CanSetNewValues(),
                           "Should not be able to set values before market order is accepted");
        }

        [Test]
        public void ShouldBeAbleToSetNewValuesAfterMarketOrderAccepted()
        {
            const decimal totalQ = 25;
            const decimal clipQ = 10;

            var expected = DefaultFakeOrderRecord(quantity: totalQ);
            var mockServer = new Mock<IServerFacade>();
            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>())).Returns(true);

            const decimal priceDelta = 0;
            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipQ,
                                           expected.Price,
                                           priceDelta);

            iceberg.Activate();
            iceberg.ActivatedMarketOrderAccepted("OrderID");

            Assert.IsTrue(iceberg.CanSetNewValues(),
                          "Should be able to set values after market order is accepted");
        }

        [Test]
        public void SettingNewPriceBeforeActivatingShouldNotNeedToUpdateServerOrder()
        {
            const decimal totalQ = 25;
            const decimal clipQ = 10;

            var expected = DefaultFakeOrderRecord(quantity: totalQ);
            var mockServer = new Mock<IServerFacade>();

            const decimal priceDelta = 0;
            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipQ,
                                           expected.Price,
                                           priceDelta);

            var newPrice = expected.Price + 10;
            iceberg.SetNewOrderPrice(newPrice);
            Assert.AreEqual(newPrice, iceberg.CurrentPrice, "Price not updated correctly");

            mockServer.Verify(s => s.CreateOrder(It.IsAny<OrderRecord>()), Times.Never());
        }

        [Test]
        public void SettingNewPriceWhileWaitingForMarketOrderAcceptanceThrows()
        {
            const decimal totalQ = 25;
            const decimal clipQ = 10;

            var expected = DefaultFakeOrderRecord(quantity: totalQ);
            var mockServer = new Mock<IServerFacade>();
            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>())).Returns(true);

            const decimal priceDelta = 0;
            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipQ,
                                           expected.Price,
                                           priceDelta);

            iceberg.Activate();
            Assert.Throws<ApplicationException>(() => iceberg.SetNewOrderPrice(100));
        }

        private class UpdatedOrders
        {
            public readonly List<OrderRecord> OldRecords = new List<OrderRecord>();
            public readonly List<OrderRecord> NewRecords = new List<OrderRecord>();

            public void AddUpdate(OrderRecord old, OrderRecord updated)
            {
                OldRecords.Add(old);
                NewRecords.Add(updated);
            }
        }

        [Test]
        public void SettingNewPriceWhileOrderActiveResultsInMarketOrderCancelAndCreatesNewOrder()
        {
            const decimal totalQ = 25;
            const decimal clipQ = 10;

            const string orderID = "OrderID";

            var expected = DefaultFakeOrderRecord(quantity: totalQ);
            
            var newPrice = expected.Price + 10;

            var mockServer = new Mock<IServerFacade>();
            mockServer.Setup(s => s.CreateOrder(It.IsAny<OrderRecord>())).Returns(true);

            var updatedOrders = new UpdatedOrders();
            mockServer.Setup(s => s.UpdateOrder(It.IsAny<OrderRecord>(), It.IsAny<OrderRecord>()))
                      .Returns(true)
                      .Callback<OrderRecord, OrderRecord>(updatedOrders.AddUpdate);

            const decimal priceDelta = 0;
            var iceberg = new IcebergOrder(mockServer.Object,
                                           expected.Symbol,
                                           expected.ClOrdID,
                                           expected.Side,
                                           expected.Quantity,
                                           clipQ,
                                           expected.Price,
                                           priceDelta);

            iceberg.Activate();
            iceberg.ActivatedMarketOrderAccepted(orderID);

            iceberg.SetNewOrderPrice(newPrice);

            mockServer.Verify(s => s.CreateOrder(It.IsAny<OrderRecord>()),
                              Times.Once());

            mockServer.Verify(s => s.UpdateOrder(It.IsAny<OrderRecord>(), It.IsAny<OrderRecord>()),
                              Times.Once());

            Assert.AreEqual(1,
                            updatedOrders.OldRecords.Count,
                            "Incorrect number of old orders in update callback");
            var old = updatedOrders.OldRecords[0];
            Assert.AreEqual(expected.ClOrdID,
                            old.ClOrdID,
                            "Incorrect ClOrdID for old record when updating");

            Assert.AreEqual(1,
                            updatedOrders.NewRecords.Count,
                            "Incorrect number of new orders in update callback");
            var update = updatedOrders.NewRecords[0];
            Assert.AreEqual(newPrice, update.Price, "Updated order price incorrect");
        }


        private static OrderRecord DefaultFakeOrderRecord(MarketSide side = MarketSide.Bid,
                                                          decimal price = 100,
                                                          decimal quantity = 10)
        {
            return new OrderRecord
            {
                ClOrdID = "ClOrdID",
                LastUpdateTime = DateTime.UtcNow,
                OrderID = "",
                OrdType = OrderType.Limit,
                Price = price,
                Quantity = quantity,
                Side = side,
                Status = OrderStatus.New,
                Symbol = "Symbol"
            };
        }

        [Flags]
        enum OptionalComparisons
        {
            None = 0,
            OrderID = 1,
            OrderStatus = 2,
            LastUpdated = 4
        }

        private static void CompareOrderRecords(OrderRecord e,
                                                OrderRecord a,
                                                OptionalComparisons compFlags =
                                                    OptionalComparisons.None)
        {
            Assert.AreEqual(e.OrdType, a.OrdType, "OrdType is different");
            Assert.AreEqual(e.Price, a.Price, "Price is different");
            Assert.AreEqual(e.Quantity, a.Quantity, "Quantity is different");
            Assert.AreEqual(e.Side, a.Side, "Side is different");
            Assert.AreEqual(e.Symbol, a.Symbol, "Symbol is different");
            
            if ((compFlags & OptionalComparisons.LastUpdated) == OptionalComparisons.LastUpdated)
                Assert.AreEqual(e.LastUpdateTime, a.LastUpdateTime, "LastUpdateTime is different");
            if ((compFlags & OptionalComparisons.OrderID) == OptionalComparisons.OrderID)
                Assert.AreEqual(e.OrderID, a.OrderID, "OrderID is different");
            if ((compFlags & OptionalComparisons.OrderStatus) == OptionalComparisons.OrderStatus)
                Assert.AreEqual(e.Status, a.Status, "Status is different");
        }

        private class SimpleOrderIDGenerator
        {
            private int _id;

            public string GetID()
            {
                return (++_id).ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}
