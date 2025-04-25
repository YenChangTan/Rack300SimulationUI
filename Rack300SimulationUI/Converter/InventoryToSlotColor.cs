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
    public class InventoryToSlotColor : IValueConverter
    {
        public Brush FilledBrush { get; set; } = Brushes.Gray;
        public Brush EmptyBrush { get; set; } = Brushes.Black;
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            if (value is bool boolean)
            {
                return boolean ? FilledBrush : EmptyBrush;
            }

            return EmptyBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureInfo) =>
            Binding.DoNothing;
    }
}
