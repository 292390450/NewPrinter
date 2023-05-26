using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;
using Knd.Printer.Model;
using System.Windows.Media.Imaging;

namespace Knd.Printer.Common.Converts
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return (!(bool)value) ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ColorToBrushConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return new SolidColorBrush((Color)value);
            }
            catch (Exception)
            {
                return new SolidColorBrush();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class StateToImgConverter : IValueConverter
    {
        private BitmapImage okImage = new BitmapImage(new Uri("pack://application:,,,/Knd.Printer.Common;Component/Resources/Images/ok.png"));

        private BitmapImage errorImage = new BitmapImage(new Uri("pack://application:,,,/Knd.Printer.Common;Component/Resources/Images/error.png"));

        private BitmapImage warnImage = new BitmapImage(new Uri("pack://application:,,,/Knd.Printer.Common;Component/Resources/Images/warn.png"));

        private BitmapImage workingImage = new BitmapImage(new Uri("pack://application:,,,/Knd.Printer.Common;Component/Resources/Images/working.png"));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PrinterState)
            {
                return (PrinterState)value switch
                {
                    PrinterState.就绪 => okImage,
                    PrinterState.错误 => errorImage,
                    PrinterState.警告 => warnImage,
                    PrinterState.打印中 => workingImage,
                    _ => okImage,
                };
            }
            return okImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
