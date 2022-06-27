namespace RSS.Api.Common.Responses;

public interface IListResponse<T>
{
    int PageSize { get; }
    
    string NextPageToken { get; }
    
    List<T> Entities { get; }
    
    public int? TotalSize { get; }
}

