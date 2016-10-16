using System;
using Heathmill.FixAT.Client.Model;
using Heathmill.FixAT.Domain;

namespace Heathmill.FixAT.Client
{   
    public interface IServerFacade
    {
        void Start();
        void Stop();

        bool CreateOrder(OrderRecord orderDetails);

        bool CancelOrder(string symbol,
                         string clOrdID,
                         MarketSide side,
                         string orderID);

        bool UpdateOrder(OrderRecord oldOrderDetails, OrderRecord newOrderDetails);

        string GetServerSessionID();
        
        event Action<OrderStatus, OrderRecord> OrderExecutionEvent;

        event Action LogonEvent;
        event Action LogoutEvent;
    }
}
