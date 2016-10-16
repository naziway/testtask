using System.Collections.Generic;
using QuickFix;
using QuickFix.FIX44;
using Message = QuickFix.Message;

namespace Heathmill.FixAT.Client
{
    public class EmptyFixStrategy : IFixStrategy
    {
        public SessionSettings SessionSettings { get; set; }

        public Dictionary<int, string> DefaultNewOrderSingleCustomFields
        {
            get { return new Dictionary<int, string>(); }
        }

        public void ProcessToAdmin(Message msg, Session session)
        {
            
        }

        public void ProcessToApp(Message msg, Session session)
        {
            
        }

        public void ProcessNewOrderSingle(NewOrderSingle nos)
        {
            
        }

        public void ProcessOrderCancelRequest(NewOrderSingle nos, OrderCancelRequest msg)
        {
            
        }

        public void ProcessOrderCancelReplaceRequest(NewOrderSingle nos,
                                                     OrderCancelReplaceRequest msg)
        {

        }
    }
}
