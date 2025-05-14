using API.Interfaces.Helpers;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace API.Helpers;

public class DateTimeConverter : IDateTimeConverter
{
    public DateTime getTimeFromRequest(string utcDateTime, string? time)
    {
        var date = DateTime.ParseExact(utcDateTime, "yyyy-MM-dd", CultureInfo.InvariantCulture);

        if (TimeSpan.TryParse(time, out var dueTime))
        {
            var localDateTime = new DateTime(date.Year, date.Month, date.Day, dueTime.Hours, dueTime.Minutes, 0, DateTimeKind.Utc);
            return localDateTime.ToUniversalTime();
        }

        var defaultLocal = new DateTime(date.Year, date.Month, date.Day, 23, 59, 0, DateTimeKind.Utc);
        return defaultLocal.ToUniversalTime();
    }
}
