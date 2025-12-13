using System;
using System.Globalization;

namespace TalkBack.Converters;

public class EmptyViewSubtextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSearching)
        {
            return isSearching ? "Try a different search term" : "Go back and record something!";
        }
        return "Go back and record something!";
    }


    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }


}
