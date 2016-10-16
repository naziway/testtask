using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Heathmill.FixAT.Client.ViewModel;

namespace Heathmill.FixAT.Client.Converters
{
    [ValueConversion(typeof(OrderBookViewModel.OrderStackRow), typeof(Brush))]
    public class ColorToSolidBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new SolidColorBrush((Color)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack not supported");
        }
    }
}
