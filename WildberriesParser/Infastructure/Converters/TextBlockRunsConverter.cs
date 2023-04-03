using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;

namespace WildberriesParser.Infastructure.Converters
{
    public class TextBlockRunsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var runs = values[0] as ObservableCollection<Run>;
            var inlines = new ObservableCollection<Inline>();
            if (runs != null)
            {
                foreach (var run in runs)
                {
                    inlines.Add(run);
                }
            }
            return inlines;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}