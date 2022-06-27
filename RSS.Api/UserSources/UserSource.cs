using RSS.Api.Common.Fields;

namespace RSS.Api.Sources;

public record UserSource
{
    public string Url { get; init; } = null!;
    public string Username { get; init; } = null!;
    public string? Name { get; init; }
}