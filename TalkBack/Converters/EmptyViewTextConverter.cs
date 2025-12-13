using System;
using System.Globalization;

namespace TalkBack.Converters;

public class EmptyViewTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSearching)
        {
            return isSearching ? "No results found" : "No recordings yet";
        }
        return "No recordings yet";
    }


    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }


}
