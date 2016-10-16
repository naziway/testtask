using System;
using System.Collections.Generic;
using System.Linq;
using Heathmill.FixAT.Domain;
using Heathmill.FixAT.Utilities;

namespace Heathmill.FixAT.Server
{
    /// <summary>
    ///     The standard, default implementation of an order repository.
    ///     Keeps an order stack per contract.
    /// </summary>
    public class StandardOrderRepository : IOrderRepository
    {
        // TODO Proper market separation, each order stack is locked internally at the moment
        private readonly Dictionary<Contract, OrderStack> _market =
            new Dictionary<Contract, OrderStack>();

        private readonly IOrderMatcher _orderMatcher;

        public StandardOrderRepository(IOrderMatcher orderMatcher)
        {
            _orderMatcher = orderMatcher;
        }

        /// <summary>
        ///     Adds an order for the given contract and market side with the given properties
        /// </summary>
        /// <returns>The added order</returns>
        public IOrder AddOrder(long orderID,
                               Contract contract,
                               OrderType orderType,
                               MarketSide marketSide,
                               decimal price,
                               decimal quantity,
                               string clOrdID,
                               TradingAccount account)
        {
            var order = new Order(orderID,
                                  orderType,
                                  contract,
                                  marketSide,
                                  price,
                                  quantity,
                                  clOrdID,
                                  account);

            var stack = _market.GetOrCreate(
                contract,
                () =>
                    {
                        var os = OrderStackFactory.CreateStandardSortedStack(_orderMatcher);
                        os.OrdersMatched += OnOrdersMatched;
                        return os;
                    });
            stack.AddOrder(order);
            return order;
        }

        /// <summary>
        ///     Gets the order with the given ID
        /// </summary>
        /// <param name="orderID">The order ID</param>
        /// <returns>The order, will not be null</returns>
        /// <exception cref="FixATServerException">If the order cannot be found</exception>
        public IOrder GetOrder(long orderID)
        {
            // TODO If this is too slow then perhaps have a separate map of orderID to contract
            foreach (var orderStack in _market.Values)
            {
                var o = orderStack.GetOrderOrDefault(orderID);
                if (o != null)
                    return o;
            }
            throw new FixATServerException(
                string.Format("Order {0} could not be found", orderID));
        }

        /// <summary>
        ///     Deletes the order with the given ID
        /// </summary>
        /// <param name="orderID">The order ID</param>
        /// <returns>The deleted order, will be null of the order did not exist</returns>
        public IOrder DeleteOrder(long orderID)
        {
            foreach (var stack in _market.Values)
            {
                var o = stack.GetOrderOrDefault(orderID);
                if (o != null)
                    return stack.DeleteOrder(o) ? o : null;
            }
            return null;
        }

        /// <summary>
        ///     Gets the best price for the given contract and side of the market
        /// </summary>
        /// <returns>
        ///     null if there are no orders for that contract or side, otherwise the best price
        /// </returns>
        public decimal? GetBestPrice(Contract contract, MarketSide side)
        {
            OrderStack stack;
            if (!_market.TryGetValue(contract, out stack))
            {
                return null;
            }
            return stack.GetBestPrice(side);
        }

        public IEnumerable<IOrder> GetAllOrders()
        {
            return _market.Values.SelectMany(os => os.GetAllOrders()).ToList();
        }

        public void MatchOrders(Contract contract)
        {
            OrderStack stack;
            if (_market.TryGetValue(contract, out stack))
            {
                stack.MatchOrders();
            }
        }

        public event Action<OrdersMatchedEventArgs> OrdersMatched;

        private void InvokeOrdersMatched(OrdersMatchedEventArgs e)
        {
            var eventCopy = OrdersMatched;
            if (eventCopy != null)
                eventCopy(e);
        }

        private void OnOrdersMatched(OrdersMatchedEventArgs e)
        {
            InvokeOrdersMatched(e);
        }
    }
}