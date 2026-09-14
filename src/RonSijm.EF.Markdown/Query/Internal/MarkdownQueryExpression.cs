// Licensed under the MIT license.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace RonSijm.EF.Markdown.Query.Internal;

/// <summary>
/// Represents a query expression for the Markdown provider.
/// </summary>
public class MarkdownQueryExpression : Expression, IPrintableExpression
{
    private readonly Dictionary<ProjectionMember, Expression> _projectionMapping = new();

    /// <summary>
    /// Creates a new instance of <see cref="MarkdownQueryExpression"/>.
    /// </summary>
    public MarkdownQueryExpression(IEntityType entityType)
    {
        EntityType = entityType;
        ServerQueryExpression = new MarkdownTableExpression(entityType);
        _projectionMapping[new ProjectionMember()] = new MarkdownEntityProjectionExpression(entityType, ServerQueryExpression);
    }

    /// <summary>
    /// Gets the entity type for this query.
    /// </summary>
    public virtual IEntityType EntityType { get; }

    /// <summary>
    /// Gets the server query expression.
    /// </summary>
    public virtual Expression ServerQueryExpression { get; }

    /// <inheritdoc />
    public override Type Type => typeof(IEnumerable<ValueBuffer>);

    /// <inheritdoc />
    public sealed override ExpressionType NodeType => ExpressionType.Extension;

    /// <summary>
    /// Gets the projection mapping.
    /// </summary>
    public virtual IReadOnlyDictionary<ProjectionMember, Expression> GetProjectionMapping()
        => _projectionMapping;

    /// <inheritdoc />
    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append($"MarkdownQueryExpression: {EntityType.DisplayName()}");
    }
}

/// <summary>
/// Represents a table expression for the Markdown provider.
/// </summary>
public class MarkdownTableExpression : Expression, IPrintableExpression
{
    /// <summary>
    /// Creates a new instance of <see cref="MarkdownTableExpression"/>.
    /// </summary>
    public MarkdownTableExpression(IEntityType entityType)
    {
        EntityType = entityType;
    }

    /// <summary>
    /// Gets the entity type.
    /// </summary>
    public virtual IEntityType EntityType { get; }

    /// <inheritdoc />
    public override Type Type => typeof(IEnumerable<ValueBuffer>);

    /// <inheritdoc />
    public sealed override ExpressionType NodeType => ExpressionType.Extension;

    /// <inheritdoc />
    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append($"MarkdownTable: {EntityType.DisplayName()}");
    }
}

/// <summary>
/// Represents an entity projection expression for Markdown provider.
/// </summary>
public class MarkdownEntityProjectionExpression : Expression, IPrintableExpression
{
    private readonly IReadOnlyDictionary<IProperty, int> _propertyIndexMap;

    /// <summary>
    /// Creates a new instance of <see cref="MarkdownEntityProjectionExpression"/>.
    /// </summary>
    public MarkdownEntityProjectionExpression(IEntityType entityType, Expression accessExpression)
    {
        EntityType = entityType;
        AccessExpression = accessExpression;

        var properties = entityType.GetProperties().ToList();
        var indexMap = new Dictionary<IProperty, int>();
        for (var i = 0; i < properties.Count; i++)
        {
            indexMap[properties[i]] = i;
        }
        _propertyIndexMap = indexMap;
    }

    /// <summary>
    /// Gets the entity type.
    /// </summary>
    public virtual IEntityType EntityType { get; }

    /// <summary>
    /// Gets the access expression.
    /// </summary>
    public virtual Expression AccessExpression { get; }

    /// <inheritdoc />
    public override Type Type => EntityType.ClrType;

    /// <inheritdoc />
    public sealed override ExpressionType NodeType => ExpressionType.Extension;

    /// <summary>
    /// Gets the index for a property.
    /// </summary>
    public virtual int GetPropertyIndex(IProperty property)
        => _propertyIndexMap.TryGetValue(property, out var index) ? index : -1;

    /// <inheritdoc />
    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append($"EntityProjection: {EntityType.DisplayName()}");
    }
}

