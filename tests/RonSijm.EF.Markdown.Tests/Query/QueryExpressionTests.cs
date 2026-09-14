using System.Linq.Expressions;
using RonSijm.EF.Markdown.Tests.TestModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RonSijm.EF.Markdown.Query.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

#pragma warning disable EF1001 // Internal EF Core API usage

namespace RonSijm.EF.Markdown.Tests.Query;

public class QueryExpressionTests : IClassFixture<TestFixture>, IDisposable
{
    private readonly TestFixture _fixture;
    private readonly string _testDir;

    public QueryExpressionTests(TestFixture fixture)
    {
        _fixture = fixture;
        _testDir = _fixture.CreateTestDirectory();
    }

    [Fact]
    public void MarkdownQueryExpression_Properties_ReturnExpectedValues()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();
        
        var entityType = context.Model.FindEntityType(typeof(TestEntity))!;
        var queryExpression = new MarkdownQueryExpression(entityType);

        // Act & Assert
        Assert.Equal(entityType, queryExpression.EntityType);
        Assert.NotNull(queryExpression.ServerQueryExpression);
        Assert.Equal(typeof(IEnumerable<ValueBuffer>), queryExpression.Type);
        Assert.Equal(ExpressionType.Extension, queryExpression.NodeType);
        Assert.NotNull(queryExpression.GetProjectionMapping());
    }

    [Fact]
    public void MarkdownQueryExpression_Print_WritesToPrinter()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();
        
        var entityType = context.Model.FindEntityType(typeof(TestEntity))!;
        var queryExpression = new MarkdownQueryExpression(entityType);
        var printable = (IPrintableExpression)queryExpression;

        // Act - Create a simple expression printer and print
        // The Print method just appends to the printer, so we verify it doesn't throw
        var exception = Record.Exception(() =>
        {
            // We can't easily test the output, but we can verify it doesn't throw
            using var innerContext = _fixture.CreateContext(_testDir);
            var printer = new TestExpressionPrinter();
            printable.Print(printer);
        });

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void MarkdownTableExpression_Properties_ReturnExpectedValues()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();
        
        var entityType = context.Model.FindEntityType(typeof(TestEntity))!;
        var tableExpression = new MarkdownTableExpression(entityType);

        // Act & Assert
        Assert.Equal(entityType, tableExpression.EntityType);
        Assert.Equal(typeof(IEnumerable<ValueBuffer>), tableExpression.Type);
        Assert.Equal(ExpressionType.Extension, tableExpression.NodeType);
    }

    [Fact]
    public void MarkdownTableExpression_Print_WritesToPrinter()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();
        
        var entityType = context.Model.FindEntityType(typeof(TestEntity))!;
        var tableExpression = new MarkdownTableExpression(entityType);
        var printable = (IPrintableExpression)tableExpression;

        // Act
        var exception = Record.Exception(() =>
        {
            var printer = new TestExpressionPrinter();
            printable.Print(printer);
        });

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void MarkdownEntityProjectionExpression_Properties_ReturnExpectedValues()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();
        
        var entityType = context.Model.FindEntityType(typeof(TestEntity))!;
        var tableExpression = new MarkdownTableExpression(entityType);
        var projectionExpression = new MarkdownEntityProjectionExpression(entityType, tableExpression);

        // Act & Assert
        Assert.Equal(entityType, projectionExpression.EntityType);
        Assert.Equal(tableExpression, projectionExpression.AccessExpression);
        Assert.Equal(typeof(TestEntity), projectionExpression.Type);
        Assert.Equal(ExpressionType.Extension, projectionExpression.NodeType);
    }

    [Fact]
    public void MarkdownEntityProjectionExpression_GetPropertyIndex_ReturnsCorrectIndex()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();
        
        var entityType = context.Model.FindEntityType(typeof(TestEntity))!;
        var tableExpression = new MarkdownTableExpression(entityType);
        var projectionExpression = new MarkdownEntityProjectionExpression(entityType, tableExpression);
        
        var properties = entityType.GetProperties().ToList();

        // Act & Assert
        for (var i = 0; i < properties.Count; i++)
        {
            var index = projectionExpression.GetPropertyIndex(properties[i]);
            Assert.Equal(i, index);
        }
    }

    [Fact]
    public void MarkdownEntityProjectionExpression_GetPropertyIndex_ReturnsMinusOneForUnknownProperty()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var testEntityType = context.Model.FindEntityType(typeof(TestEntity))!;
        var longKeyEntityType = context.Model.FindEntityType(typeof(TestEntityWithLongKey))!;

        var tableExpression = new MarkdownTableExpression(testEntityType);
        var projectionExpression = new MarkdownEntityProjectionExpression(testEntityType, tableExpression);

        // Get a property from a different entity type
        var foreignProperty = longKeyEntityType.GetProperties().First();

        // Act
        var index = projectionExpression.GetPropertyIndex(foreignProperty);

        // Assert
        Assert.Equal(-1, index);
    }

    [Fact]
    public void MarkdownEntityProjectionExpression_Print_WritesToPrinter()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var entityType = context.Model.FindEntityType(typeof(TestEntity))!;
        var tableExpression = new MarkdownTableExpression(entityType);
        var projectionExpression = new MarkdownEntityProjectionExpression(entityType, tableExpression);
        var printable = (IPrintableExpression)projectionExpression;

        // Act
        var exception = Record.Exception(() =>
        {
            var printer = new TestExpressionPrinter();
            printable.Print(printer);
        });

        // Assert
        Assert.Null(exception);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            try { Directory.Delete(_testDir, true); } catch { }
        }
    }
}

