using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Heathmill.FixAT.Domain
{
    // TODO Replace this with MEF type providers
    public static class OrderStackFactory
    {
        /// <summary>
        /// Create an order stack using default order sorting
        /// </summary>
        /// <param name="orderMatcher">The class to carry out order matching</param>
        /// <see cref="OrderSorter.StandardOrderSorter"/>
        public static OrderStack CreateStandardSortedStack(IOrderMatcher orderMatcher)
        {
            return new OrderStack(orderMatcher,
                                  new OrderStack.StandardOrderComparer());
        }
    }
}
