namespace Portfolio.Models;

public static class ExperienceDuration
{
    public static string Format(DateOnly? startDate, DateOnly? endDate)
    {
        if (startDate is null || endDate is null || endDate < startDate)
            return string.Empty;

        var start = startDate.Value;
        var end = endDate.Value;
        var months = (end.Year - start.Year) * 12 + end.Month - start.Month;
        if (end.Day < start.Day) months--;
        if (months < 0) return string.Empty;
        if (months == 0) return "Menos de 1 mês";

        var years = months / 12;
        var remainingMonths = months % 12;
        var parts = new List<string>(2);
        if (years > 0) parts.Add($"{years} {(years == 1 ? "ano" : "anos")}");
        if (remainingMonths > 0) parts.Add($"{remainingMonths} {(remainingMonths == 1 ? "mês" : "meses")}");
        return string.Join(" e ", parts);
    }

    public static string FormatMonthYear(DateOnly? date) => date?.ToString("MM/yyyy") ?? "";

    public static string FormatRange(DateOnly? start, DateOnly? end, bool isCurrent = false)
    {
        var startText = FormatMonthYear(start);
        var endText = isCurrent ? "Presente" : FormatMonthYear(end);
        if (string.IsNullOrEmpty(startText)) return endText;
        return string.IsNullOrEmpty(endText) ? startText : $"{startText} — {endText}";
    }
}
