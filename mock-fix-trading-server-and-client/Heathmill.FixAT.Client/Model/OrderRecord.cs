using System;
using Heathmill.FixAT.Client.ViewModel;
using Heathmill.FixAT.Domain;

namespace Heathmill.FixAT.Client.Model
{
    public class OrderRecord : NotifyPropertyChangedBase, IComparable
    {
        public QuickFix.FIX44.NewOrderSingle OriginalNos { get; private set; }
        
        public OrderRecord(QuickFix.FIX44.NewOrderSingle nos)
        {
            OriginalNos = nos;

            decimal price = -1;
            if (nos.OrdType.Obj == QuickFix.Fields.OrdType.LIMIT && nos.IsSetPrice())
                price = nos.Price.Obj;

            ClOrdID = nos.ClOrdID.Obj;
            Symbol = nos.Symbol.Obj;
            Side = Services.TranslateFixFields.Translate(nos.Side);
            OrdType = Services.TranslateFixFields.Translate(nos.OrdType);
            Price = price;
            Quantity = nos.OrderQty.Obj;
            Status = OrderStatus.New;
        }

        public OrderRecord(QuickFix.FIX44.ExecutionReport msg)
        {
            // If creating from an ExecutionReport then it's from an incoming message
            // and therefore we shouldn't need the NOS details that don't exist anyway
            OriginalNos = null;

            ClOrdID = msg.ClOrdID.Obj;
            OrderID = msg.OrderID.Obj;
            Symbol = msg.Symbol.Obj;
            Side = Services.TranslateFixFields.Translate(msg.Side);
            //OrdType = FIXApplication.FixEnumTranslator.Translate(msg.OrdType);
            OrdType = OrderType.Limit; // Not specified in ExecutionReport
            //Price = msg.Price.Obj; // Not specified in ExecutionReport
            Price = msg.AvgPx.Obj; // TODO We may need to be smarter, updates should use LastPx but what about new orders?
            Quantity = msg.LeavesQty.Obj;
            Status = Services.TranslateFixFields.Translate(msg.OrdStatus);
            if (Status == OrderStatus.Rejected && msg.IsSetOrdRejReason())
            {
                RejectReason = msg.OrdRejReason.ToString();
            }
        }

        public OrderRecord()
        {
            
        }

        private string _clOrdID = "";
        public string ClOrdID
        {
            get { return _clOrdID; }
            set { _clOrdID = value; OnPropertyChanged("ClOrdID"); }
        }

        private string _symbol = "";
        public string Symbol
        {
            get { return _symbol; }
            set { _symbol = value; OnPropertyChanged("Symbol"); }
        }

        private MarketSide _side;
        public MarketSide Side
        {
            get { return _side; }
            set { _side = value; OnPropertyChanged("Side"); }
        }

        private OrderType _ordType = OrderType.Limit;
        public OrderType OrdType
        {
            get { return _ordType; }
            set { _ordType = value; OnPropertyChanged("OrdType"); }
        }

        private decimal _price = 0m;
        public decimal Price
        {
            get { return _price; }
            set { _price = value; OnPropertyChanged("Price"); }
        }

        private decimal _quantity = 0m;
        public decimal Quantity
        {
            get { return _quantity; }
            set { _quantity = value; OnPropertyChanged("Quantity"); }
        }

        private OrderStatus _status;
        public OrderStatus Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged("Status"); }
        }

        private string _orderID = "(unset)";
        public string OrderID
        {
            get { return _orderID; }
            set { _orderID = value; OnPropertyChanged("OrderID"); }
        }

        private DateTime _lastUpdateTime = DateTime.Now;
        public DateTime LastUpdateTime
        {
            get { return _lastUpdateTime; }
            set { _lastUpdateTime = value; OnPropertyChanged("LastUpdateTime"); }
        }

        private string _rejectReason;
        public string RejectReason
        {
            get { return _rejectReason; }
            set { _rejectReason = value; OnPropertyChanged("RejectReason"); }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return OrderID == ((OrderRecord) obj).OrderID;
        }

        public override int GetHashCode()
        {
            return OrderID.GetHashCode();
        }

        public int CompareTo(object obj)
        {
            var or = obj as OrderRecord;
            if (or == null)
            {
                throw new ArgumentException("Object is not an OrderRecord");
            }
            if (Side != or.Side)
            {
                throw new ArgumentException("Trying to sort orders from different market sides");
            }
            if (Symbol != or.Symbol)
            {
                throw new ArgumentException("Trying to sort orders with different symbols");
            }

            if (Price != or.Price)
            {
                return Side == MarketSide.Ask
                    ? Price.CompareTo(or.Price)
                    : or.Price.CompareTo(Price);
            }
            if (LastUpdateTime != or.LastUpdateTime)
            {
                return LastUpdateTime.CompareTo(or.LastUpdateTime);
            }
            return String.Compare(OrderID, or.OrderID, StringComparison.Ordinal);
        }
    }
}
