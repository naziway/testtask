
using System.Text;

namespace Heathmill.FixAT.Domain
{
    public static class OrderExtensions
    {
        public static bool HasBetterPriceThan(this IOrder x, IOrder y)
        {
            if (x.MarketSide != y.MarketSide)
                throw new DomainException(
                    "Trying to compare prices for orders on different market sides");

            return x.MarketSide == MarketSide.Ask
                       ? x.Price < y.Price
                       : x.Price > y.Price;
        }

        public static string GetPropertiesString(this IOrder order)
        {
            var bld = new StringBuilder();
            bld.Append("[");
            var properties = order.GetType().GetProperties();
            foreach (var p in properties)
            {
                var getter = p.GetGetMethod();
                var o = getter.Invoke(order, new object[] { });
                bld.AppendFormat("{0}={1};", p.Name, o);
            }
            bld.Append("]");
            return bld.ToString();
        }
    }
}
