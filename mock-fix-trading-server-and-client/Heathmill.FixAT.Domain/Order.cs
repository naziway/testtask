using System;

namespace Heathmill.FixAT.Domain
{
    public class Order : IOrder
    {
        public Order(long id,
                     OrderType orderType,
                     Contract contract,
                     MarketSide marketSide,
                     decimal price,
                     decimal quantity,
                     string clOrdID,
                     TradingAccount tradingAccount)
        {
            ID = id;
            OrderType = orderType;
            Contract = contract;
            MarketSide = marketSide;
            Price = price;
            Quantity = quantity;
            LastUpdateTime = DateTime.UtcNow;
            ClOrdID = clOrdID;
            Account = tradingAccount;
            OriginalQuantity = quantity;
        }

        public long ID { get; private set; }
        public OrderType OrderType { get; private set; }
        public Contract Contract { get; private set; }
        public MarketSide MarketSide { get; private set; }
        public decimal Price { get; private set; }
        public decimal Quantity { get; private set; }
        public decimal OriginalQuantity { get; private set; }
        public decimal FilledQuantity { get { return OriginalQuantity - Quantity; } }
        public DateTime LastUpdateTime { get; private set; }
        public string ClOrdID { get; private set; }
        public TradingAccount Account { get; private set; }
        
        public void OrderPartiallyFilled(decimal filledQuantity)
        {
            Quantity -= filledQuantity;
            // Do NOT adjust the last udpate time for a partial match
        }

        public void UpdatePrice(decimal newPrice)
        {
            Price = newPrice;
            LastUpdateTime = DateTime.UtcNow;
        }

        public void UpdateQuantity(decimal newQuantity)
        {
            Quantity = newQuantity;
            LastUpdateTime = DateTime.UtcNow;
        }

        // TODO Should this go by ID or ClOrdID
        public bool Equals(IOrder other)
        {
            if (other == null) throw new ArgumentNullException("other");
            return ID == other.ID;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is IOrder))
                return false;

            return Equals((IOrder) obj);
        }

        public bool IsSameClientOrder(IOrder o)
        {
            return ClOrdID == o.ClOrdID;
        }

        public override string ToString()
        {
            // TODO Consider a better ToString for display?
            return this.GetPropertiesString();
        }
    }
}
