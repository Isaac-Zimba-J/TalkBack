using System;
using System.Globalization;

namespace TalkBack.Converters;

public class TranscriptionStatusConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isTranscribed)
        {
            return isTranscribed ? "✓ Transcribed" : "⚠ Not transcribed";
        }
        return "⚠ Not transcribed";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }


}


