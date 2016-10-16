using System;
using System.Linq;
using Heathmill.FixAT.Client.Model;
using Heathmill.FixAT.Domain;
using Heathmill.FixAT.Services;
using QuickFix.FIX44;

namespace Heathmill.FixAT.Client
{
    public class FixServerFacade : IServerFacade
    {
        private readonly ClientApplication _app;
        private readonly IExecIDGenerator _execIDGenerator;
        private readonly IFixMessageGenerator _fixMessageGenerator;

        public FixServerFacade(ClientApplication app,
                               IExecIDGenerator execIDGenerator,
                               IFixMessageGenerator fixMessageGenerator)
        {
            _app = app;
            _execIDGenerator = execIDGenerator;
            _fixMessageGenerator = fixMessageGenerator;
            _app.Fix44ExecReportEvent += HandleExecutionReport;
            _app.LogonEvent += HandleLogon;
            _app.LogoutEvent += HandleLogout;
        }

        public void Start()
        {
            _app.Start();
        }

        public void Stop()
        {
            _app.Stop();
        }

        public bool CreateOrder(OrderRecord o)
        {
            var msg =
               _fixMessageGenerator.CreateNewOrderSingleMessage(o.Symbol,
                                                                o.Side,
                                                                o.ClOrdID,
                                                                TradingAccount.None,
                                                                o.Price,
                                                                o.Quantity,
                                                                o.OrdType,
                                                                _execIDGenerator.CreateExecID());
            return _app.Send(msg);
        }

        public bool CancelOrder(string symbol,
                                  string clOrdID,
                                  MarketSide side,
                                  string orderID)
        {
            var msg =
                _fixMessageGenerator.CreateOrderCancelMessage(symbol,
                                                              clOrdID,
                                                              GenerateOrderCancelClOrdID(clOrdID),
                                                              side,
                                                              orderID);
            return _app.Send(msg);
        }

        public bool UpdateOrder(OrderRecord oldOrderDetails, OrderRecord newOrderDetails)
        {
            // TODO This should use an ordercancelreplace

            var fakeCancelClOrdID = oldOrderDetails.ClOrdID + "_Cancel";
            var cancel = _fixMessageGenerator.CreateOrderCancelMessage(oldOrderDetails.Symbol,
                                                                       oldOrderDetails.ClOrdID,
                                                                       fakeCancelClOrdID,
                                                                       oldOrderDetails.Side,
                                                                       oldOrderDetails.OrderID);
            if (!_app.Send(cancel))
                return false;

            var add = _fixMessageGenerator.CreateNewOrderSingleMessage(newOrderDetails.Symbol,
                                                                       newOrderDetails.Side,
                                                                       newOrderDetails.ClOrdID,
                                                                       TradingAccount.None,
                                                                       newOrderDetails.Price,
                                                                       newOrderDetails.Quantity,
                                                                       OrderType.Limit,
                                                                       _execIDGenerator.CreateExecID());
            return _app.Send(add);
        }

        public string GetServerSessionID()
        {
            var sidset = _app.MySessionSettings.GetSessions();
            return sidset.First().ToString();
        }

        public event Action<OrderStatus, OrderRecord> OrderExecutionEvent;
        public event Action LogonEvent;
        public event Action LogoutEvent;

        private void HandleExecutionReport(ExecutionReport msg)
        {
            if (msg == null) throw new ArgumentNullException("msg");

            var order = new OrderRecord(msg);
            var status = TranslateFixFields.Translate(msg.OrdStatus);

            var e = OrderExecutionEvent;
            if (e != null)
                e(status, order);
        }

        private void HandleLogon()
        {
            var e = LogonEvent;
            if (e != null)
                e();            
        }


        private void HandleLogout()
        {
            var e = LogoutEvent;
            if (e != null)
                e();
        }

        private static string GenerateOrderCancelClOrdID(string clOrdID)
        {
            return clOrdID + "_Cancel";
        }
    }
}
