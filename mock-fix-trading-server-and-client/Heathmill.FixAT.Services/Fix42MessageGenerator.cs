using System;
using System.Collections.Generic;
using System.Globalization;
using Heathmill.FixAT.Domain;
using QuickFix.Fields;
using QuickFix.FIX42;
using MatchType = Heathmill.FixAT.Domain.MatchType;
using Message = QuickFix.Message;

namespace Heathmill.FixAT.Services
{
    public class Fix42MessageGenerator : IFixMessageGenerator
    {
        public Message CreateNewOrderExecutionReport(IOrder o, string execID)
        {
            var exReport = new ExecutionReport(
                new OrderID(o.ID.ToString(CultureInfo.InvariantCulture)),
                new ExecID(execID),
                new ExecTransType(ExecType.NEW),
                new ExecType(ExecType.NEW),
                new OrdStatus(OrdStatus.NEW),
                new Symbol(o.Contract.Symbol),
                TranslateFixFields.Translate(o.MarketSide),
                new LeavesQty(o.Quantity),
                new CumQty(0m),
                new AvgPx(o.Price))
                {
                    ClOrdID = new ClOrdID(o.ClOrdID),
                    OrderQty = new OrderQty(o.Quantity),
                    LastShares = new LastShares(0m)
                };
            //exReport.Set(new LastPx(o.Price));

            if (TradingAccount.IsSet(o.Account))
                exReport.SetField(new Account(o.Account.Name));

            return exReport;
        }

        public Message CreateNewOrderSingleMessage(string symbol,
                                                   MarketSide marketSide,
                                                   string clOrdID,
                                                   TradingAccount account,
                                                   decimal price,
                                                   decimal quantity,
                                                   OrderType orderType,
                                                   string execID)
        {
            // hard-coded fields
            var fHandlInst =
                new HandlInst(HandlInst.AUTOMATED_EXECUTION_ORDER_PRIVATE);

            var fOrdType = TranslateFixFields.Translate(orderType);
            var fSide = TranslateFixFields.Translate(marketSide);
            var fSymbol = new Symbol(symbol);
            var fTransactTime = new TransactTime(DateTime.Now);
            var fClOrdID = new ClOrdID(clOrdID);

            var nos = new NewOrderSingle(fClOrdID,
                                         fHandlInst,
                                         fSymbol,
                                         fSide,
                                         fTransactTime,
                                         fOrdType)
            {
                OrderQty = new OrderQty(quantity),
                TimeInForce = new TimeInForce(TimeInForce.GOOD_TILL_CANCEL)
            };

            if (orderType == OrderType.Limit)
                nos.Price = new Price(price);

            return nos;
        }

        public Message CreateRejectNewOrderExecutionReport(string symbol,
                                                           MarketSide marketSide,
                                                           string clOrdID,
                                                           decimal orderQuantity,
                                                           TradingAccount account,
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
               new Symbol(symbol), 
               TranslateFixFields.Translate(marketSide),
               new LeavesQty(0m),
               new CumQty(0m),
               new AvgPx(0m))
            {
                ClOrdID = new ClOrdID(clOrdID),
                OrderQty = new OrderQty(orderQuantity)
            };

            if (rejectionCode.HasValue)
            {
                exReport.OrdRejReason = new OrdRejReason(rejectionCode.Value);
            }

            if (TradingAccount.IsSet(account))
            {
                exReport.Account = new Account(account.Name);
            }

            return exReport;
        }

        // From fixprotocol.org: 
        // CumQty: Currently executed shares for chain of orders.
        // LeavesQty: Amount of shares open for further execution. If the OrdStatus is Canceled,
        //            DoneForTheDay, Expired, Calculated, or Rejected (in which case the order
        //            is no longer active) then LeavesQty could be 0,
        //            otherwise LeavesQty = OrderQty - CumQty.
        // LastShares: Quantity of shares bought/sold on this (last) fill.
        //
        // Also see http://www.onixs.biz/fix-dictionary/4.2/msgType_8_8.html
        // The general rule is: OrderQty <38> = CumQty <14> + LeavesQty <151>.

        public Message CreateFillReport(OrderMatch match, string execID)
        {
            var exReport = new ExecutionReport(
                new OrderID(match.OrderID.ToString(CultureInfo.InvariantCulture)),
                new ExecID(execID),
                new ExecTransType(ExecTransType.NEW),
                new ExecType(match.MatchType == MatchType.Full
                                 ? ExecType.FILL
                                 : ExecType.PARTIAL_FILL),
                new OrdStatus(match.MatchType == MatchType.Full
                                  ? OrdStatus.FILLED
                                  : OrdStatus.PARTIALLY_FILLED),
                new Symbol(match.Contract.Symbol),
                TranslateFixFields.Translate(match.MarketSide),
                new LeavesQty(match.RemainingQuantity),
                new CumQty(match.OriginalOrderQuantity - match.RemainingQuantity),
                new AvgPx(match.Price))
                {
                    ClOrdID = new ClOrdID(match.ClOrdID),
                    OrderQty = new OrderQty(match.OriginalOrderQuantity),
                    LastShares = new LastShares(match.MatchedQuantity),
                    LastPx = new LastPx(match.Price)
                };

            if (TradingAccount.IsSet(match.Account))
                exReport.SetField(new Account(match.Account.Name));

            return exReport;
        }

        public Message CreateOrderCancelMessage(string symbol,
                                                string clOrdID,
                                                string newClOrdID,
                                                MarketSide side,
                                                string orderID)
        {
            var ocq = new OrderCancelRequest(new OrigClOrdID(clOrdID),
                                             new ClOrdID(newClOrdID),
                                             new Symbol(symbol),
                                             TranslateFixFields.Translate(side),
                                             new TransactTime(DateTime.Now))
            {
                OrderID = new OrderID(orderID)
            };

            return ocq;
        }

        public Message CreateOrderCancelAccept(IOrder cancelledOrder, string execID)
        {
            var exReport = new ExecutionReport(
                new OrderID(cancelledOrder.ID.ToString(CultureInfo.InvariantCulture)),
                new ExecID(execID),
                new ExecTransType(ExecTransType.NEW),
                new ExecType(ExecType.CANCELED),
                new OrdStatus(OrdStatus.CANCELED),
                new Symbol(cancelledOrder.Contract.Symbol),
                TranslateFixFields.Translate(cancelledOrder.MarketSide),
                new LeavesQty(0m), // 0 because the order is being cancelled
                new CumQty(cancelledOrder.FilledQuantity),
                new AvgPx(cancelledOrder.Price))
                {
                    OrderQty = new OrderQty(cancelledOrder.OriginalQuantity),
                    ClOrdID = new ClOrdID(cancelledOrder.ClOrdID)
                };

            return exReport;
        }

        public Message CreateOrderCancelReject(long orderID,
                                               string clOrdID,
                                               string origClOrdID,
                                               int rejectionReason,
                                               string rejectionReasonText)
        {
            return new OrderCancelReject(
                new OrderID(orderID.ToString(CultureInfo.InvariantCulture)),
                new ClOrdID(clOrdID),
                new OrigClOrdID(origClOrdID),
                new OrdStatus(OrdStatus.REJECTED),
                new CxlRejResponseTo(CxlRejResponseTo.ORDER_CANCEL_REQUEST))
            {
                CxlRejReason = new CxlRejReason(rejectionReason),
                Text = new Text(rejectionReasonText)
            };
        }

        public Message CreateOrderCancelReplaceReject(long orderID,
                                                      string clOrdID,
                                                      string origClOrdID,
                                                      int rejectionReason,
                                                      string rejectionReasonText)
        {
            return new OrderCancelReject(
                new OrderID(orderID.ToString(CultureInfo.InvariantCulture)),
                new ClOrdID(clOrdID),
                new OrigClOrdID(origClOrdID),
                new OrdStatus(OrdStatus.REJECTED),
                new CxlRejResponseTo(CxlRejResponseTo.ORDER_CANCEL_REPLACE_REQUEST))
            {
                CxlRejReason = new CxlRejReason(rejectionReason),
                Text = new Text(rejectionReasonText)
            };
        }

        public Message CreateNews(string headline)
        {
            return CreateNews(headline, new List<string>());
        }

        public Message CreateNews(string headline, IList<string> lines)
        {
            var h = new Headline(headline);

            var news = new News(h);
            foreach (var s in lines)
            {
                var group = new News.LinesOfTextGroup { Text = new Text(s) };
                news.AddGroup(group);
            }

            if (lines.Count == 0)
            {
                var noLines = new LinesOfText(0);
                news.SetField(noLines, true);
            }

            return news;
        }
    }
}
