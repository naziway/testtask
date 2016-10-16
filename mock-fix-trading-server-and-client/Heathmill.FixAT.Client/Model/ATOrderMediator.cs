using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Heathmill.FixAT.Domain;

namespace Heathmill.FixAT.Client.Model
{
    public class ATOrderMediator
    {
        private readonly IATOrderRepository _atOrderRepository;
        private readonly IServerFacade _serverFacade;
        private readonly IClOrdIDGenerator _clOrdIDGenerator;

        public ATOrderMediator(IATOrderRepository atOrderRepository,
                               IServerFacade serverFacade,
                               IClOrdIDGenerator clOrdIDGenerator)
        {
            _atOrderRepository = atOrderRepository;
            _serverFacade = serverFacade;
            _clOrdIDGenerator = clOrdIDGenerator;

            _serverFacade.OrderExecutionEvent += OnOrderExecutionEvent;
        }

        public event Action<IcebergOrder> IcebergOrderUpdated;
        public event Action<IcebergOrder> IcebergOrderAdded;

        protected virtual void OnIcebergOrderAdded(IcebergOrder obj)
        {
            Action<IcebergOrder> handler = IcebergOrderAdded;
            if (handler != null) handler(obj);
        }

        protected virtual void OnOrderUpdated(IcebergOrder order)
        {
            var handler = IcebergOrderUpdated;
            if (handler != null) handler(order);
        }

        public void AddIcebergOrder(string symbol,
                                    string clOrdID,
                                    MarketSide side,
                                    decimal totalQuantity,
                                    decimal clipSize,
                                    decimal initialPrice,
                                    decimal priceDelta)
        {
            var io = new IcebergOrder(_serverFacade,
                                      symbol,
                                      clOrdID,
                                      side,
                                      totalQuantity,
                                      clipSize,
                                      initialPrice,
                                      priceDelta);

            _atOrderRepository.IcebergOrders.Add(io);

            OnIcebergOrderAdded(io);
        }

        public string GenerateClOrdID()
        {
            return _clOrdIDGenerator.CreateATClOrdID();
        }

        private void OnOrderExecutionEvent(OrderStatus status, OrderRecord order)
        {
            switch (status)
            {
                case OrderStatus.New:
                    OnNewOrder(order);
                    break;
                case OrderStatus.Filled:
                    OnOrderTotalFill(order);
                    break;
                case OrderStatus.PartiallyFilled:
                    OnOrderPartialFill(order);
                    break;
                case OrderStatus.Canceled:
                    OnOrderCanceled(order);
                    break;
                
                // TODO Handle OrderStatus.Suspended
            }
        }

        private void OnOrderCanceled(OrderRecord order)
        {
            var iceberg =
             _atOrderRepository.IcebergOrders.FirstOrDefault(io => io.ClOrdID == order.ClOrdID);
            if (iceberg != null)
            {
                iceberg.MarketOrderCanceled();
                OnOrderUpdated(iceberg);
            }
        }

        private void OnNewOrder(OrderRecord order)
        {
            var iceberg =
                _atOrderRepository.IcebergOrders.FirstOrDefault(io => io.ClOrdID == order.ClOrdID);
            if (iceberg != null)
            {
                iceberg.ActivatedMarketOrderAccepted(order.OrderID);
                OnOrderUpdated(iceberg);
            }
        }

        private void OnOrderPartialFill(OrderRecord order)
        {
            var iceberg =
                _atOrderRepository.IcebergOrders.FirstOrDefault(io => io.ClOrdID == order.ClOrdID);
            if (iceberg != null)
            {
                // This is predicated on the ClOrdID of an order not changing
                var filledQty = iceberg.CurrentQuantity - order.Quantity;
                iceberg.OnPartialFill(filledQty);
                OnOrderUpdated(iceberg);
            }
        }

        private void OnOrderTotalFill(OrderRecord order)
        {
            var iceberg =
                _atOrderRepository.IcebergOrders.FirstOrDefault(io => io.ClOrdID == order.ClOrdID);
            if (iceberg != null)
            {
                iceberg.OnTotalFill();
                OnOrderUpdated(iceberg);
            } 
        }
    }
}
