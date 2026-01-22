using System.Globalization;

namespace CodeQuizDesktop.Converters
{
    /// <summary>
    /// Converts a percentage (0-100) to a decimal (0.0-1.0) for ProgressBar.
    /// </summary>
    public class PercentToDecimalConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
                return doubleValue / 100.0;
            if (value is float floatValue)
                return floatValue / 100.0;
            if (value is int intValue)
                return intValue / 100.0;

            return 0.0;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
                return doubleValue * 100.0;
            return 0.0;
        }
    }
}
