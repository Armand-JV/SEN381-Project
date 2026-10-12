using CivicConnect.Domain.Entities;

namespace CivicConnect.Application.Patterns.Strategy;

public class Category1ValidationStrategy : ICategoryValidationStrategy
{
    public int CategoryId => 1;
    public bool Validate(Request request)
    {
        // Require description for Category 1
        return !string.IsNullOrWhiteSpace(request.Description);
    }
}
