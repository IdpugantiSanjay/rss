namespace RSS.Api.Common.Requests;

public interface IListRequest
{
    public int PageSize { get; init; }

    public string PageToken { get; init; }
    
    public string? OrderBy { get; init; }
}