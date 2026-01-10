using System.Globalization;
using CodeQuizDesktop.Models;

namespace CodeQuizDesktop.Converters
{
    /// <summary>
    /// Converter that compares the current question with the selected question to determine if it's selected.
    /// Returns true if the question's Order matches the selected question's Order.
    /// </summary>
    public class IsSelectedQuestionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return false;

            // values[0] = current question's Order (int)
            // values[1] = selected question (Question)
            if (values[0] is int currentOrder && values[1] is Question selectedQuestion)
            {
                return currentOrder == selectedQuestion.Order;
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter that returns a background color based on whether the question is selected.
    /// Returns Primary color if selected, SurfaceDark if not.
    /// </summary>
    public class SelectedQuestionBackgroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return Application.Current!.Resources["SurfaceDark"];

            // values[0] = current question's Order (int)
            // values[1] = selected question (Question)
            if (values[0] is int currentOrder && values[1] is Question selectedQuestion)
            {
                if (currentOrder == selectedQuestion.Order)
                {
                    return Application.Current!.Resources["Primary"];
                }
            }

            return Application.Current!.Resources["SurfaceDark"];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter that returns a border stroke color based on whether the question is selected.
    /// Returns Primary color if selected, BorderDefault if not.
    /// </summary>
    public class SelectedQuestionBorderConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return Application.Current!.Resources["BorderDefault"];

            // values[0] = current question's Order (int)
            // values[1] = selected question (Question)
            if (values[0] is int currentOrder && values[1] is Question selectedQuestion)
            {
                if (currentOrder == selectedQuestion.Order)
                {
                    return Application.Current!.Resources["Primary"];
                }
            }

            return Application.Current!.Resources["BorderDefault"];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
