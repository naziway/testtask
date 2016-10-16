using System.Collections.Generic;
using Heathmill.FixAT.Domain;
using QuickFix;

namespace Heathmill.FixAT.Server
{
    /// <summary>
    ///     The interface that is common between handlers for different versions of FIX
    /// </summary>
    internal interface IFixMessageHandler
    {
        void OnOrderFilled(SessionID sessionID, OrderMatch match);
        void SendOrdersToSession(SessionID sessionID, IEnumerable<IOrder> orders);
    }
}