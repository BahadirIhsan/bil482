namespace IndustryApp.Application.Common;

/// <summary>Sayfalı liste sorgusu. Sayfa 1'den başlar.</summary>
public interface IPagedQuery
{
    int Page { get; }

    int PageSize { get; }
}
