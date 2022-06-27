using System.Linq.Expressions;
using System.ServiceModel.Syndication;
using System.Xml;
using Cassandra.Mapping;
using HtmlAgilityPack;

namespace RSS.Api.Sources;

public class Service
{
    private readonly IMapper _mapper;

    public Service(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task<Source> Get(string sourceName)
    {
        return await _mapper.SingleAsync<Source>(@"SELECT * FROM rss.sources WHERE name = ?", sourceName);
    }

    public async Task SetLastCheckedTime(Source source)
    {
        await _mapper.UpdateAsync(source);
    }

    public async Task<Source> Create(string url, string? sourceName = null)
    {
        if (sourceName is not null)
            return await CreateWithoutFeed(url, sourceName);
        
        var web = new HtmlWeb();
        var uri = new Uri(url);
        var host = $"{uri.Scheme}://{uri.Host}";
        var doc = await web.LoadFromWebAsync(host);
        var faviconLinkNode = doc.DocumentNode.SelectSingleNode("/html/head/link[@rel='icon' and @href]") ?? doc.DocumentNode.SelectSingleNode("/html/head/link[@rel='shortcut icon' and @href]");
        var faviconLinkNodeHrefValue = faviconLinkNode.GetAttributeValue("href", "");
        var faviconLink = faviconLinkNodeHrefValue.StartsWith("/") ? $"{host}{faviconLinkNodeHrefValue}" : faviconLinkNodeHrefValue;
        
        var reader = XmlReader.Create(url);

        try
        {
            var feed = SyndicationFeed.Load(reader);

            var source = await _mapper.SingleOrDefaultAsync<Source>(@"
            SELECT * FROM rss.sources WHERE name = ? AND feed_link = ?
        ", feed.Title.Text, url);

            if (source is not null) return source;

            source = new Source
            {
                Name = feed.Title.Text,
                FeedLink = url,
                Link = feed.Links.Count > 1
                    ? feed.Links.ElementAt(1).Uri.ToString()
                    : feed.Links.ElementAt(0).Uri.ToString(),
                FaviconLink = faviconLink,
                LastUpdatedTime = feed.LastUpdatedTime
            };
            await _mapper.InsertAsync(source);
            return source;
        }
        catch (XmlException e)
        {
            return await CreateWithoutFeed(url, sourceName);
        }
    }

    private async Task<Source> CreateWithoutFeed(string url, string? sourceName)
    {
        var source = await _mapper.SingleOrDefaultAsync<Source>(@"
                SELECT * FROM rss.sources WHERE name = ?
            ", url);

        if (source is not null) return source;

        source = new Source
        {
            Name = sourceName ?? new Uri(url).Host,
            Link = url,
        };

        return source;
    }
}