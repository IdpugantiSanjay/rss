namespace RSS.Api.Sources;

public record Source
{
    public string Name { get; init; } = null!;
    
    public string FeedLink { get; init; } = null!;
    
    public string Link { get; init; } = null!;

    public string FaviconLink { get; init; } = null!;

    public DateTimeOffset LastCheckedTime { get; init; }
    
    public DateTimeOffset LastUpdatedTime { get; init; }
}