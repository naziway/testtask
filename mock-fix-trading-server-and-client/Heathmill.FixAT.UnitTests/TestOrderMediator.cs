using System;
using Heathmill.FixAT.Domain;
using Heathmill.FixAT.Server;
using Moq;
using NUnit.Framework;

namespace Heathmill.FixAT.UnitTests
{
    [TestFixture]
    public class TestOrderMediator
    {
        // TODO Increase coverage for the mediator

        [TestFixtureSetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void CancellingAnOrderWhenYouAreTheOwningSessionSucceeds()
        {
            var mockRepository = new Mock<IOrderRepository>();
            var o = DefaultOrder(1);

            mockRepository.Setup(r => r.AddOrder(o.ID,
                                                 o.Contract,
                                                 o.OrderType,
                                                 o.MarketSide,
                                                 o.Price,
                                                 o.Quantity,
                                                 o.ClOrdID,
                                                 o.Account))
                          .Returns(o);

            mockRepository.Setup(r => r.DeleteOrder(o.ID)).Returns(o);

            Action<OrderMatch, FixSessionID> emptyMatchCallback = (m, s) => { };
            var mediator = new OrderMediator(mockRepository.Object, emptyMatchCallback);
            var fakeSessionId = new FixSessionID();

            var addedOrder = mediator.AddOrder(fakeSessionId,
                                               o.OrderType,
                                               o.Contract.Symbol,
                                               o.MarketSide,
                                               o.ClOrdID,
                                               o.Account,
                                               o.Quantity,
                                               o.Price);

            Assert.AreEqual(o.ID, addedOrder.ID);
            Assert.AreEqual(o.ClOrdID, addedOrder.ClOrdID);
            
            var cancelledOrder = mediator.CancelOrder(addedOrder.ID, fakeSessionId);
            Assert.IsNotNull(cancelledOrder, "Cancelled order should not be null");

            mockRepository.Verify(r => r.DeleteOrder(o.ID), Times.Once());

            Assert.AreEqual(o.ID, cancelledOrder.ID);
            Assert.AreEqual(o.ClOrdID, cancelledOrder.ClOrdID);
        }

        [Test]
        public void CancellingAnOrderWhenYouAreNotTheOwningSessionFails()
        {
            var mockRepository = new Mock<IOrderRepository>();
            var o = DefaultOrder(1);

            mockRepository.Setup(r => r.AddOrder(o.ID,
                                                 o.Contract,
                                                 o.OrderType,
                                                 o.MarketSide,
                                                 o.Price,
                                                 o.Quantity,
                                                 o.ClOrdID,
                                                 o.Account))
                          .Returns(o);

            Action<OrderMatch, FixSessionID> emptyMatchCallback = (m, s) => { };
            var mediator = new OrderMediator(mockRepository.Object, emptyMatchCallback);
            
            var fakeSessionId1 = new FixSessionID();
            var fakeSessionId2 = new FixSessionID();
            Assert.AreNotEqual(fakeSessionId1, fakeSessionId2);

            var addedOrder = mediator.AddOrder(fakeSessionId1,
                                               o.OrderType,
                                               o.Contract.Symbol,
                                               o.MarketSide,
                                               o.ClOrdID,
                                               o.Account,
                                               o.Quantity,
                                               o.Price);

            Assert.Throws<FixATServerException>(
                () => mediator.CancelOrder(addedOrder.ID, fakeSessionId2));

            mockRepository.Verify(r => r.DeleteOrder(o.ID), Times.Never());
        }

        private FakeOrder DefaultOrder(long orderID)
        {
            return FakeOrder.CreateOrderFromString(orderID, "Limit;TEST;Bid;10@100");
        }
    }
}
