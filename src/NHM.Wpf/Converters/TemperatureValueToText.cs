﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace NHM.Wpf.Converters
{
    class TemperatureValueToText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return Translations.Tr(value);
            if (value is float temp && temp > -1)
            {
                return $"{temp:F2}ºC";
            }
            return "- - -";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}