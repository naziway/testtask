using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Heathmill.FixAT.Domain
{
    /// <summary>
    /// A sorted order list representing a price/order stack for each side of the market
    /// </summary>
    /// <remarks>
    /// In theory nothing would stop you storing multiple contracts in a single stack,
    /// however the default order matcher would then automatch across contracts.
    /// If you don't want that then provide a different implementation of IOrderMatcher.
    /// </remarks>
    public class OrderStack
    {
        public class StandardOrderComparer : Comparer<IOrder>
        {
            public override int Compare(IOrder x, IOrder y)
            {
                return OrderSorter.StandardOrderSorter(x, y);
            }
        }


        // TODO Should we be averaging out matches across price levels?
        // TODO How do we sensibly handle matching crossed markets?
        // Should be "initiator" price, so price of older order?
        // Currently orders that would cross the market are rejected

        /// <summary>
        /// Create an order stack with a provided way of sorting the orders
        /// </summary>
        /// <param name="orderMatcher">The class to carry out order matching</param>
        /// <param name="comp">
        /// A way of comparing the orders where less-than means a better order.
        /// The comparer should ensure that it sorts bids and asks appropriately.
        /// </param>
        public OrderStack(IOrderMatcher orderMatcher,
                          IComparer<IOrder> comp)
        {
            _bids = new SortedSet<IOrder>(comp);
            _asks = new SortedSet<IOrder>(comp);
            _orderMatcher = orderMatcher;
        }

        /// <summary>
        /// Event to signal that a given quantity of an order has been matched.
        /// </summary>
        public event Action<OrdersMatchedEventArgs> OrdersMatched;

        /// <summary>
        /// Add an order to the stack.
        /// </summary>
        public void AddOrder(IOrder order)
        {
            var locker = GetSidedLock(order);
            locker.EnterWriteLock();
            try
            {
                AddOrderImpl(order);
            }
            finally
            {
                locker.ExitWriteLock();
            }            
        }

        /// <summary>
        /// Gets the best price for the given side of the market
        /// </summary>
        /// <returns>
        /// null if there are no orders for that side, otherwise the best price
        /// </returns>
        public decimal? GetBestPrice(MarketSide side)
        {
            var locker = GetSidedLock(side);
            locker.EnterReadLock();
            try
            {
                var stack = GetSidedStack(side);
                return stack.Count == 0 ? (decimal?)null : stack.ElementAt(0).Price;
            }
            finally
            {
                locker.ExitReadLock();
            }            
        }

        private void AddOrderImpl(IOrder order)
        {
            var stack = GetSidedStack(order);
            if (!stack.Add(order))
                throw new DomainException("Unable to add order " + order.GetPropertiesString());
        }

        /// <summary>
        /// Deletes the order with the given ID
        /// </summary>
        /// <param name="orderID">The ID of the order</param>
        /// <returns>The deleted order, will be null of the order did not exist</returns>
        public IOrder DeleteOrder(long orderID)
        {
            var order = GetOrderOrDefault(orderID);
            
            if (order == null || !DeleteOrder(order))
                return null;

            return order;
        }

        /// <summary>
        /// Deletes the given order
        /// </summary>
        /// <param name="order">The order</param>
        /// <returns>
        /// Whether the order was deleted i.e. false if the order was not found or the delete fails
        /// </returns>
        public bool DeleteOrder(IOrder order)
        {
            var locker = GetSidedLock(order);
            locker.EnterWriteLock();
            try
            {
                return DeleteOrderImpl(order);
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        private bool DeleteOrderImpl(IOrder order)
        {
            return GetSidedStack(order).Remove(order);
        }

        /// <summary>
        /// Gets the order with the given ID, or null if not found
        /// </summary>
        /// <param name="orderID">The order ID</param>
        /// <returns>The order, or null if the order was not found</returns>
        public IOrder GetOrderOrDefault(long orderID)
        {
            _bidsLock.EnterReadLock();
            _asksLock.EnterReadLock();
            try
            {
                return GetOrderOrDefaultImpl(orderID);
            }
            finally
            {
                _bidsLock.ExitReadLock();
                _asksLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets all orders in the stack, bids and asks
        /// </summary>
        public List<IOrder> GetAllOrders()
        {
            _bidsLock.EnterReadLock();
            _asksLock.EnterReadLock();
            var allOrders = new List<IOrder>();
            try
            {
                allOrders.AddRange(_bids);
                allOrders.AddRange(_asks);
                return allOrders;
            }
            finally
            {
                _bidsLock.ExitReadLock();
                _asksLock.ExitReadLock();
            }
        }

        private IOrder GetOrderOrDefaultImpl(long orderID)
        {
            // TODO Should be able to leverage sorted nature in search
            return _bids.FirstOrDefault(o => o.ID == orderID) ??
                   _asks.FirstOrDefault(o => o.ID == orderID);
        }

        private SortedSet<IOrder> GetSidedStack(IOrder o)
        {
            return GetSidedStack(o.MarketSide);
        }

        private ReaderWriterLockSlim GetSidedLock(IOrder o)
        {
            return GetSidedLock(o.MarketSide);
        }

        private SortedSet<IOrder> GetSidedStack(MarketSide side)
        {
            return (side == MarketSide.Bid) ? _bids : _asks;
        }

        private ReaderWriterLockSlim GetSidedLock(MarketSide side)
        {
            return (side == MarketSide.Bid) ? _bidsLock : _asksLock;
        }

        public void MatchOrders()
        {
            // Lock both sides of the market and look for matches
            _bidsLock.EnterWriteLock();
            _asksLock.EnterWriteLock();
            try
            {
                var matches = _orderMatcher.Match(_bids, _asks).ToList();
                InvokeOrdersMatched(matches);
                UpdateMatchedOrders(matches);
            }
            finally
            {
                _bidsLock.ExitWriteLock();
                _asksLock.ExitWriteLock();
            }
        }

        // Delete fully matched orders, update quantity for partial matches
        private void UpdateMatchedOrders(IEnumerable<OrderMatch> matches)
        {
            foreach (var match in matches)
            {
                var o = GetOrderOrDefaultImpl(match.OrderID);
                if (o == null) continue;

                if (match.MatchType == MatchType.Full)
                {
                    DeleteOrderImpl(o);
                }
                else
                {
                    o.OrderPartiallyFilled(match.MatchedQuantity);
                }
            }
        }


        private void InvokeOrdersMatched(List<OrderMatch> matches)
        {
            var eventCopy = OrdersMatched;
            if (eventCopy != null && matches.Any())
                eventCopy(new OrdersMatchedEventArgs(matches));
        }

        private readonly SortedSet<IOrder> _bids;
        private readonly SortedSet<IOrder> _asks;

        // Since the write lock may already be held by AddOrder when a Match is kicked off
        // the sided locks are made recursive.

        private readonly ReaderWriterLockSlim _bidsLock =
            new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private readonly ReaderWriterLockSlim _asksLock =
            new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private readonly IOrderMatcher _orderMatcher;
    }
}
