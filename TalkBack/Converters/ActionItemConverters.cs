using System;
using System.Globalization;

namespace TalkBack.Converters;

public class PriorityColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string priority)
        {
            return priority.ToLower() switch
            {
                "high" => Color.FromArgb("#FF4444"),
                "medium" => Color.FromArgb("#FFA500"),
                "low" => Color.FromArgb("#4CAF50"),
                _ => Color.FromArgb("#808080")
            };
        }
        return Color.FromArgb("#808080");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class PriorityTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string priority)
        {
            return priority.ToUpper();
        }
        return "MEDIUM";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class IsNotNullOrEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return !string.IsNullOrWhiteSpace(value?.ToString());
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ShowCompletedTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool showCompleted)
        {
            return showCompleted ? "Show Active" : "Show Completed";
        }
        return "Show Completed";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
