using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Rack300SimulationUI.Converter
{
    public class ColorCodeToSideLightColor : IValueConverter
    {
        public Brush NoColor = Brushes.White;
        public Brush Green = Brushes.Green;
        public Brush Red = Brushes.Red;
        public Brush Orange = Brushes.Orange;
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            if (value is int colorCode)
            {
                switch (colorCode)
                {
                    case 0:
                        return NoColor;
                    case 1:
                        return Green;
                    case 2:
                        return Red;
                    case 3:
                        return Orange;
                    default:
                        return NoColor;
                }
            }

            return NoColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureInfo) =>
            Binding.DoNothing;
    }
}
