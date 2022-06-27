namespace RSS.Api.Common.Fields;

public interface IStandardFields
{
    public DateTimeOffset CreateTime { get; }
    
    public DateTimeOffset UpdateTime { get; }
}