using System;
using System.Collections.Generic;
using Heathmill.FixAT.Domain;

namespace Heathmill.FixAT.Server
{
    /// <summary>
    ///     A respository for the orders in the system
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>
        ///     Adds an order to the repository
        /// </summary>
        IOrder AddOrder(long orderID,
                        Contract contract,
                        OrderType orderType,
                        MarketSide marketSide,
                        decimal price,
                        decimal quantity,
                        string clOrdID,
                        TradingAccount account);

        /// <summary>
        ///     Gets an order from the repository
        /// </summary>
        /// <param name="orderID">The ID of the order</param>
        /// <returns>The order, will not be null</returns>
        /// <exception cref="FixATServerException">If the order cannot be found</exception>
        IOrder GetOrder(long orderID);

        /// <summary>
        ///     Deletes an order from the repository
        /// </summary>
        /// <param name="orderID">The ID of the order to delete</param>
        /// <returns>The deleted order, will be null of the order did not exist</returns>
        IOrder DeleteOrder(long orderID);

        /// <summary>
        ///     Gets the best price for a given contract and side of the market
        /// </summary>
        /// <param name="contract">The contract</param>
        /// <param name="side">The side of the market</param>
        /// <returns>
        ///     Null if there are no orders for that side and contract, otherwise the best price
        /// </returns>
        decimal? GetBestPrice(Contract contract, MarketSide side);

        /// <summary>
        /// Returns all orders in the repository
        /// </summary>
        IEnumerable<IOrder> GetAllOrders();

        /// <summary>
        /// Carries out order matching for the given contract
        /// </summary>
        void MatchOrders(Contract contract);

        /// <summary>
        ///     An event triggered when orders are automatched
        /// </summary>
        event Action<OrdersMatchedEventArgs> OrdersMatched;
    }
}