using System.Collections.Generic;
using System.Linq;
using CivicConnect.Domain.Entities;

namespace CivicConnect.Application.Patterns.Strategy;

public class CategoryValidationContext
{
    private readonly IEnumerable<ICategoryValidationStrategy> _strategies;

    public CategoryValidationContext(IEnumerable<ICategoryValidationStrategy> strategies)
    {
        _strategies = strategies;
    }

    public bool Validate(Request request)
    {
        var strategy = _strategies.FirstOrDefault(s => s.CategoryId == request.CategoryId);
        if (strategy == null)
        {
            // Default validation logic if no specific strategy found
            return !string.IsNullOrWhiteSpace(request.Title) && !string.IsNullOrWhiteSpace(request.Description);
        }

        return strategy.Validate(request);
    }
}
