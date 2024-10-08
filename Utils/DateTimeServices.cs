namespace FXChangeWebAPI.Utils;

public static class DateTimeServices
{
    public static DateTime CurrentDateTime()
    {
        return DateTime.UtcNow.AddHours(-5);
    }
}
