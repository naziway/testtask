using System.Windows;
using Heathmill.FixAT.Client;
using Heathmill.FixAT.Client.Model;
using Heathmill.FixAT.Client.ViewModel;
using Heathmill.FixAT.Domain;

namespace Heathmill.FixAT.ATOrderBook.View
{
    /// <summary>
    /// Interaction logic for AddIcebergOrder.xaml
    /// </summary>
    public partial class AddIcebergOrder : Window
    {
        public AddIcebergOrder()
        {
            InitializeComponent();
            DataContext = new AddIcebergOrderViewModel();
        }

        public void SetDefaultClOrdID(string clOrdID)
        {
            GetViewModel().ClOrdID = clOrdID;
        }

        public string Symbol
        {
            get { return GetViewModel().Symbol; }
        }

        public string ClOrdID
        {
            get { return GetViewModel().ClOrdID; }
        }

        public MarketSide Side
        {
            get { return GetViewModel().IsBuyChecked ? MarketSide.Bid : MarketSide.Ask; }
        }

        public decimal TotalQuantity
        {
            get { return GetViewModel().TotalQuantity; }
        }

        public decimal ClipSize
        {
            get { return GetViewModel().ClipSize; }
        }

        public decimal Price
        {
            get { return GetViewModel().Price; }
        }

        public decimal PriceDelta
        {
            get { return GetViewModel().PriceDelta; }
        }

        private AddIcebergOrderViewModel GetViewModel()
        {
            return (AddIcebergOrderViewModel)DataContext;
        }

        private void OKButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
