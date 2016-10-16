using System;

namespace Heathmill.FixAT.Domain
{
    // TODO Flesh out, encapsulate any order calculations and matching equality

    /// <summary>
    /// An order in the system
    /// </summary>
    public interface IOrder : IEquatable<IOrder>
    {
        /// <summary>
        /// The internal ID of the order in this system
        /// </summary>
        long ID { get; }

        /// <summary>
        /// The Type of the order e.g. Limit, Market
        /// </summary>
        OrderType OrderType { get; }

        /// <summary>
        /// The contract the order is for
        /// </summary>
        Contract Contract { get; }

        /// <summary>
        /// Whether the order is a bid or an ask
        /// </summary>
        MarketSide MarketSide { get; }

        /// <summary>
        /// The price for this order
        /// </summary>
        decimal Price { get; }

        /// <summary>
        /// The quantity or volume this order has
        /// </summary>
        decimal Quantity { get; }

        /// <summary>
        /// The quantity or volume this order was created with
        /// </summary>
        decimal OriginalQuantity { get; }

        /// <summary>
        /// The quantity or volume that has been filled so far for this order
        /// </summary>
        decimal FilledQuantity { get; }

        /// <summary>
        /// When the order was last updated, either creation or a property changed
        /// </summary>
        DateTime LastUpdateTime { get; }

        /// <summary>
        /// The Client Order ID, assigned to the order by the client application
        /// </summary>
        string ClOrdID { get; }

        /// <summary>
        /// The trading account associated with the order
        /// </summary>
        TradingAccount Account { get; }

        /// <summary>
        /// The order was partially filled/matched and should adjust itself accordingly
        /// </summary>
        /// <param name="filledQuantity"></param>
        void OrderPartiallyFilled(decimal filledQuantity);

        /// <summary>
        /// Update the price of this order, this will set LastUpdateTime to now
        /// </summary>
        /// <param name="newPrice"></param>
        void UpdatePrice(decimal newPrice);

        /// <summary>
        /// Update the quantity of this order, this will set LastUpdateTime to now
        /// </summary>
        /// <param name="newQuantity"></param>
        void UpdateQuantity(decimal newQuantity);
    }
}
