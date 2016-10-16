using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Heathmill.FixAT.Domain;
using MatchType = Heathmill.FixAT.Domain.MatchType;

namespace Heathmill.FixAT.Server
{
    /// <summary>
    ///     Mediator for communication with the order respository.
    ///     If you want to do something with orders then this is where to do it.
    /// </summary>
    internal class OrderMediator
    {
        public enum OrderCancelRejectReason
        {
            PermissionDenied,
            OrderNotFound
        }

        public const string RejectReasonExceptionString = "RejectReason";

        private readonly Dictionary<long, FixSessionID> _orderOwners =
            new Dictionary<long, FixSessionID>();

        private readonly IOrderRepository _orderRepository;
        private readonly Action<OrderMatch, FixSessionID> _orderMatchCallback;

        // A production server would not doubt need better order ID generation
        private long _orderID;

        public OrderMediator(IOrderRepository orderRepository,
                             Action<OrderMatch, FixSessionID> orderMatchCallback)
        {
            _orderRepository = orderRepository;
            _orderMatchCallback = orderMatchCallback;
            _orderRepository.OrdersMatched += OnOrdersMatched;
        }

        /// <summary>
        /// Adds an order to the system with the given properties
        /// </summary>
        /// <param name="sessionID">The FIX Session ID</param>
        /// <param name="orderType">
        /// The type of order, currently only Limit orders are supported
        /// </param>
        /// <param name="symbol">The symbol for the order</param>
        /// <param name="marketSide">The side of the market for the order</param>
        /// <param name="clOrdID">The FIX ClOrdID for the order, set by the client</param>
        /// <param name="account">The trading account associated with the order</param>
        /// <param name="quantity">The quantity of the order</param>
        /// <param name="price">The price of the order, may be null for market orders</param>
        /// <returns>The new order after it has been added</returns>
        /// <exception cref="FixATServerException">If the order is rejected</exception>
        public IOrder AddOrder(FixSessionID sessionID,
                               OrderType orderType,
                               string symbol,
                               MarketSide marketSide,
                               string clOrdID,
                               TradingAccount account,
                               decimal quantity,
                               decimal? price = null)
        {
            // A more complete system would look the contract up in a contract store
            var contract = new Contract(symbol);

            // TODO Replace this with a better mechanism (esp if more order types are supported)
            decimal orderPrice;
            switch (orderType)
            {
                case OrderType.Limit:
                    {
                        if (!price.HasValue)
                            throw new FixATServerException("Limit order must specify a price");
                        orderPrice = price.Value;
                        break;
                    }

                    // Uncomment this if and when market orders are supported
                    //case OrderType.Market:
                    //    {
                    //        if (price.HasValue)
                    //            throw new FixATServerException(
                    //                "Market order should not have a specified price");

                    //        orderPrice = GetMarketPrice(contract, marketSide);
                    //        break;
                    //    }

                default:
                    throw new FixATServerException(
                        string.Format("Order Type {0} not supported", orderType));
            }

            if (OrderWouldLeadToACrossedMarket(marketSide, contract, orderPrice))
                throw new FixATServerException("Order would lead to a crossed market");

            var order = _orderRepository.AddOrder(CreateOrderID(),
                                                  contract,
                                                  orderType,
                                                  marketSide,
                                                  orderPrice,
                                                  quantity,
                                                  clOrdID,
                                                  account);

            _orderOwners[order.ID] = sessionID;

            return order;
        }

        private bool OrderWouldLeadToACrossedMarket(MarketSide marketSide,
                                                    Contract contract,
                                                    decimal orderPrice)
        {
            var bestOppositePrice = _orderRepository.GetBestPrice(contract,
                                                                  marketSide.Opposite());
            var wouldCross = marketSide == MarketSide.Bid
                                 ? orderPrice > bestOppositePrice
                                 : orderPrice < bestOppositePrice;
            return wouldCross;
        }

        /// <summary>
        /// Gets the order with a given ID
        /// </summary>
        /// <param name="orderID">The ID of the order to get</param>
        /// <returns>The order, will not be null</returns>
        /// <exception cref="FixATServerException">If the order cannot be found</exception>
        public IOrder GetOrder(long orderID)
        {
            return _orderRepository.GetOrder(orderID);
        }

        /// <summary>
        /// Checks the given FIX session is the order owner before deleting an order
        /// </summary>
        /// <param name="orderID">The order to delete</param>
        /// <param name="sessionID">The ID of the FIX session</param>
        /// <returns>The order which has been cancelled</returns>
        /// <exception cref="FixATServerException">If the FIX session does not own the order</exception>
        public IOrder CancelOrder(long orderID, FixSessionID sessionID)
        {
            var owner = _orderOwners[orderID];
            if (!owner.Equals(sessionID))
            {
                var e = new FixATServerException(
                    string.Format("Unable to cancel order {0}, permission denied", orderID));
                e.Data[RejectReasonExceptionString] = OrderCancelRejectReason.PermissionDenied;
                throw e;
            }

            var deletedOrder = DeleteOrder(orderID);
            if (deletedOrder == null)
            {
                var e = new FixATServerException(
                    string.Format("Unable to cancel order {0}, order not found", orderID));
                e.Data[RejectReasonExceptionString] = OrderCancelRejectReason.OrderNotFound;
                throw e;
            }
            return deletedOrder;
        }

        /// <summary>
        /// Deletes all the orders owned by the given FIX session
        /// </summary>
        public void DeleteAllOrders(FixSessionID sessionID)
        {
            // TODO This could get racey
            var sessionsOrders =
                _orderOwners.Where(p => sessionID.Equals(p.Value)).Select(p => p.Key).ToList();
            foreach (var orderID in sessionsOrders)
            {
                DeleteOrder(orderID);
            }
        }

        /// <summary>
        /// Gets all the orders in the system
        /// </summary>
        public List<IOrder> GetAllOrders()
        {
            return _orderRepository.GetAllOrders().ToList();
        }

        /// <summary>
        /// Deletes an order
        /// </summary>
        /// <returns>The deleted order, will be null of the order did not exist</returns>
        /// <remarks>
        /// Use CancelOrder if you need to verify that the FIX session deleting the order owns it
        /// </remarks>
        public IOrder DeleteOrder(long orderID)
        {
            var deleted = _orderRepository.DeleteOrder(orderID);
            if (deleted != null)
            {
                _orderOwners.Remove(orderID);
            }
            return deleted;
        }

        /// <summary>
        /// Carries out order matching for the given contract
        /// </summary>
        public void MatchOrders(string symbol)
        {
            // This would be lookup on the contract repo were this a production server
            var contract = new Contract(symbol);
            _orderRepository.MatchOrders(contract);
        }

        private long CreateOrderID()
        {
            return Interlocked.Increment(ref _orderID);
        }

        private void OnOrdersMatched(OrdersMatchedEventArgs e)
        {
            foreach (var match in e.OrderMatches)
            {
                var ownerSession = _orderOwners[match.OrderID];
                if (match.MatchType == MatchType.Full)
                {
                    _orderOwners.Remove(match.OrderID);
                }
                _orderMatchCallback(match, ownerSession);
            }
        }
    }
}