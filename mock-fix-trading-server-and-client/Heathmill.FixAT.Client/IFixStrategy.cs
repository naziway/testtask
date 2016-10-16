using System.Collections.Generic;

namespace Heathmill.FixAT.Client
{
    public interface IFixStrategy
    {
        QuickFix.SessionSettings SessionSettings { get; set; }

        Dictionary<int, string> DefaultNewOrderSingleCustomFields { get; }

        void ProcessToAdmin(QuickFix.Message msg, QuickFix.Session session);
        void ProcessToApp(QuickFix.Message msg, QuickFix.Session session);

        /// <summary>
        /// Modify a newly-created NewOrderSingle before it is sent to the server
        /// </summary>
        /// <param name="nos"></param>
        void ProcessNewOrderSingle(QuickFix.FIX44.NewOrderSingle nos);

        /// <summary>
        /// Modify a newly-created OrderCancelRequest in some way before it is sent out.  May throw exceptions.
        /// </summary>
        /// <param name="nos">the message that created the order being canceled</param>
        /// <param name="msg">the cancel message to be modified</param>
        void ProcessOrderCancelRequest(
            QuickFix.FIX44.NewOrderSingle nos,
            QuickFix.FIX44.OrderCancelRequest msg);

        /// <summary>
        /// Modify a newly-created OrderCancelReplaceRequest in some way before it is sent out.  May throw exceptions.
        /// </summary>
        /// <param name="nos">the message that created the order being canceled</param>
        /// <param name="msg">the cancel message to be modified</param>
        void ProcessOrderCancelReplaceRequest(
            QuickFix.FIX44.NewOrderSingle nos,
            QuickFix.FIX44.OrderCancelReplaceRequest msg);
    }
}
