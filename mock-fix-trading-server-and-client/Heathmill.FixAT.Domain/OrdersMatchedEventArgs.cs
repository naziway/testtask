using System;
using System.Collections.Generic;

namespace Heathmill.FixAT.Domain
{
    public class OrdersMatchedEventArgs : EventArgs
    {
        public OrdersMatchedEventArgs(List<OrderMatch> matches)
        {
            _matches = matches;
        }

        public IEnumerable<OrderMatch> OrderMatches
        {
            get { return _matches; }
        }

        private readonly List<OrderMatch> _matches;
    }
}
