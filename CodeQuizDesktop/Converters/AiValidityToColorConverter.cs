using System.Globalization;

namespace CodeQuizDesktop.Converters
{
    /// <summary>
    /// Converts AI validity status to appropriate color.
    /// </summary>
    public class AiValidityToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isValid)
            {
                return isValid ? Color.FromArgb("#22C55E") : Color.FromArgb("#EF4444"); // Green or Red
            }
            return Color.FromArgb("#6B7280"); // Gray for null/unknown
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
