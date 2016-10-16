using System.Collections.ObjectModel;
using System.Linq;
using Heathmill.FixAT.Client.Model;
using Heathmill.FixAT.Domain;

namespace Heathmill.FixAT.Client.ViewModel
{
    public class ATOrderBookViewModel : NotifyPropertyChangedBase
    {
        private readonly ATOrderMediator _orderMediator;

        public ObservableCollection<IcebergDisplay> IcebergOrders { get; set; }

        public ATOrderBookViewModel(IServerFacade serverFacade, ATOrderMediator orderMediator)
        {
            serverFacade.LogoutEvent += OnLogout;

            _orderMediator = orderMediator;
            _orderMediator.IcebergOrderUpdated += OnIcebergOrderUpdated;
            _orderMediator.IcebergOrderAdded += OnIcebergOrderAdded;

            IcebergOrders = new ObservableCollection<IcebergDisplay>();
        }

        public void CreateIcebergOrder(string symbol,
                                       string clOrdID,
                                       MarketSide side,
                                       decimal totalQuantity,
                                       decimal clipSize,
                                       decimal initialPrice,
                                       decimal priceDelta)
        {
            _orderMediator.AddIcebergOrder(symbol,
                                           clOrdID,
                                           side,
                                           totalQuantity,
                                           clipSize,
                                           initialPrice,
                                           priceDelta);
        }

        public string CreateClOrdID()
        {
            return _orderMediator.GenerateClOrdID();
        }

        private void OnIcebergOrderAdded(IcebergOrder obj)
        {
            IcebergOrders.Add(new IcebergDisplay(obj));
        }

        private void OnIcebergOrderUpdated(IcebergOrder order)
        {
            var disp = IcebergOrders.FirstOrDefault(io => io.ClOrdID == order.ClOrdID);
            if (disp != null)
            {
                disp.SetFromIcebergOrder(order);
            }
        }

        private void OnLogout()
        {
            // TODO Mark all AT orders as inactive (suspend if appropriate)
        }
    }
}
