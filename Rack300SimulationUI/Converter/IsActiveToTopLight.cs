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
    public class IsActiveToTopLight:IValueConverter
    {
        public Brush ActiveColor = Brushes.Green;
        public Brush InactiveColor = Brushes.Red;
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            if (value is bool isActive)
            {
                if (isActive)
                {
                    return ActiveColor;
                }
                else
                {
                    return InactiveColor;
                }
            }

            return InactiveColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureInfo) =>
            Binding.DoNothing;

    }
}
