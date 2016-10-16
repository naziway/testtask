using System;
using Heathmill.FixAT.Domain;

namespace Heathmill.FixAT.Client.Model
{
    public class IcebergOrder
    {
        private readonly IServerFacade _serverFacade;

        public string Symbol { get; private set; }
        public string ClOrdID { get; private set; }
        public string OrderID { get; private set; }
        public MarketSide Side { get; private set; }
        public decimal TotalQuantity { get; private set; }
        public decimal ClipSize { get; private set; }
        public decimal InitialPrice { get; private set; }
        public decimal PriceDelta { get; private set; }
        public ActivationState State { get; private set; }
        public decimal RemainingQuantity { get; private set; }
        public decimal CurrentQuantity { get; private set; }
        public decimal CurrentPrice { get; private set; }

        public enum ActivationState
        {
            Suspended,
            PendingSuspension,
            PendingActivationAcceptance,
            Active
        }

        /// <summary>
        /// The time the market order from this iceberg order was last traded.
        /// Will be null if the order has not yet been traded.
        /// </summary>
        public DateTime? LastTradedTime { get; private set; }

        public IcebergOrder(IServerFacade serverFacade,
                            string symbol,
                            string clOrdID,
                            MarketSide side,
                            decimal totalQuantity,
                            decimal clipSize,
                            decimal initialPrice,
                            decimal priceDelta)
        {
            if (totalQuantity <= 0)
                throw new ApplicationException(
                    "Iceberg Order must have a quantity greater than zero");

            if (clipSize > totalQuantity)
                throw new ApplicationException(
                    "Iceberg Order total quantity must be greater or equal to the clip size");

            Symbol = symbol;
            ClOrdID = clOrdID;
            Side = side;
            TotalQuantity = totalQuantity;
            ClipSize = clipSize;
            InitialPrice = initialPrice;
            PriceDelta = priceDelta;
            State = ActivationState.Suspended;

            _serverFacade = serverFacade;
            RemainingQuantity = totalQuantity;
            CurrentPrice = InitialPrice;
            CurrentQuantity = clipSize;
        }

        public void Activate()
        {
            if (State == ActivationState.Active)
                throw new ApplicationException("Iceberg Order is already active");

            if (State == ActivationState.PendingActivationAcceptance)
                throw new ApplicationException("Iceberg Order has already been activated");

            
            var order = ToOrderRecord();
            
            if (_serverFacade.CreateOrder(order))
                State = ActivationState.PendingActivationAcceptance;
        }

        /// <summary>
        /// Should be called once the server accepts the market order created
        /// by this Iceberg Order
        /// </summary>
        /// <param name="orderID">The OrderID assigned to the market order by the server</param>
        public void ActivatedMarketOrderAccepted(string orderID)
        {
            State = ActivationState.Active;
            OrderID = orderID;
        }

        public void Suspend()
        {
            if (State == ActivationState.Suspended || State == ActivationState.PendingSuspension)
                throw new ApplicationException("Iceberg Order is already suspended");

            if (State == ActivationState.PendingActivationAcceptance ||
                string.IsNullOrEmpty(OrderID))
            {
                throw new ApplicationException(
                    "Market order has not been accepted yet, cannot suspend the order yet");
            }

            if (_serverFacade.CancelOrder(Symbol, ClOrdID, Side, OrderID))
                State = ActivationState.PendingSuspension;
        }

        /// <summary>
        /// Should be called when the Execution Report containing the cancel for the market
        /// order is received from the server
        /// </summary>
        public void MarketOrderCanceled()
        {
            State = ActivationState.Suspended;
        }

        /// <summary>
        /// The market order has been partially filled
        /// </summary>
        public void OnPartialFill(decimal quantityFilled)
        {
            if (State != ActivationState.Active)
                throw new ApplicationException("Order must be active before OnPartialFill is called");

            if (quantityFilled >= CurrentQuantity)
                throw new ApplicationException(
                    "Partial fill must be for less than the current order quantity");

            LastTradedTime = DateTime.UtcNow;
            CurrentQuantity -= quantityFilled;
            RemainingQuantity -= quantityFilled;
        }

        /// <summary>
        /// The active market order has been totalled filled (i.e. all quantity traded)
        /// </summary>
        public void OnTotalFill()
        {
            if (State != ActivationState.Active)
                throw new ApplicationException("Order must be active before OnTotalFill is called");

            State = ActivationState.Suspended;

            LastTradedTime = DateTime.UtcNow;

            // Change the price by the delta
            AdjustPriceForRefill();

            // A total fill means the trade has taken all the currentQuantity
            RemainingQuantity -= CurrentQuantity; 

            // Now replenish the current available quantity based on the clip size
            CurrentQuantity = Math.Min(ClipSize, RemainingQuantity);

            if (CurrentQuantity > 0m)
                Activate();
        }

        /// <summary>
        /// Currently you can only update the values of an iceberg order if it's either
        /// inactive or its market order has been accepted by the server
        /// </summary>
        public bool CanSetNewValues()
        {
            return
                State != ActivationState.PendingActivationAcceptance &&
                State != ActivationState.PendingSuspension;
        }

        /// <summary>
        /// Call this if you wish to manually change the price level for the Iceberg Order
        /// </summary>
        /// <param name="newPrice">The new price</param>
        /// <remarks>Call CanSerNewValues() first</remarks>
        public void SetNewOrderPrice(decimal newPrice)
        {
            if (!CanSetNewValues())
                throw new ApplicationException("Unable to set new order price");

            // Should we update initial price here?
            // If the purpose is to show how the delta has changed the price then yes,
            // if it's to show creation data about the order then no

            var oldOrderDetails = ToOrderRecord();

            CurrentPrice = newPrice;
            if (State == ActivationState.Active)
            {
                _serverFacade.UpdateOrder(oldOrderDetails, ToOrderRecord());
            }
        }

        private void AdjustPriceForRefill()
        {
            if (Side == MarketSide.Bid)
            {
                CurrentPrice -= PriceDelta;
            }
            else
            {
                CurrentPrice += PriceDelta;
            }

            // Lowest price is zero
            CurrentPrice = Math.Max(CurrentPrice, 0);
        }

        private OrderRecord ToOrderRecord()
        {
            return new OrderRecord
            {
                ClOrdID = ClOrdID,
                LastUpdateTime = DateTime.UtcNow,
                OrderID = OrderID,
                OrdType = OrderType.Limit,
                Price = CurrentPrice,
                Quantity = CurrentQuantity,
                Side = Side,
                Status = OrderStatus.New,
                Symbol = Symbol
            };
        }

    }
}
