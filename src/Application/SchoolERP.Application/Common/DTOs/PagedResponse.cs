namespace SchoolERP.Application.Common.DTOs;

public class PagedResponse<T>
{
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Size);
}