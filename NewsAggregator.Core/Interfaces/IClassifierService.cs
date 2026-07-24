namespace NewsAggregator.Core.Interfaces;

public interface IClassifierService
{
    string Classify(string text);
}