using System.Globalization;

namespace CodeQuizDesktop.Converters
{
    /// <summary>
    /// Converts a boolean to a pass/fail symbol.
    /// True = ✓, False = ✗
    /// </summary>
    public class BoolToPassFailConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isSuccess)
            {
                return isSuccess ? "✓" : "✗";
            }
            return "?";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
