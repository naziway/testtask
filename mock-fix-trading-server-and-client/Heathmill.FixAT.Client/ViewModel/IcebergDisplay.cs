using System;
using System.Windows.Input;
using Heathmill.FixAT.Client.Model;
using Heathmill.FixAT.Domain;
using Heathmill.WpfUtilities;

namespace Heathmill.FixAT.Client.ViewModel
{
    public class IcebergDisplay : NotifyPropertyChangedBase
    {
        private IcebergOrder _icebergOrder;

        // TODO Allow the user to update the order price as a demo of changed a rule while its running
        // _icebergOrder.SetNewOrderPrice

        public IcebergDisplay(IcebergOrder icebergOrder)
        {
            SetFromIcebergOrder(icebergOrder);
            
             _activateIcebergCommand = new RelayCommand(
                param => ((IcebergDisplay) param).Activate(),
                param => param != null && ((IcebergDisplay) param).CanActivate);

            _suspendIcebergCommand = new RelayCommand(
                param => ((IcebergDisplay) param).Suspend(),
                param => param != null && ((IcebergDisplay)param).CanSuspend);

            //_updateIcebergOrder = new RelayCommand(
            //    param => ((IcebergDisplay) param).);
        }

        public void SetFromIcebergOrder(IcebergOrder io)
        {
            if (io == null) throw new ArgumentNullException("io");
            _icebergOrder = io;

            Symbol = io.Symbol;
            ClOrdID = io.ClOrdID;
            Side = io.Side;
            InitialPrice = io.InitialPrice;
            CurrentPrice = io.CurrentPrice;
            PriceDelta = io.PriceDelta;
            RemainingQuantity = io.RemainingQuantity;
            TotalQuantity = io.TotalQuantity;
            CurrentQuantity = io.CurrentQuantity;
            ClipSize = io.ClipSize;
            LastTradedTime = io.LastTradedTime.HasValue
                                 ? io.LastTradedTime.Value.ToLocalTime()
                                 : io.LastTradedTime;
            SetStatus(io.State);
        }

        // Properties that the user cannot change (and will not be able to change) for the
        // iceberg orders have a private setter and no property changed since they're immutable.

        public string Symbol { get; private set; }

        public string ClOrdID { get; private set; }

        public MarketSide Side { get; private set;  }


        private decimal _initialPrice;
        public decimal InitialPrice
        {
            get { return _initialPrice; }
            set { SetIfChanged(ref _initialPrice, value, "InitialPrice"); }
        }

        private decimal _currentPrice;
        public decimal CurrentPrice
        {
            get { return _currentPrice; }
            set { SetIfChanged(ref _currentPrice, value, "CurrentPrice"); }
        }

        private decimal _priceDelta;
        public decimal PriceDelta
        {
            get { return _priceDelta; }
            set { SetIfChanged(ref _priceDelta, value, "PriceDelta"); }
        }

        private decimal _remainingQuantity;
        public decimal RemainingQuantity
        {
            get { return _remainingQuantity; }
            set { SetIfChanged(ref _remainingQuantity, value, "RemainingQuantity"); }
        }

        private decimal _totalQuantity;
        public decimal TotalQuantity
        {
            get { return _totalQuantity; }
            set { SetIfChanged(ref _totalQuantity, value, "TotalQuantity"); }
        }

        private decimal _currentQuantity;
        public decimal CurrentQuantity
        {
            get { return _currentQuantity; }
            set { SetIfChanged(ref _currentQuantity, value, "CurrentQuantity"); }
        }

        private decimal _clipSize;
        public decimal ClipSize
        {
            get { return _clipSize; }
            set { SetIfChanged(ref _clipSize, value, "ClipSize"); }
        }

        private DateTime? _lastTradedTime;
        public DateTime? LastTradedTime
        {
            get { return _lastTradedTime; }
            set 
            {
                SetIfChanged(ref _lastTradedTime, value, "LastTradedTime");
                FlashOrder = true;
            }
        }

        private IcebergOrder.ActivationState _state;
        public string Status
        {
            get
            {
                switch (_state)
                {
                    case IcebergOrder.ActivationState.Active:
                        return "Active";
                    case IcebergOrder.ActivationState.PendingActivationAcceptance:
                        return "Pending activation";
                    case IcebergOrder.ActivationState.PendingSuspension:
                        return "Pending suspension";
                    case IcebergOrder.ActivationState.Suspended:
                        return "Suspended";
                    default:
                        return "Unknown";
                }
            }
        }

        public void SetStatus(IcebergOrder.ActivationState status)
        {
            SetIfChanged(ref _state, status, "Status");
        }

        public bool FlashOrder { get; set; }

        private void SetIfChanged<T>(ref T prop, T newVal, string propName)
        {
            if (prop.Equals(newVal)) return;
            prop = newVal;
            OnPropertyChanged(propName);
        }

        private bool CanActivate
        {
            get
            {
                return 
                    RemainingQuantity > 0 &&
                    _state == IcebergOrder.ActivationState.Suspended;
            }
        }

        private bool CanSuspend
        {
            get { return _state == IcebergOrder.ActivationState.Active; }
        }

        private bool CanSetNewValues
        {
            get { return _icebergOrder.CanSetNewValues(); }
        }

        private void Activate()
        {
            _icebergOrder.Activate();
        }

        private void Suspend()
        {
            _icebergOrder.Suspend();
        }

        private readonly RelayCommand _activateIcebergCommand;
        public ICommand ActivateIcebergCommand
        {
            get { return _activateIcebergCommand; }
        }

        private readonly RelayCommand _suspendIcebergCommand;
        public ICommand SuspendIcebergCommand
        {
            get { return _suspendIcebergCommand; }
        }

        //private readonly RelayCommand _updateIcebergOrder;
        //public ICommand UpdateIcebergCommand
        //{
        //    get { return _updateIcebergOrder; }
        //}
    }
}
