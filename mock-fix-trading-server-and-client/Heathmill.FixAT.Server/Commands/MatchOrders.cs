
namespace Heathmill.FixAT.Server.Commands
{
    internal class MatchOrders : ICommand
    {
        private readonly OrderMediator _orderMediator;
        private readonly string _symbol;

        public MatchOrders(OrderMediator orderMediator, string symbol)
        {
            _orderMediator = orderMediator;
            _symbol = symbol;
        }

        public void Execute()
        {
            _orderMediator.MatchOrders(_symbol);
        }
    }
}
