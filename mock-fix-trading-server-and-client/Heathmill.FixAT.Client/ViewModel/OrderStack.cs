using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Heathmill.FixAT.Client.Model;
using Heathmill.FixAT.Domain;

namespace Heathmill.FixAT.Client.ViewModel
{
    public class OrderStack
    {
        private readonly List<OrderRecord> _bids = new List<OrderRecord>();
        private readonly object _bidsLock = new object();

        private readonly List<OrderRecord> _asks = new List<OrderRecord>();
        private readonly object _asksLock = new object();

        public void AddOrUpdateOrder(OrderRecord order)
        {
            AddOrUpdateOrder(order, UpdateLastUpdateTime.Yes);
        }

        public void OnPartialFillOrder(OrderRecord order)
        {
            AddOrUpdateOrder(order, UpdateLastUpdateTime.No);
        }

        public List<OrderRecord> GetBids()
        {
            lock (_bidsLock)
            {
                return new List<OrderRecord>(_bids);
            }
        }

        public List<OrderRecord> GetAsks()
        {
            lock (_asksLock)
            {
                return new List<OrderRecord>(_asks);
            }
        }

        private enum UpdateLastUpdateTime
        {
            No,
            Yes
        }

        private void AddOrUpdateOrder(
            OrderRecord order,
            UpdateLastUpdateTime updateLastUpdateTime)
        {
            if (order.Side == MarketSide.Bid)
            {
                lock (_bidsLock)
                {
                    AddOrUpdateImpl(_bids, order, updateLastUpdateTime);
                    _bids.Sort();
                }
            }
            else
            {
                lock (_asksLock)
                {
                    AddOrUpdateImpl(_asks, order, updateLastUpdateTime);
                    _asks.Sort();
                }
            }
        }

        public void RemoveOrder(OrderRecord order)
        {
            if (order.Side == MarketSide.Bid)
            {
                lock (_bidsLock)
                {
                    RemoveOrderImpl(_bids, order);
                    _bids.Sort();
                }
            }
            else
            {
                lock (_asksLock)
                {
                    RemoveOrderImpl(_asks, order);
                    _asks.Sort();
                }
            }
        }

        private static void AddOrUpdateImpl(
            List<OrderRecord> orders,
            OrderRecord order,
            UpdateLastUpdateTime updateLastUpdateTime)
        {
            var removed = RemoveOrderImpl(orders, order);

            Debug.Assert(!(updateLastUpdateTime == UpdateLastUpdateTime.No && removed == null));

            if (updateLastUpdateTime == UpdateLastUpdateTime.No && removed != null)
            {
                order.LastUpdateTime = removed.LastUpdateTime;
            }
            orders.Add(order);
        }

        private static OrderRecord RemoveOrderImpl(List<OrderRecord> orders, OrderRecord order)
        {
            var existing = orders.FirstOrDefault(o => o.ClOrdID == order.ClOrdID);
            if (existing != null)
            {
                orders.Remove(existing);
            }
            return existing;
        }
    }
}