using System.Windows;
using System.Windows.Controls;
using Heathmill.FixAT.Client.ViewModel;
using Heathmill.FixAT.Domain;
using Heathmill.WpfUtilities;

namespace Heathmill.FixAT.ATOrderBook.View
{
    /// <summary>
    /// Interaction logic for OrderBook.xaml
    /// </summary>
    public partial class OrderBook : UserControl
    {
        public OrderBook()
        {
            InitializeComponent();
            CreateCommands();
        }

        // Ideally these would all be in the view model rather than the code-behind
        // but since ContextMenu isn't in the visual tree binding is a little trickier
        // http://stackoverflow.com/questions/3583507/wpf-binding-a-contextmenu-to-an-mvvm-command

        public RelayCommand BuyOrderCommand { get; private set; }
        public RelayCommand SellOrderCommand { get; private set; }

        private void CreateCommands()
        {
            BuyOrderCommand = new RelayCommand(
                o => TradeOrder(o, MarketSide.Bid),
                o => CanTradeOrder(o, MarketSide.Bid));

            SellOrderCommand = new RelayCommand(
                o => TradeOrder(o, MarketSide.Ask),
                o => CanTradeOrder(o, MarketSide.Ask));
        }

        private void TradeOrder(object o, MarketSide side)
        {
            var vm = o as OrderBookViewModel;
            var selectedOrderRow = LvOrders.SelectedItem;
            var osr = selectedOrderRow as OrderBookViewModel.OrderStackRow;
            if (vm == null || osr == null) return;

            if (side == MarketSide.Bid)
                vm.BuyOrder(osr);
            else
                vm.SellOrder(osr);
        }

        private bool CanTradeOrder(object o, MarketSide side)
        {
            var vm = o as OrderBookViewModel;
            var selectedOrderRow = LvOrders.SelectedItem;
            var osr = selectedOrderRow as OrderBookViewModel.OrderStackRow;
            if (vm == null || osr == null) return false;

            return !string.IsNullOrWhiteSpace(
                side == MarketSide.Bid ? osr.BidClOrdID : osr.AskClOrdID);
        }

        private void AddOrder_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = (OrderBookViewModel) DataContext;
            var dlg = new AddOrder();
            dlg.SetDefaultClOrdID(vm.CreateClOrdID());
            var result = dlg.ShowDialog();

            if (result.HasValue && result.Value)
            {
                var orderDetails = dlg.GetOrderDetails();
                vm.SubmitNewOrder(orderDetails);
            }
        }


    }
}
