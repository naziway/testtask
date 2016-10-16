
namespace Heathmill.FixAT.Client.ViewModel
{
    public class AddOrderViewModel : NotifyPropertyChangedBase
    {
        private string _clOrdID = "";
        public string ClOrdID
        {
            get { return _clOrdID; }
            set { _clOrdID = value; ValidationPropertyChanged("ClOrdID"); }
        }

        private string _symbol = "EURUSD";
        public string Symbol
        {
            get { return _symbol; }
            set { _symbol = value; ValidationPropertyChanged("Symbol"); }
        }

        private decimal _price;
        public decimal Price
        {
            get { return _price; }
            set { _price = value; ValidationPropertyChanged("Price"); }
        }

        private decimal _quantity;
        public decimal Quantity
        {
            get { return _quantity; }
            set { _quantity = value; ValidationPropertyChanged("Quantity"); }
        }

        private bool _isBuyChecked = true;
        public bool IsBuyChecked
        {
            get { return _isBuyChecked; }
            set { _isBuyChecked = value; ValidationPropertyChanged("IsBuyChecked"); }
        }

        private bool _isSellChecked = false;
        public bool IsSellChecked
        {
            get { return _isSellChecked; }
            set { _isSellChecked = value; ValidationPropertyChanged("IsSellChecked"); }
        }

        private bool _isValidOrder = false;
        public bool IsValidOrder
        {
            get { return _isValidOrder; }
            set { _isValidOrder = value; OnPropertyChanged("IsValidOrder"); }
        }

        private void ValidationPropertyChanged(string propertyName)
        {
            SetIsValidOrder();
            OnPropertyChanged(propertyName);
        }

        private void SetIsValidOrder()
        {
            bool isValid = 
                !string.IsNullOrWhiteSpace(ClOrdID) &&
                !string.IsNullOrWhiteSpace(Symbol) &&
                 Quantity > 0 &&
                 Price >= 0 &&
                 (IsBuyChecked || IsSellChecked);
            IsValidOrder = isValid;
        }
    }
}
