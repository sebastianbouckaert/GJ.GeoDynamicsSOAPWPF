using System;
using System.Globalization;

namespace GJ.GeoDynamics.Common
{
    public static class StringExtensions
    {
        public static string FormatToSqlDateTimeString(this string? dateString)
        {
            if (string.IsNullOrEmpty(dateString)) return "";

            if (DateTime.TryParse(dateString, out DateTime result))
            {
                // Force "/" as separator regardless of machine culture
                return result.ToString("dd/MM/yyyy H:mm:ss", CultureInfo.InvariantCulture);
            }

            return dateString ?? "";
        }
    }
}