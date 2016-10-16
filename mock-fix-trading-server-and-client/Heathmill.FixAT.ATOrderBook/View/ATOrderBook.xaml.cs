using System.Windows;
using System.Windows.Controls;
using Heathmill.FixAT.Client.ViewModel;

namespace Heathmill.FixAT.ATOrderBook.View
{
    /// <summary>
    /// Interaction logic for ATOrderBook.xaml
    /// </summary>
    public partial class ATOrderBook : UserControl
    {
        public ATOrderBook()
        {
            InitializeComponent();
        }

        private void AddIcebergOrder_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = (ATOrderBookViewModel)DataContext;
            var dlg = new AddIcebergOrder();
            dlg.SetDefaultClOrdID(vm.CreateClOrdID());
            var result = dlg.ShowDialog();

            if (result.HasValue && result.Value)
            {
                vm.CreateIcebergOrder(dlg.Symbol,
                                      dlg.ClOrdID,
                                      dlg.Side,
                                      dlg.TotalQuantity,
                                      dlg.ClipSize,
                                      dlg.Price,
                                      dlg.PriceDelta);
            }
        }
    }
}
