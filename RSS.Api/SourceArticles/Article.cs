namespace RSS.Api.SourceArticles;

public record Article
{
    public string SourceName { get; init; } = null!;
    
    public string ArticleName { get; init; } = null!;
    
    public string Link { get; init; } = null!;
    
    public Guid Id { get; init; }
}

public record UserFavoriteArticle(string Username, DateTimeOffset CreatedTime): Article;