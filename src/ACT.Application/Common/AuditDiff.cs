namespace ACT.Application.Common;

public static class AuditDiff
{
    /// <summary>
    /// Compares old/new values for a set of named fields and returns only the ones that actually
    /// changed, ready to hand to IAuditService.LogChangesAsync. Comparison is by .ToString() so
    /// callers can pass enums, DateTimes, ints, etc. alongside strings.
    /// </summary>
    public static List<(string Field, string? Old, string? New)> Compare(
        params (string Field, object? Old, object? New)[] fields)
    {
        var changes = new List<(string, string?, string?)>();
        foreach (var (field, oldVal, newVal) in fields)
        {
            var oldStr = oldVal?.ToString();
            var newStr = newVal?.ToString();
            if (oldStr != newStr)
                changes.Add((field, oldStr, newStr));
        }
        return changes;
    }
}
