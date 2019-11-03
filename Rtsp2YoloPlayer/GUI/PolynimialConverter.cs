using System;
using System.Windows.Data;
using System.Windows.Media;

namespace Rtsp2YoloPlayer.GUI
{
    public class PolynomialConverter : IValueConverter
    {
        public DoubleCollection Coefficients { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double x = (double)value;
            double output = 0;
            for (int i = Coefficients.Count - 1; i >= 0; i--)
                output += Coefficients[i] * Math.Pow(x, (Coefficients.Count - 1) - i);

            return output;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //This one is a bit tricky, if anyone feels like implementing this...
            throw new NotSupportedException();
        }
    }
}
