using System.Globalization;
using Heathmill.FixAT.Domain;
using QuickFix.FIX42;
using QuickFix.Fields;
using MatchType = Heathmill.FixAT.Domain.MatchType;

namespace Heathmill.FixAT.Services
{
    public static class CreateFix42Message
    {
        public static ExecutionReport CreateRejectNewOrderExecutionReport(
            NewOrderSingle n,
            string execID,
            string rejectionReason,
            int? rejectionCode = null)
        {
            var exReport = new ExecutionReport(
                new OrderID("unknown orderID"),
                new ExecID(execID),
                new ExecTransType(ExecTransType.NEW),
                new ExecType(ExecType.REJECTED),
                new OrdStatus(OrdStatus.REJECTED),
                n.Symbol,
                n.Side,
                new LeavesQty(0m),
                new CumQty(0m),
                new AvgPx(0m));

            if (rejectionCode.HasValue)
            {
                exReport.OrdRejReason = new OrdRejReason(rejectionCode.Value);
            }

            exReport.Set(n.ClOrdID);
            exReport.Set(n.OrderQty);

            if (n.IsSetAccount())
                exReport.SetField(n.Account);

            return exReport;
        }

        public static OrderCancelReject CreateOrderCancelReject(OrderCancelRequest msg,
                                                                int rejectReason,
                                                                string rejectReasonText)
        {
            var orderid = (msg.IsSetOrderID()) ? msg.OrderID.Obj : "unknown orderID";
            var ocj = new OrderCancelReject(
                new OrderID(orderid),
                msg.ClOrdID,
                msg.OrigClOrdID,
                new OrdStatus(OrdStatus.REJECTED),
                new CxlRejResponseTo(CxlRejResponseTo.ORDER_CANCEL_REQUEST))
                {
                    CxlRejReason = new CxlRejReason(rejectReason),
                    Text = new Text(rejectReasonText)
                };

            return ocj;
        }

        public static OrderCancelReject CreateOrderCancelReplaceReject(OrderCancelReplaceRequest msg,
                                                                        int rejectReason,
                                                                        string rejectReasonText)
        {
            var orderid = (msg.IsSetOrderID()) ? msg.OrderID.Obj : "unknown orderID";
            var ocj = new OrderCancelReject(
                new OrderID(orderid),
                msg.ClOrdID,
                msg.OrigClOrdID,
                new OrdStatus(OrdStatus.REJECTED),
                new CxlRejResponseTo(CxlRejResponseTo.ORDER_CANCEL_REPLACE_REQUEST))
                {
                    CxlRejReason = new CxlRejReason(rejectReason),
                    Text = new Text(rejectReasonText)
                };

            return ocj;
        }
    }
}