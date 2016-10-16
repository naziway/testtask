using Heathmill.FixAT.Domain;
using QuickFix;
using QuickFix.Fields;

namespace Heathmill.FixAT.Services
{
    public static class TranslateFixFields
    {
        public static MarketSide Translate(Side side)
        {
            switch (side.getValue())
            {
                case Side.BUY:
                    return MarketSide.Bid;
                case Side.SELL:
                    return MarketSide.Ask;
                default:
                    throw new IncorrectTagValue(side.Tag);
            }
        }

        public static Side Translate(MarketSide side)
        {
            switch (side)
            {
                case MarketSide.Bid:
                    return new Side(Side.BUY);
                case MarketSide.Ask:
                    return new Side(Side.SELL);
                default:
                    throw new ServicesException("Unknown MarketSide when translating " + side);
            }
        }

        public static OrderType Translate(OrdType type)
        {
            switch (type.getValue())
            {
                case OrdType.LIMIT:
                    return OrderType.Limit;
                default:
                    throw new IncorrectTagValue(type.Tag);
            }
        }

        public static OrdType Translate(OrderType type)
        {
            switch (type)
            {
                case OrderType.Limit:
                    return new OrdType(OrdType.LIMIT);
                default:
                    throw new ServicesException("Unknown OrderType when translating " + type);
            }
        }

        public static OrderStatus Translate(OrdStatus ordStatus)
        {
            switch (ordStatus.Obj)
            {
                case ExecType.NEW:
                    return OrderStatus.New;
                case ExecType.PARTIAL_FILL:
                    return OrderStatus.PartiallyFilled;
                case ExecType.FILL:
                    return OrderStatus.Filled;
                case ExecType.CANCELED:
                    return OrderStatus.Canceled;
                case ExecType.REPLACED:
                    return OrderStatus.Replaced;
                case ExecType.REJECTED:
                    return OrderStatus.Rejected;
                case ExecType.SUSPENDED:
                    return OrderStatus.Suspended;
                case ExecType.TRADE:
                    return OrderStatus.Traded;
            }
            return OrderStatus.Unknown;
        }
    }
}
