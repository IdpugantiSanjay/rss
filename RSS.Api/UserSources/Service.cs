using Cassandra.Mapping;
using JetBrains.Annotations;
using RSS.Shared;

namespace RSS.Api.UserSources;

using SourceService = Sources.Service;

public class Service
{
    private readonly IMapper _mapper;
    private readonly SourceService _sourceService;

    public Service(IMapper mapper, SourceService sourceService)
    {
        _mapper = mapper;
        _sourceService = sourceService;
    }

    public async Task<ListUserSourceResponse> List(ListUserSourceRequest userSourceRequest)
    {
        const string cql = @"
            SELECT name FROM rss.user_sources
            WHERE username = ?
        ";
        var vms = await _mapper.FetchAsync<ListUserSourceResponseEntity>(cql, userSourceRequest.Username);
        return new(vms);
    }

    

    public async Task<UserSource> Create(CreateUserSourceRequest userSourceRequest)
    {
        var source = await _sourceService.Create(userSourceRequest.Url, userSourceRequest.SourceName);
        var userSource = new UserSource {Name = source.Name, Username = userSourceRequest.Username};

        var existingUserSource = _mapper.SingleOrDefault<UserSource>(@"
            SELECT * FROM rss.user_sources WHERE username = ? AND name = ?
        ", userSourceRequest.Username , source.Name);

        if (existingUserSource is { }) return existingUserSource;
        
        await _mapper.InsertAsync(userSource);

        return userSource;
    }

    // public async Task Remove(string username, string name)
    // {
    //     const string cql = @"
    //         DELETE FROM rss.user_sources
    //         WHERE username = ? AND name = ?
    //     ";
    //     await _mapper.DeleteAsync<UserSource>(cql, username, name);
    // }

    // public async Task<UserSource> Get(GetInput input)
    // {
    //     const string cql = @"
    //        SELECT name, url FROM rss.user_sources
    //        WHERE username = ? AND id = ?
    //     ";
    //
    //     return await _mapper.SingleAsync<UserSource>(cql, input.Username, input.Id);
    // }
}