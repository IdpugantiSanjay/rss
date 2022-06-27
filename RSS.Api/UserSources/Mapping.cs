using Cassandra.Mapping;

namespace RSS.Api.Sources;

public class SourceMapping: Mappings
{

    public SourceMapping()
    {
        For<UserSource>()
            .TableName("user_sources")
            .PartitionKey(u => u.Username)
            .Column(u => u.Url, cm => cm.WithName("url"))
            .Column(u => u.Name, cm => cm.WithName("name"))
            .Column(u => u.Username, cm => cm.WithName("username"))
            // .Column(u => u.Id, cm => cm.WithName("id"))
            // .Column(u => u.Id, cm => cm.WithName("Id"))
            // .Column(u => u.UpdateTime, cm => cm.WithName("last_updated"))
            ;
    }
    
}