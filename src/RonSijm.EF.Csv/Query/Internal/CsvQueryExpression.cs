// Licensed under the MIT license.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace RonSijm.EF.Csv.Query.Internal;

/// <summary>
/// Query expression for the CSV provider.
/// </summary>
public class CsvQueryExpression : Expression, IPrintableExpression
{
    /// <summary>
    /// Creates a new instance of <see cref="CsvQueryExpression"/>.
    /// </summary>
    public CsvQueryExpression(IEntityType entityType)
    {
        EntityType = entityType;
    }

    /// <summary>
    /// Gets the entity type being queried.
    /// </summary>
    public virtual IEntityType EntityType { get; }

    /// <inheritdoc />
    public override Type Type => typeof(IEnumerable<>).MakeGenericType(EntityType.ClrType);

    /// <inheritdoc />
    public override ExpressionType NodeType => ExpressionType.Extension;

    /// <inheritdoc />
    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append($"CsvQueryExpression: {EntityType.DisplayName()}");
    }
}

