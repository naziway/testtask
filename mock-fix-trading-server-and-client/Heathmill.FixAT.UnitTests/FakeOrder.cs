using System;
using System.Globalization;
using Heathmill.FixAT.Domain;

namespace Heathmill.FixAT.UnitTests
{
    internal class FakeOrder : IOrder
    {
        public bool Equals(IOrder other)
        {
            if (other == null) throw new ArgumentNullException("other");
            return ID == other.ID;
        }

        public long ID { get; set; }
        public OrderType OrderType { get; set; }
        public Contract Contract { get; set; }
        public MarketSide MarketSide { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string ClOrdID { get; set; }
        public TradingAccount Account { get; set; }
        public decimal OriginalQuantity { get; set; }
        public decimal FilledQuantity { get { return OriginalQuantity - Quantity; } }
        
        public void OrderPartiallyFilled(decimal filledQuantity)
        {
            Quantity -= filledQuantity;
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

        // OrderType;symbol;MarketSide;quantity@price
        // e.g. "Limit;EURUSD;Bid;20@10"
        public static FakeOrder CreateOrderFromString(long id, string orderString)
        {
            var sections = orderString.Split(new[] { ';', '@' });
            if (sections.Length != 5)
                throw new ApplicationException("Invalid orderString " + orderString);
            var orderType = (OrderType)Enum.Parse(typeof(OrderType), sections[0]);
            var contract = new Contract(sections[1]);
            var side = (MarketSide)Enum.Parse(typeof(MarketSide), sections[2]);
            var quantity = decimal.Parse(sections[3]);
            var price = decimal.Parse(sections[4]);

            return new FakeOrder
                {
                    Contract = contract,
                    ID = id,
                    LastUpdateTime = DateTime.UtcNow,
                    MarketSide = side,
                    OrderType = orderType,
                    Price = price,
                    Quantity = quantity,
                    OriginalQuantity = quantity,
                    ClOrdID = "ClOrdID" + id.ToString(CultureInfo.InvariantCulture),
                    Account = TradingAccount.None
                };
        }
    }
}
