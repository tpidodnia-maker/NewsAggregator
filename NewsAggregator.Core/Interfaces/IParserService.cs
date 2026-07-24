using NewsAggregator.Core.Entities;

namespace NewsAggregator.Core.Interfaces;

public interface IParserService
{
    Task<List<News>> ParseAllSourcesAsync();
}