using System.Collections.Generic;
using Heathmill.FixAT.Domain;

namespace Heathmill.FixAT.Services
{
    public interface IFixMessageGenerator
    {
        QuickFix.Message CreateNewOrderExecutionReport(IOrder o, string execID);

        QuickFix.Message CreateNewOrderSingleMessage(string symbol,
                                                     MarketSide marketSide,
                                                     string clOrdID,
                                                     TradingAccount account,
                                                     decimal price,
                                                     decimal quantity,
                                                     OrderType orderType,
                                                     string execID);

        QuickFix.Message CreateRejectNewOrderExecutionReport(string symbol,
                                                             MarketSide marketSide,
                                                             string clOrdID,
                                                             decimal orderQuantity,
                                                             TradingAccount account,
                                                             string execID,
                                                             string rejectionReason,
                                                             int? rejectionCode = null);

        QuickFix.Message CreateFillReport(OrderMatch match, string execID);

        QuickFix.Message CreateOrderCancelMessage(string symbol,
                                                  string clOrdID,
                                                  string newClOrdID,
                                                  MarketSide side,
                                                  string orderID);

        QuickFix.Message CreateOrderCancelAccept(IOrder cancelledOrder,
                                                 string execID);

        QuickFix.Message CreateOrderCancelReject(long orderID,
                                                 string clOrdID,
                                                 string origClOrdID,
                                                 int rejectionReason,
                                                 string rejectionReasonText);

        QuickFix.Message CreateOrderCancelReplaceReject(long orderID,
                                                        string clOrdID,
                                                        string origClOrdID,
                                                        int rejectionReason,
                                                        string rejectionReasonText);

        QuickFix.Message CreateNews(string headline);
        QuickFix.Message CreateNews(string headline, IList<string> lines);
    }
}
