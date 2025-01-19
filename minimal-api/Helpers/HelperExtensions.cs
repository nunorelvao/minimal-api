using System;
using System.Globalization;

namespace minimal_api.Helpers
{
    public static class HelperExtensions
    {
        static string format = "yyyyMMdd'T'HHmmssff'Z'";
        public static DateTimeOffset ToUniversalDateTimeOffset(this string datetime)
        {
           return  DateTimeOffset.ParseExact(datetime, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        }

        public static string FromUniversalDateTimeOffset(this DateTimeOffset datetime)
        {
            return datetime.ToString(format);
        }
    }
}
