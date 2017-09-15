using System;
using TaskMonitor.ViewModels;
using Windows.UI.Xaml.Data;

namespace TaskMonitor
{
    // Convert ulong bytes to Mb as a string.
    public class ByteToMbConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return null;
            }

            ulong ulongValue = (ulong)value;
            double convertedValue = (double)ulongValue / (1024 * 1024); 
            string returnValue = $"{convertedValue:N2} MB";
            return returnValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    // For platforms known to have no limit, and for cases where we get an extremely large limit
    // (which also indicates no limit), we return "n/a", otherwise the bytes converted to MB.
    public class MemoryLimitConverter : IValueConverter
    {
        private const ulong MB = 1024 * 1024;
        private const ulong ONE_GB = MB * 1024;

        // For memory limit, where there are no caps, we normally get ulong.MaxValue.
        // In some cases, we could get some large value that's close but < ulong.MaxValue.
        private const ulong ULONG_MAX_MINUS_ONE_GB = ulong.MaxValue - ONE_GB;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return null;
            }

            string returnValue = string.Empty;
            ulong ulongValue = (ulong)value;
            if (App.FormFactor == DeviceFormFactorType.Desktop || 
                App.FormFactor == DeviceFormFactorType.Tablet ||
                ulongValue >= ULONG_MAX_MINUS_ONE_GB)
            {
                returnValue = "n/a";
            }
            else if (ulongValue >= 0)
            {
                double convertedValue = (double)ulongValue / MB;
                returnValue = $"{convertedValue:N2} MB";
            }
            return returnValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    // If we couldn't get TimeSpan data, we return an empty string.
    public class CpuTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return null;
            }

            string returnValue = string.Empty;
            TimeSpan tmp = (TimeSpan)value;
            if (tmp != TimeSpan.MinValue)
            {
                returnValue = $"{value:h\\:mm\\:ss}";
            }
            return returnValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    // If the value is negative, return an empty string, otherwise the string representation of the value.
    public class NegativeIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return null;
            }

            string returnValue = string.Empty;
            int intValue = (int)value;
            if (intValue >= 0)
            {
                returnValue = value.ToString();
            }
            return returnValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    // For NotApplicable, we return an empty string, otherwise the string representation of the enum.
    public class EnergyQuotaStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return null;
            }

            string returnValue = string.Empty;
            EnergyQuotaStateEx energyState = (EnergyQuotaStateEx)value;
            if (energyState != EnergyQuotaStateEx.NotApplicable)
            {
                returnValue = energyState.ToString();
            }
            return returnValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return null;
            }

            if (parameter == null)
            {
                return value;
            }

            return string.Format((string)parameter, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            string language)
        {
            throw new NotImplementedException();
        }
    }

}
