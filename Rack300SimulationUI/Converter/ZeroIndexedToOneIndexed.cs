using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Rack300SimulationUI.Converter
{
    public class ZeroIndexedToOneIndexed : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            if (value is int index)
            {
                return (index+1);
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            if (value is int index)
            {
                return (index - 1);
            }

            return 0;
        }
    }
}
