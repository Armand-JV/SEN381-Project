using System;
using CivicConnect.Domain.Entities;
using CivicConnect.Domain.Enums;

namespace CivicConnect.Tests.Builders;

/// <summary>
/// Builder for <see cref="Request"/> instances used by tests. Defaults to a valid
/// "Infrastructure" request so that most tests can start from a working object.
/// </summary>
public class RequestBuilder
{
    private string _title = "Pothole on Main Street";
    private string _description = "Large pothole near the junction.";
    private int _categoryId = 1;
    private RequestState _state = RequestState.Open;
    private string _requesterId = "user123";
    private DateTime _createdAt = DateTime.UtcNow;

    public RequestBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public RequestBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public RequestBuilder WithCategoryId(int categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    public RequestBuilder WithState(RequestState state)
    {
        _state = state;
        return this;
    }

    public RequestBuilder WithRequesterId(string requesterId)
    {
        _requesterId = requesterId;
        return this;
    }

    public RequestBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public Request Build()
    {
        return new Request
        {
            Title = _title,
            Description = _description,
            CategoryId = _categoryId,
            State = _state,
            RequesterId = _requesterId,
            CreatedAt = _createdAt
        };
    }
}