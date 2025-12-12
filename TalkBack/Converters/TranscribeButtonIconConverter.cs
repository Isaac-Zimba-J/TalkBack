using System;
using System.Globalization;

namespace TalkBack.Converters;

public class TranscribeButtonIconConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 3 &&
            values[0] is bool isTranscribed &&
            values[2] is string recordingId)
        {
            var currentlyTranscribingId = values[1] as string;

            if (currentlyTranscribingId == recordingId)
            {
                return "⏳"; // Transcribing...
            }

            return isTranscribed ? "📝" : "🎤";
        }
        return "🎤";
    }



    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }


}
