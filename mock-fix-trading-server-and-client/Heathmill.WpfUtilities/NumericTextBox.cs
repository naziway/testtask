using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Heathmill.WpfUtilities
{
    public class NumericTextBox : TextBox
    {
        public string BindingPath
        {
            get { return GetValue(BindingPathProperty).ToString(); }
            set { SetValue(BindingPathProperty, value); }
        }

        public static readonly DependencyProperty BindingPathProperty =
            DependencyProperty.Register(
                "BindingPath",
                typeof (string),
                typeof (NumericTextBox),
                new PropertyMetadata(string.Empty));

        public bool IsNegativeAllowed
        {
            get { return (bool) GetValue(IsNegativeAllowedProperty); }
            set { SetValue(IsNegativeAllowedProperty, value); }
        }

        public static readonly DependencyProperty IsNegativeAllowedProperty =
            DependencyProperty.Register(
                "IsNegativeAllowed",
                typeof (bool),
                typeof (NumericTextBox),
                new PropertyMetadata(true));

        public bool ZeroIsNull
        {
            get { return (bool) GetValue(ZeroIsNullProperty); }
            set { SetValue(ZeroIsNullProperty, value); }
        }

        public static readonly DependencyProperty ZeroIsNullProperty =
            DependencyProperty.Register(
                "ZeroIsNull",
                typeof (bool),
                typeof (NumericTextBox),
                new PropertyMetadata(false));

        private readonly IMarshallToUi _uiExecutive;

        private INotifyPropertyChanged _bindingSource;

        private PropertyInfo _bindingProperty;

        private decimal? _lastBoundValue;

        private bool _isIntegerField, _isSettable, _isTextChanging;

        public NumericTextBox()
        {
            DataContextChanged += NumericTextBox_DataContextChanged;
            _uiExecutive = new SimpleUiExecutive(this.Dispatcher);
        }

        private void BindingSource_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _bindingProperty.Name) _uiExecutive.Marshall(UpdateText);
        }

        private void NumericTextBox_DataContextChanged(
            object sender,
            DependencyPropertyChangedEventArgs e)
        {
            ResetBinding();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property.Name == "BindingPath") ResetBinding();
        }

        private void ResetBinding()
        {
            ClearOldBinding();
            var source = ResolveNewBindingSource();
            if (source == null) return;
            var prop = ResolveNewBindingProperty(source);
            if (prop == null) return;
            SetNewBinding(source, prop);
            UpdateText();
        }

        private void ClearOldBinding()
        {
            if (_bindingSource != null)
                _bindingSource.PropertyChanged -= BindingSource_PropertyChanged;
            _bindingSource = null;
            _bindingProperty = null;
        }

        private INotifyPropertyChanged ResolveNewBindingSource()
        {
            if (string.IsNullOrWhiteSpace(BindingPath)) return null;
            object source = DataContext;
            string[] path = BindingPath.Split('.');
            for (int i = 0; i < path.Length - 1; i++)
            {
                if (source == null) break;
                PropertyInfo propinfo = source.GetType().GetProperty(path[i]);
                source = propinfo == null ? null : propinfo.GetValue(source, null);
            }
            return source as INotifyPropertyChanged;
        }

        private PropertyInfo ResolveNewBindingProperty(INotifyPropertyChanged source)
        {
            return source.GetType().GetProperty(BindingPath.Split('.').Last());
        }

        private void SetNewBinding(INotifyPropertyChanged source, PropertyInfo prop)
        {
            _bindingSource = source;
            _bindingSource.PropertyChanged += BindingSource_PropertyChanged;
            _bindingProperty = prop;
            _isIntegerField = _bindingProperty.PropertyType == typeof (int?)
                              || _bindingProperty.PropertyType == typeof (byte?);
            _isSettable = _bindingProperty.CanSet();
        }

        private void UpdateText()
        {
            if (_isTextChanging) return;
            UpdateText(GetBoundValue());
        }

        private object GetBoundValue()
        {
            return _bindingProperty.GetValue(_bindingSource, null);
        }

        private void UpdateText(object value)
        {
            if (_isTextChanging) return;
            if (value == null)
                Text = ZeroIsNull ? "0" : string.Empty;
            else
            {
                string s = ValueToString(value);
                if (s != Text)
                {
                    if (string.IsNullOrEmpty(s))
                        Text = string.Empty;
                    else
                    {
                        decimal x;
                        bool ok = decimal.TryParse(s, out x);
                        if (ok) Text = s;
                    }
                }
            }
        }

        private string ValueToString(object value)
        {
            string s = value.ToString();
            while (s.Contains('.') && (s.EndsWith("0") || s.EndsWith(".")))
                s = s.Substring(0, s.Length - 1);
            return s;
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            if (_isTextChanging) return;
            _isTextChanging = true;
            try
            {
                int savestart = SelectionStart;
                Text = Text.Replace(',', '.');
                SelectionStart = savestart;
                decimal x;
                bool ok = decimal.TryParse(Text, out x);
                if (ok)
                {
                    if (_lastBoundValue != x)
                    {
                        _lastBoundValue = x;
                        UpdateSource(x);
                    }
                }
                else
                {
                    _lastBoundValue = null;
                    NullifySource();
                }
            }
            finally
            {
                _isTextChanging = false;
            }
        }

        private void NullifySource()
        {
            _bindingProperty.SetValue(_bindingSource, null, null);
        }

        private void UpdateSource(decimal x)
        {
            object v = x;
            try
            {
                if (_bindingProperty.PropertyType == typeof (byte?))
                    v = (byte) x;
                else if (_bindingProperty.PropertyType == typeof (int?))
                    v = (int) x;
                else if (_bindingProperty.PropertyType == typeof (double?))
                    v = (double) x;
            }
            catch
            {
                v = null;
            }
            _bindingProperty.SetValue(_bindingSource, v, null);
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);
            if (e.Handled) return;
            string newtext = e.Text;
            string prefix = Text.Substring(0, SelectionStart);
            string suffix = Text.Substring(SelectionStart + SelectionLength);
            string remaining = prefix + suffix;
            e.Handled = HasKeyError(newtext, remaining);
        }

        private bool HasKeyError(string newtext, string remaining)
        {
            bool hasDecimalPointAlready = _isIntegerField || remaining.Contains('.');
            for (int i = 0; i < newtext.Length; i++)
            {
                char c = newtext[i];
                if (char.IsDigit(c)) continue;
                bool error;
                switch (c)
                {
                    case ',':
                    case '.':
                        error = hasDecimalPointAlready;
                        hasDecimalPointAlready = true;
                        break;
                    case '-':
                        error = !IsNegativeAllowed || remaining.Contains('-')
                                || i > 0 || SelectionStart > 0;
                        break;
                    default:
                        error = true;
                        break;
                }
                if (error) return true;
            }
            return false;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Handled) return;
            e.Handled = e.Key == Key.Space;
        }

        public static Type[] CompatibleTypes
        {
            get { return BackingCompatibleTypes; }
        }

        private static readonly Type[] BackingCompatibleTypes =
        {
            typeof (decimal?), typeof (double?), typeof (float?), typeof (int?), typeof (byte?)
        };
    }
}
