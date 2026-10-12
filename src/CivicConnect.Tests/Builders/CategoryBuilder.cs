using CivicConnect.Domain.Entities;

namespace CivicConnect.Tests.Builders;

/// <summary>
/// Builder for <see cref="Category"/> instances. Defaults to the seeded "Infrastructure" row.
/// </summary>
public class CategoryBuilder
{
    private int _id = 1;
    private string _name = "Infrastructure";
    private string _description = "Roads, bridges, etc.";

    public CategoryBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public CategoryBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public CategoryBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public Category Build()
    {
        return new Category
        {
            Id = _id,
            Name = _name,
            Description = _description
        };
    }
}