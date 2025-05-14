namespace API.Interfaces.Helpers;

public interface IDateTimeConverter
{
    DateTime getTimeFromRequest(string localDateTime, string? time);
}
