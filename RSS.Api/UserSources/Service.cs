using System.ServiceModel.Syndication;
using System.Xml;
using Cassandra.Mapping;
using JetBrains.Annotations;

namespace RSS.Api.Sources;

public class Service
{
    private readonly IMapper _mapper;

    public Service(IMapper mapper)
    {
        _mapper = mapper;
    }

    public record ListInput(string Username);

    public record ListOutput(IEnumerable<ListOutputEntity> Entities);

    [UsedImplicitly]
    public record ListOutputEntity(string Name, string Url);

    public async Task<ListOutput> List(ListInput input)
    {
        const string cql = @"
            SELECT name, url FROM rss.user_sources
            WHERE username = ?
        ";
        var vms = await _mapper.FetchAsync<ListOutputEntity>(cql, input.Username);
        return new(vms);
    }

    public record CreateInput(string Username, string Url);
    
    public async Task Create(CreateInput input)
    {
        var reader = XmlReader.Create(input.Url);
        var feed = SyndicationFeed.Load(reader);
        var source = new UserSource {Name = feed.Title.Text, Url = input.Url, Username = input.Username};
        await _mapper.InsertAsync(source);
    }

    public async Task Remove(string name)
    {
        const string cql = @"
            DELETE FROM rss.user_sources
            WHERE name = ?
        ";
        await _mapper.DeleteAsync(cql, name);
    }

    public record GetOptions();

    public record GetInput(string Username, Guid Id, GetOptions Options);

    public async Task<UserSource> Get(GetInput input)
    {
        const string cql = @"
           SELECT name, url FROM rss.user_sources
           WHERE username = ? AND id = ?
        ";

        return await _mapper.SingleAsync<UserSource>(cql, input.Username, input.Id);
    }
}