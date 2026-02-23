using System;

namespace GJ.GeoDynamics.Common
{
    public static class StringExtensions
    {
        public static string FormatToSqlDateTimeString(this string? dateString)
        {
            if (string.IsNullOrEmpty(dateString)) return "";

            if (DateTime.TryParse(dateString, out DateTime result))
            {
                // Format: 8/01/2026 0:00:00
                return result.ToString("d/MM/yyyy H:mm:ss");
            }

            return dateString ?? "";
        }
    }
}