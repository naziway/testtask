using System;
using System.Windows;
using Heathmill.FixAT.Client.Model;
using Heathmill.FixAT.Client.ViewModel;
using Heathmill.FixAT.Domain;

namespace Heathmill.FixAT.ATOrderBook.View
{
    /// <summary>
    /// Interaction logic for AddOrder.xaml
    /// </summary>
    public partial class AddOrder : Window
    {
        public AddOrder()
        {
            InitializeComponent();
            DataContext = new AddOrderViewModel();
        }

        public void SetDefaultClOrdID(string clOrdID)
        {
            GetViewModel().ClOrdID = clOrdID;
        }

        public OrderRecord GetOrderDetails()
        {
            var vm = GetViewModel();
            return new OrderRecord
            {
                ClOrdID = vm.ClOrdID,
                LastUpdateTime = DateTime.UtcNow,
                OrderID = string.Empty, // Set by the server
                OrdType = OrderType.Limit,
                Price = vm.Price,
                Side = vm.IsBuyChecked ? MarketSide.Bid : MarketSide.Ask,
                Quantity = vm.Quantity,
                Status = OrderStatus.New,
                Symbol = vm.Symbol
            };
        }

        private AddOrderViewModel GetViewModel()
        {
            return (AddOrderViewModel) DataContext;
        }

        private void OKButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
