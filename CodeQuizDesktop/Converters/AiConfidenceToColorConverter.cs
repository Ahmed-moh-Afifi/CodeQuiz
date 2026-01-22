using System.Globalization;

namespace CodeQuizDesktop.Converters
{
    /// <summary>
    /// Converts AI confidence score to appropriate color.
    /// High confidence (>= 0.7) = Green
    /// Medium confidence (>= 0.4) = Yellow
    /// Low confidence (< 0.4) = Red
    /// </summary>
    public class AiConfidenceToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is float confidence)
            {
                if (confidence >= 0.7f)
                    return Color.FromArgb("#22C55E"); // Green
                if (confidence >= 0.4f)
                    return Color.FromArgb("#EAB308"); // Yellow
                return Color.FromArgb("#EF4444"); // Red
            }
            return Color.FromArgb("#6B7280"); // Gray for null/unknown
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
