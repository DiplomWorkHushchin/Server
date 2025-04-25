namespace API.Extensions;

public static class DateTimeExtensions
{
    public static int CalculateAge(this DateTime dob)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var age = today.Year - dob.Year;

        if (dob.Month > today.Month || (dob.Month == today.Month && dob.Day > today.Day)) age--;

        return age;
    }
}
