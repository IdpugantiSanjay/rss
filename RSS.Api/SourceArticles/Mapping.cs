using Cassandra.Mapping;

namespace RSS.Api.SourceArticles;

public class SourceArticleMapping: Mappings
{
    public SourceArticleMapping()
    {
        For<Article>()
            .TableName("source_articles")
            .Column(sa => sa.Id, cm => cm.WithName("id"))
            .Column(sa => sa.Link, cm => cm.WithName("link"))
            .Column(sa => sa.ArticleName, cm => cm.WithName("article_name"))
            .Column(sa => sa.SourceName, cm => cm.WithName("source_name"))
            ;
    }
}

public class FavoriteArticleMapping: Mappings
{
    public FavoriteArticleMapping()
    {
        For<UserFavoriteArticle>()
            .TableName("user_favorites")
            .PartitionKey(uf => uf.Username)
            .Column(uf => uf.Username, cm => cm.WithName("username"))
            .Column(uf => uf.CreatedTime, cm => cm.WithName("created_time"))
            .Column(sa => sa.Id, cm => cm.WithName("id"))
            .Column(sa => sa.Link, cm => cm.WithName("link"))
            .Column(sa => sa.ArticleName, cm => cm.WithName("article_name"))
            .Column(sa => sa.SourceName, cm => cm.WithName("source_name"))
            ;
    }
}