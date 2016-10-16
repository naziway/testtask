using Heathmill.FixAT.Domain;
using QuickFix;
using QuickFix.Fields;

namespace Heathmill.FixAT.Services
{
    public static class TranslateFixMessages
    {
        /// <summary>
        ///  Translate FIX42 NewOrderSingle message to OrderData
        /// </summary>
        /// <param name="n">The message</param>
        /// <returns>The order data</returns>
        /// <exception cref="QuickFix.IncorrectTagValue"></exception>
        public static OrderData Translate(QuickFix.FIX42.NewOrderSingle n)
        {
            ValidateIsSupportedOrderType(n.OrdType);

            return TranslateOrderImpl(n.Symbol,
                                      n.Side,
                                      n.OrdType,
                                      n.OrderQty,
                                      n.Price,
                                      n.ClOrdID,
                                      n.IsSetAccount() ? n.Account : null);
        }


        /// <summary>
        ///  Translate FIX44 NewOrderSingle message to OrderData
        /// </summary>
        /// <param name="n">The message</param>
        /// <returns>The order data</returns>
        /// <exception cref="QuickFix.IncorrectTagValue"></exception>
        public static OrderData Translate(QuickFix.FIX44.NewOrderSingle n)
        {
            ValidateIsSupportedOrderType(n.OrdType);

            return TranslateOrderImpl(n.Symbol,
                                      n.Side,
                                      n.OrdType,
                                      n.OrderQty,
                                      n.Price,
                                      n.ClOrdID,
                                      n.IsSetAccount() ? n.Account : null);
        }

        private static void ValidateIsSupportedOrderType(OrdType ordType)
        {
            switch (ordType.getValue())
            {
                case OrdType.LIMIT:
                    break;
                default:
                    throw new IncorrectTagValue(ordType.Tag);
            }
        }

        private static OrderData TranslateOrderImpl(Symbol symbol,
                                                    Side side,
                                                    OrdType ordType,
                                                    OrderQty orderQty,
                                                    Price price,
                                                    ClOrdID clOrdID,
                                                    Account account)
        {
            switch (ordType.getValue())
            {
                case OrdType.LIMIT:
                    if (price.Obj == 0)
                        throw new IncorrectTagValue(price.Tag);

                    break;

                    //case OrdType.MARKET: break;

                default:
                    throw new IncorrectTagValue(ordType.Tag);
            }

            return new OrderData
                {
                    MarketSide = TranslateFixFields.Translate(side),
                    Symbol = symbol.getValue(),
                    OrderType = TranslateFixFields.Translate(ordType),
                    Quantity = orderQty.getValue(),
                    Price = price.getValue(),
                    ClOrdID = clOrdID.getValue(),
                    Account =
                        account == null
                            ? TradingAccount.None
                            : new TradingAccount(account.getValue())
                };
        }

        public static long GetOrderIdFromMessage(QuickFix.FIX42.OrderCancelRequest msg)
        {
            if (!msg.IsSetOrderID())
                throw new IncorrectTagValue(msg.OrderID.Tag);

            var idString = msg.OrderID.getValue();
            long id;
            if (!string.IsNullOrEmpty(idString) && long.TryParse(idString, out id))
            {
                return id;
            }

            throw new IncorrectTagValue(msg.OrderID.Tag);
        }

        public static long GetOrderIdFromMessage(QuickFix.FIX44.OrderCancelRequest msg)
        {
            if (!msg.IsSetOrderID())
                throw new IncorrectTagValue(msg.OrderID.Tag);

            var idString = msg.OrderID.getValue();
            long id;
            if (!string.IsNullOrEmpty(idString) && long.TryParse(idString, out id))
            {
                return id;
            }

            throw new IncorrectTagValue(msg.OrderID.Tag);
        }
    }
}
