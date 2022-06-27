namespace RSS.Api.UserSources;

public record UserSource
{
    public string Username { get; init; } = null!;
    public string Name { get; init; } = null!;
}