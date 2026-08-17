namespace SchoolERP.Application.Common.DTOs;

public class PagedRequest
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortOrder { get; set; } = "desc";
    public string? SearchTerm { get; set; }
}