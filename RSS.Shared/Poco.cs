using JetBrains.Annotations;

namespace RSS.Shared;

public record ListUserSourceRequest(string Username);

public record ListUserSourceResponse(IEnumerable<ListUserSourceResponseEntity> Entities);


[UsedImplicitly]
public record ListUserSourceResponseEntity(string Name);

public record CreateUserSourceRequest(string Username, string Url) { public string? SourceName { get; init; } };

public record ListArticlesResponse(List<ListArticlesResponseEntity> Entities);

public record ListArticlesResponseEntity(string Name, string Id);

[UsedImplicitly]
public record GetArticleResponse(string Link);