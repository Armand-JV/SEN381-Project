using CivicConnect.Domain.Entities;

namespace CivicConnect.Application.Patterns.Strategy;

public interface ICategoryValidationStrategy
{
    int CategoryId { get; }
    bool Validate(Request request);
}
