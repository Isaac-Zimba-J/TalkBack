using System.Globalization;

namespace TalkBack.Converters;

public class PlayPauseImageConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isPlaying)
        {
            return isPlaying ? "pause.svg" : "play.png";
        }
        return "play.png";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class TranscribeImageConverter : IMultiValueConverter
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
                return "transcribing.svg"; // Loading/processing icon
            }

            return isTranscribed ? "transcribed.svg" : "transcribe.svg";
        }
        return "transcribe.svg";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
