namespace ACT.Application.Common;

public static class Paging
{
    public const int MaxPageSize = 100;

    /// <summary>
    /// Clamps page/pageSize to sane bounds before they reach a repository — a single
    /// authenticated caller can't force a full-table scan/serialize with e.g. ?pageSize=999999999.
    /// Clamp here (not just in the repository) so the returned PagedResult's Page/PageSize/
    /// TotalPages describe what was actually queried, instead of echoing back the raw request.
    /// </summary>
    public static (int Page, int PageSize) Clamp(int page, int pageSize)
        => (Math.Max(page, 1), Math.Clamp(pageSize, 1, MaxPageSize));
}
