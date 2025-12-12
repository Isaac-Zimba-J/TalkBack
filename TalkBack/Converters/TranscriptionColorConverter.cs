using System;
using System.Globalization;

namespace TalkBack.Converters;

public class TranscriptionColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isTranscribed)
        {
            return isTranscribed ? Color.FromArgb("#4CAF50") : Color.FromArgb("#FF9800");
        }
        return Color.FromArgb("#FF9800");
    }


    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }


}
