using Cassandra.Mapping;

namespace RSS.Api.Sources;

public class SourceMapping: Mappings
{
    public SourceMapping()
    {
        For<Source>()
            .TableName("sources")
            .PartitionKey(s => s.Name, s => s.FeedLink, s => s.LastUpdatedTime)
            .Column(s => s.Link, cm => cm.WithName("link"))
            .Column(s => s.Name, cm => cm.WithName("name"))
            .Column(s => s.FeedLink, cm => cm.WithName("feed_link"))
            .Column(s => s.LastUpdatedTime, cm => cm.WithName("last_updated"))
            .Column(s => s.FaviconLink, cm => cm.WithName("favicon_link"))
            .Column(s => s.LastCheckedTime, cm => cm.WithName("last_checked_timestamp").WithDbType<DateTimeOffset>())
            ;
    }   
}