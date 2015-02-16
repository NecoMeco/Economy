﻿using System;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace DataGridFilterLibrary.Support
{
    public class VisibilityToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;

            return visibility == Visibility.Visible ? Double.NaN : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
