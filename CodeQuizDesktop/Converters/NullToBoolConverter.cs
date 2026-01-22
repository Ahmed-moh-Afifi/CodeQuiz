using System.Globalization;

namespace CodeQuizDesktop.Converters
{
    /// <summary>
    /// Converts null to false, non-null to true.
    /// Use parameter "invert" to reverse the logic.
    /// </summary>
    public class NullToBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isNotNull = value != null;

            if (parameter is string paramStr && paramStr.Equals("invert", StringComparison.OrdinalIgnoreCase))
            {
                return !isNotNull;
            }

            return isNotNull;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
