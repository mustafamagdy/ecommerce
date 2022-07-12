namespace FSH.WebApi.Application.Common.Models;

public class PaginationResponse<T>
{
    public PaginationResponse(List<T> data, int totalCount, int currentPage, int pageSize)
    {
        Data = data;
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        TotalCount = totalCount;
    }

    public List<T> Data { get; set; }

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public int TotalCount { get; set; }

    public int PageSize { get; set; }

    public bool HasPreviousPage => CurrentPage > 1;

    public bool HasNextPage => CurrentPage < TotalPages;
}