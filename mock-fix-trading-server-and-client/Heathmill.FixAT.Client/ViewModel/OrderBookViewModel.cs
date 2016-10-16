using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Media;
using Heathmill.FixAT.Client.Model;
using Heathmill.FixAT.Domain;
using Heathmill.FixAT.Utilities;

namespace Heathmill.FixAT.Client.ViewModel
{
    public class OrderBookViewModel : NotifyPropertyChangedBase
    {
        public sealed class OrderStackRow
        {
            public string Symbol { get; set; }
            public string BidClOrdID { get; set; }
            public string BidStatus { get; set; }
            public string BidQty { get; set; }
            public string BidPrice { get; set; }
            public string AskClOrdID { get; set; }
            public string AskStatus { get; set; }
            public string AskQty { get; set; }
            public string AskPrice { get; set; }
            public Color RowColor { get; set; }
        }

        public ObservableCollection<OrderStackRow> OrderStack { get; set; }
            
        private readonly object _stackLock = new object();

        private readonly SortedDictionary<string, OrderStack> _market =
            new SortedDictionary<string, OrderStack>();
        private readonly object _marketLock = new object();

        private readonly IClOrdIDGenerator _clOrdIDGenerator;
        private readonly IMessageSink _messageSink;
        private readonly IServerFacade _serverFacade;

        public OrderBookViewModel(IServerFacade serverFacade,
                                  IClOrdIDGenerator clOrdIDGenerator,
            IMessageSink messageSink)
        {
            _clOrdIDGenerator = clOrdIDGenerator;
            _messageSink = messageSink;
            _serverFacade = serverFacade;
            OrderStack = new ObservableCollection<OrderStackRow>();

            _serverFacade.OrderExecutionEvent += HandleOrderExecutionEvent;
            _serverFacade.LogoutEvent += OnLogout;
        }

        public void SubmitNewOrder(OrderRecord o)
        {
            _serverFacade.CreateOrder(o);
        }

        public string CreateClOrdID()
        {
            return _clOrdIDGenerator.CreateClOrdID();
        }

        public void BuyOrder(OrderStackRow osr)
        {
            Trade(osr, MarketSide.Bid);
        }

        public void SellOrder(OrderStackRow osr)
        {
            Trade(osr, MarketSide.Ask);
        }

        private void Trade(OrderStackRow osr, MarketSide side)
        {
            if (osr == null) throw new ArgumentNullException("osr");

            // The server currently does not support click trading, so we fake
            // it here by adding a matching order to the other side of the market.
            // If the server supported ImmediateOrCancel then we'd use that.
            var isBid = side == MarketSide.Bid;
            var matchingOrderDetails = new OrderRecord
            {
                
                ClOrdID = _clOrdIDGenerator.CreateClOrdID(),
                LastUpdateTime = DateTime.UtcNow,
                OrderID = string.Empty, // Set by server
                OrdType = OrderType.Limit,
                Price = decimal.Parse(isBid ? osr.BidPrice : osr.AskPrice),
                Quantity = decimal.Parse(isBid ? osr.BidQty : osr.AskQty),
                Side = isBid ? MarketSide.Ask : MarketSide.Bid,
                Symbol = osr.Symbol,
                Status = OrderStatus.New
            };

            _serverFacade.CreateOrder(matchingOrderDetails);
        }

        private void OnLogout()
        {
            // Clear all the orders, we'll get a new list when we reconnect
            lock (_marketLock)
            {
                _market.Clear();
            }
            lock (_stackLock)
            {
                SmartDispatcher.Invoke(() => OrderStack.Clear());
            }
        }

        private void HandleOrderExecutionEvent(OrderStatus status, OrderRecord order)
        {
            if (order == null) throw new ArgumentNullException("order");
            
            // TODO We need to handle suspended -> active
            switch (status)
            {
                case OrderStatus.Canceled:
                case OrderStatus.Filled:
                case OrderStatus.Traded:
                    RemoveOrder(order);
                    break;
                case OrderStatus.PartiallyFilled:
                    OnPartialFill(order);
                    break;
                case OrderStatus.Suspended:
                    UpdateOrder(order);
                    break;
                case OrderStatus.New:
                    AddOrUpdateOrder(order);
                    break;
                case OrderStatus.Rejected:
                    OrderRejected(order);
                    break;
            }
        }

        private void OrderRejected(OrderRecord order)
        {
            _messageSink.Error(() =>
            {
                var reason = order.RejectReason;
                if (string.IsNullOrWhiteSpace(reason))
                {
                    // TODO There is a lack of ability to send free-text error messages via FIX
                    // Therefore if the reason is not filled in here we guess at the likely cause
                    reason = "Unkown. Possibly order would have led to a crossed market";
                }
                return "Unable to add order: " + reason;
            });
        }

        private void AddOrUpdateOrder(OrderRecord order)
        {
            GetOrCreateStack(order).AddOrUpdateOrder(order);
            UpdateOrderStackRows();
        }

        private void OnPartialFill(OrderRecord order)
        {
            GetOrCreateStack(order).OnPartialFillOrder(order);
            UpdateOrderStackRows();
        }

        private void UpdateOrder(OrderRecord order)
        {
            GetOrCreateStack(order).AddOrUpdateOrder(order);
            UpdateOrderStackRows();
        }

        private void RemoveOrder(OrderRecord order)
        {
            GetOrCreateStack(order).RemoveOrder(order);
            UpdateOrderStackRows();
        }

        private OrderStack GetOrCreateStack(OrderRecord order)
        {
            var symbol = order.Symbol;
            lock (_marketLock)
            {
                return _market.GetOrCreate(symbol);
            }
        }

        private void UpdateOrderStackRows()
        {
            // TODO Diff the old and new stacks
            // Phase 1 just swaps out the old for the new, which is bad for many reasons
            var displayStack = new List<OrderStackRow>();
            lock (_marketLock)
            {
                var backgroundColours = new[] {Colors.White, Colors.LightGray};
                bool useColour1 = true;
                foreach (var orderStack in _market)
                {
                    var rowColour = useColour1 ? backgroundColours[0] : backgroundColours[1];
                    var symbol = orderStack.Key;
                    var stack = orderStack.Value;
                    var rows = CreateOrderStackRows(symbol, stack, rowColour);
                    rows.ForEach(displayStack.Add);

                    useColour1 = !useColour1;
                }
            }
            
            SmartDispatcher.Invoke(UpdateOrderStack, displayStack);
        }

        private void UpdateOrderStack(IEnumerable<OrderStackRow> newStack)
        {
            // TODO A better swap method
            // Should use .Move for moved rows and Add/Remove for new/deleted
            lock (_stackLock)
            {
                OrderStack.Clear();
                foreach (var orderStackRow in newStack)
                {
                    OrderStack.Add(orderStackRow);
                }
            }
        }

        private static List<OrderStackRow> CreateOrderStackRows(
            string symbol,
            OrderStack stack,
            Color rowColour)
        {
            var bids = stack.GetBids();
            var asks = stack.GetAsks();
            var maxSize = Math.Max(bids.Count, asks.Count);
            var rows = new List<OrderStackRow>();
            for (var i = 0; i < maxSize; ++i)
            {
                var bid = bids.ElementAtOrDefault(i);
                var ask = asks.ElementAtOrDefault(i);
                Debug.Assert(bid != null || ask != null);
                var osr = new OrderStackRow
                {
                    Symbol = symbol,
                    RowColor = rowColour
                };
                if (bid != null)
                {
                    osr.BidClOrdID = bid.ClOrdID;
                    osr.BidStatus = bid.Status.ToString();
                    osr.BidPrice = bid.Price.ToString(CultureInfo.CurrentUICulture);
                    osr.BidQty = bid.Quantity.ToString(CultureInfo.CurrentUICulture);
                }
                if (ask != null)
                {
                    osr.AskClOrdID = ask.ClOrdID;
                    osr.AskStatus = ask.Status.ToString();
                    osr.AskPrice = ask.Price.ToString(CultureInfo.CurrentUICulture);
                    osr.AskQty = ask.Quantity.ToString(CultureInfo.CurrentUICulture);
                }
                rows.Add(osr);
            }
            return rows;
        }
    }
}
