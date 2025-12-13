using System;
using System.Globalization;

namespace TalkBack.Converters;

public class EmptyViewIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSearching)
        {
            return isSearching ? "search.svg" : "note.svg";
        }
        return "note.svg";
    }



    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

}
