namespace MedicalReportEditor.Converters;

public class BoolToForegroundConverter : IValueConverter
{
    public Color TrueColor { get; set; } = Colors.Red;
    public Color FalseColor { get; set; } = Colors.Black;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? new SolidColorBrush(TrueColor) : new SolidColorBrush(FalseColor);
        }
        return new SolidColorBrush(Colors.Black);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}