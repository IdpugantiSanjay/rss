using System.Globalization;
using System.ServiceModel.Syndication;
using System.Xml;
using Cassandra;
using Cassandra.Mapping;
using HtmlAgilityPack;
using RSS.Api.Sources;
using RSS.Shared;
using Service = RSS.Api.UserSources.Service;
using SourceService = RSS.Api.Sources.Service;

namespace RSS.Api.SourceArticles;

public class ArticleService
{
    private readonly IMapper _mapper;
    private readonly SourceService _sourceService;
    private readonly Service _userSourceService;

    public ArticleService(IMapper mapper, SourceService sourceService, Service userSourceService)
    {
        _mapper = mapper;
        _sourceService = sourceService;
        _userSourceService = userSourceService;
    }

    public async Task<ListArticlesResponse> List(string sourceName)
    {
        var vms = (await _mapper.FetchAsync<Article>(
            @"SELECT article_name, id FROM rss.source_articles WHERE source_name = ?",
            sourceName)).Select(s => new ListArticlesResponseEntity(s.ArticleName, s.Id.ToString())).ToList();
        return new(vms);
    }

    public async Task<Article?> Create(string username, string articleUrl)
    {
        var uri = new Uri(articleUrl);
        var hostUrl = $"{uri.Scheme}://{uri.Host}";

        if (articleUrl.Contains("medium.com", StringComparison.InvariantCultureIgnoreCase))
        {
            var article = await HandleMedium(username, articleUrl);
            if (article is null) return null;
            await _userSourceService.Create(
                new CreateUserSourceRequest(username, hostUrl) {SourceName = article.SourceName});
            await _mapper.InsertAsync(article);
            return article;
        }

        var web = new HtmlWeb();
        var doc = await web.LoadFromWebAsync(hostUrl);

        var linkNode = doc.DocumentNode.SelectSingleNode("//link[@type=\"application/rss+xml\"]") ??
                       doc.DocumentNode.SelectSingleNode("//link[@type=\"application/atom+xml\"]");

        if (linkNode is null)
        {
            var anchorNodes = doc.DocumentNode.SelectNodes("//a");

            foreach (var anchorNode in anchorNodes)
            {
                var href = anchorNode.GetAttributeValue("href", "");
                if ((href.Contains("rss") || href.Contains("feed")) && href.Contains("xml"))
                {
                    linkNode = anchorNode;
                    break;
                }
            }
        }

        if (linkNode is null)
        {
            doc = await web.LoadFromWebAsync(articleUrl);

            linkNode = doc.DocumentNode.SelectSingleNode("//link[@type=\"application/rss+xml\"]") ??
                       doc.DocumentNode.SelectSingleNode("//link[@type=\"application/atom+xml\"]");

            if (linkNode?.OuterHtml.Contains("comments", StringComparison.InvariantCultureIgnoreCase) ?? false)
                linkNode = null;
        }


        if (linkNode is not null)
        {
            var sourceUrl = linkNode.GetAttributeValue<string>("href", "");

            if (sourceUrl.Contains("Comments", StringComparison.InvariantCultureIgnoreCase)) return null;
            if (sourceUrl.StartsWith("/")) sourceUrl = $"{hostUrl}{sourceUrl}";

            var reader = XmlReader.Create(sourceUrl);
            var feed = SyndicationFeed.Load(reader);
            var sourceName = feed.Title.Text;
            string? articleName = null;

            foreach (var item in feed.Items)
            {
                if (item.Links is null) continue;
                if (articleName is not null) break;

                foreach (var itemLink in item.Links)
                {
                    if (itemLink.Uri.PathAndQuery == uri.PathAndQuery ||
                        itemLink.Uri.PathAndQuery.Contains(uri.PathAndQuery))
                    {
                        articleName = item.Title.Text;
                        break;
                    }
                }
            }


            if (articleName is null)
            {
                doc = await web.LoadFromWebAsync(articleUrl);
                articleName = doc.DocumentNode.SelectSingleNode("//h1")?.InnerText;
            }

            if (articleName is null) return null;

            var article = new Article
                {Id = TimeUuid.NewId(), Link = articleUrl, ArticleName = articleName, SourceName = sourceName};

            await _userSourceService.Create(new CreateUserSourceRequest(username, sourceUrl));
            await _mapper.InsertAsync(article);

            return article;
        }
        else
        {
            var sourceName = uri.Host;
            doc = await web.LoadFromWebAsync(articleUrl);
            var articleName = doc.DocumentNode.SelectSingleNode("//h1")?.InnerText;

            if (articleName is null) return null;

            var article = new Article
                {Id = TimeUuid.NewId(), Link = articleUrl, ArticleName = articleName, SourceName = sourceName};

            await _userSourceService.Create(new CreateUserSourceRequest(username, hostUrl));
            await _mapper.InsertAsync(article);

            return article;
        }
    }

    public async Task<Article?> HandleMedium(string username, string articleUrl)
    {
        var web = new HtmlWeb();
        var doc = await web.LoadFromWebAsync(articleUrl);
        var articleName = doc.DocumentNode.SelectSingleNode("//h1")?.InnerText;

        if (articleName is null) return null;

        var uri = new Uri(articleUrl);
        var sourceName = uri.AbsolutePath.Trim('/').Split("/")[0];
        var titleCaseSourceName = new CultureInfo("en-US", false).TextInfo.ToTitleCase(sourceName).Replace('-', ' ');

        return new Article
            {ArticleName = articleName, Id = TimeUuid.NewId(), SourceName = titleCaseSourceName, Link = articleUrl};
    }

    public async Task<Article> Get(string source, string id)
    {
        var article = await _mapper.SingleOrDefaultAsync<Article>(
            @"SELECT * FROM rss.source_articles WHERE source_name = ? AND id = ?", source,
            TimeUuid.Parse(id));

        return article;
    }

    public async Task<Article?> MarkAsFavorite(string username, string source, string id)
    {
        var article = await _mapper.SingleOrDefaultAsync<Article>(@"
            SELECT * FROM rss.source_articles WHERE source_name = ? AND id = ?
        ", source, TimeUuid.Parse(id));

        if (article is not { }) return null;
        
        var poco = new UserFavoriteArticle(username, DateTimeOffset.UtcNow)
            {Id = article.Id, Link = article.Link, ArticleName = article.ArticleName, SourceName = article.SourceName};
        
        await _mapper.InsertAsync(poco);
        return poco;
    }

    public async Task Poll(string sourceName)
    {
        var source = await _sourceService.Get(sourceName);

        if ((DateTimeOffset.UtcNow - source.LastCheckedTime).TotalMinutes < 60)
            return;

        var reader = XmlReader.Create(source.FeedLink);
        var feed = SyndicationFeed.Load(reader);

        if (source.LastUpdatedTime == feed.LastUpdatedTime)
        {
            await _sourceService.SetLastCheckedTime(source with {LastCheckedTime = DateTimeOffset.UtcNow});
            return;
        }

        var sourceArticles =
            (await _mapper.FetchAsync<Article>(@"SELECT * FROM rss.source_articles WHERE source_name = ?",
                sourceName)).ToList();

        foreach (var item in feed.Items)
        {
            if (sourceArticles.Any(sa => sa.ArticleName == item.Title.Text)) continue;

            await _mapper.InsertAsync(new Article()
            {
                Id = TimeUuid.NewId(), Link = item.Links.First().Uri.ToString(), ArticleName = item.Title.Text,
                SourceName = sourceName
            });
        }

        await _sourceService.SetLastCheckedTime(source with {LastCheckedTime = DateTimeOffset.UtcNow});
    }
}