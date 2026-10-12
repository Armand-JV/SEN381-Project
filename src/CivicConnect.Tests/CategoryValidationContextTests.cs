using System.Collections.Generic;
using System.Linq;
using CivicConnect.Application.Patterns.Strategy;
using CivicConnect.Domain.Entities;
using CivicConnect.Domain.Enums;
using CivicConnect.Tests.Builders;
using FluentAssertions;
using Moq;
using Xunit;

namespace CivicConnect.Tests;

/// <summary>
/// Unit tests for <see cref="CategoryValidationContext"/>, verifying the Strategy
/// pattern dispatches to the correct <see cref="ICategoryValidationStrategy"/> and
/// that the default fallback logic works when no strategy is registered.
/// </summary>
public class CategoryValidationContextTests
{
    [Fact]
    public void Validate_WithMatchingStrategy_DelegatesToStrategy()
    {
        // Arrange
        var request = new RequestBuilder().WithCategoryId(1).Build();

        var strategy = new Mock<ICategoryValidationStrategy>();
        strategy.Setup(s => s.CategoryId).Returns(1);
        strategy.Setup(s => s.Validate(request)).Returns(true);

        var context = new CategoryValidationContext(new[] { strategy.Object });

        // Act
        var result = context.Validate(request);

        // Assert
        result.Should().BeTrue();
        strategy.Verify(s => s.Validate(request), Times.Once);
    }

    [Fact]
    public void Validate_WithNoMatchingStrategy_UsesDefaultValidation()
    {
        // Arrange – no strategies registered for category 99
        var request = new RequestBuilder()
            .WithCategoryId(99)
            .WithTitle("Valid title")
            .WithDescription("Valid description")
            .Build();

        var context = new CategoryValidationContext(Enumerable.Empty<ICategoryValidationStrategy>());

        // Act
        var result = context.Validate(request);

        // Assert – default checks Title and Description are non-empty
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Valid description")]
    [InlineData("Valid title", "")]
    [InlineData("", "")]
    [InlineData("   ", "Valid description")]
    public void Validate_DefaultFallback_RejectsMissingTitleOrDescription(string title, string description)
    {
        // Arrange
        var request = new RequestBuilder()
            .WithCategoryId(99)
            .WithTitle(title)
            .WithDescription(description)
            .Build();

        var context = new CategoryValidationContext(Enumerable.Empty<ICategoryValidationStrategy>());

        // Act
        var result = context.Validate(request);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Validate_StrategyReturningFalse_PropagatesRejection()
    {
        // Arrange
        var request = new RequestBuilder().WithCategoryId(2).Build();

        var strategy = new Mock<ICategoryValidationStrategy>();
        strategy.Setup(s => s.CategoryId).Returns(2);
        strategy.Setup(s => s.Validate(It.IsAny<Request>())).Returns(false);

        var context = new CategoryValidationContext(new[] { strategy.Object });

        // Act
        var result = context.Validate(request);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Validate_MultipleStrategies_SelectsCorrectOne()
    {
        // Arrange
        var request = new RequestBuilder().WithCategoryId(2).Build();

        var infra = new Mock<ICategoryValidationStrategy>();
        infra.Setup(s => s.CategoryId).Returns(1);
        infra.Setup(s => s.Validate(It.IsAny<Request>())).Returns(false);

        var utilities = new Mock<ICategoryValidationStrategy>();
        utilities.Setup(s => s.CategoryId).Returns(2);
        utilities.Setup(s => s.Validate(It.IsAny<Request>())).Returns(true);

        var context = new CategoryValidationContext(new[] { infra.Object, utilities.Object });

        // Act
        var result = context.Validate(request);

        // Assert – category 2 strategy returns true, category 1 should not be called
        result.Should().BeTrue();
        infra.Verify(s => s.Validate(It.IsAny<Request>()), Times.Never);
        utilities.Verify(s => s.Validate(request), Times.Once);
    }
}
