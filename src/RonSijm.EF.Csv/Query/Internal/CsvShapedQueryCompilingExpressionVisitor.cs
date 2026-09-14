// Licensed under the MIT license.

using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using RonSijm.EF.Csv.Storage.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace RonSijm.EF.Csv.Query.Internal;

/// <summary>
/// Expression visitor for compiling shaped queries for the CSV provider.
/// </summary>
public class CsvShapedQueryCompilingExpressionVisitor : ShapedQueryCompilingExpressionVisitor
{
    private readonly Type _contextType;
    private readonly bool _threadSafetyChecksEnabled;

    // Cache for compiled property setters - significantly faster than reflection
    private static readonly ConcurrentDictionary<Type, EntityMaterializer> _materializerCache = new();

    /// <summary>
    /// Creates a new instance of <see cref="CsvShapedQueryCompilingExpressionVisitor"/>.
    /// </summary>
    public CsvShapedQueryCompilingExpressionVisitor(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies,
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, queryCompilationContext)
    {
        _contextType = queryCompilationContext.ContextType;
        _threadSafetyChecksEnabled = dependencies.CoreSingletonOptions.AreThreadSafetyChecksEnabled;
    }

    /// <inheritdoc />
    protected override Expression VisitShapedQuery(ShapedQueryExpression shapedQueryExpression)
    {
        var queryExpression = shapedQueryExpression.QueryExpression;

        if (queryExpression is CsvQueryExpression csvQueryExpression)
        {
            var entityType = csvQueryExpression.EntityType;
            var enumerableExpression = CreateEnumerableExpression(entityType, shapedQueryExpression.ShaperExpression);
            return enumerableExpression;
        }

        throw new InvalidOperationException($"Unknown query expression type: {queryExpression.GetType().Name}");
    }

    private Expression CreateEnumerableExpression(IEntityType entityType, Expression shaperExpression)
    {
        var queryMethod = typeof(CsvShapedQueryCompilingExpressionVisitor)
            .GetMethod(nameof(QueryEntities), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(entityType.ClrType);

        var queryContextParameter = QueryCompilationContext.QueryContextParameter;
        var entityTypeConstant = Expression.Constant(entityType);

        return Expression.Call(
            queryMethod,
            queryContextParameter,
            entityTypeConstant);
    }

    private static IEnumerable<TEntity> QueryEntities<TEntity>(QueryContext queryContext, IEntityType entityType)
        where TEntity : class
    {
        var database = queryContext.Context.GetService<ICsvDatabase>();
        var store = database.Store;
        var table = store.GetTable(entityType);

        var materializer = _materializerCache.GetOrAdd(typeof(TEntity), _ => CreateMaterializer<TEntity>(entityType));

        foreach (var row in table.Rows)
        {
            yield return materializer.Materialize<TEntity>(row);
        }
    }

    private static EntityMaterializer CreateMaterializer<TEntity>(IEntityType entityType)
        where TEntity : class
    {
        var properties = entityType.GetProperties().ToArray();
        var setters = new Action<object, object?>[properties.Length];

        for (var i = 0; i < properties.Length; i++)
        {
            var property = properties[i];
            var propertyInfo = property.PropertyInfo;

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                setters[i] = CreatePropertySetter(typeof(TEntity), propertyInfo);
            }
        }

        return new EntityMaterializer(typeof(TEntity), setters);
    }

    private static Action<object, object?> CreatePropertySetter(Type entityType, PropertyInfo propertyInfo)
    {
        var instanceParam = Expression.Parameter(typeof(object), "instance");
        var valueParam = Expression.Parameter(typeof(object), "value");

        var castInstance = Expression.Convert(instanceParam, entityType);
        var castValue = Expression.Convert(valueParam, propertyInfo.PropertyType);

        var propertyAccess = Expression.Property(castInstance, propertyInfo);
        var assign = Expression.Assign(propertyAccess, castValue);

        var lambda = Expression.Lambda<Action<object, object?>>(assign, instanceParam, valueParam);
        return lambda.Compile();
    }

    private sealed class EntityMaterializer
    {
        private readonly Func<object> _constructor;
        private readonly Action<object, object?>[] _setters;

        public EntityMaterializer(Type entityType, Action<object, object?>[] setters)
        {
            _setters = setters;

            var newExpr = Expression.New(entityType);
            var castExpr = Expression.Convert(newExpr, typeof(object));
            _constructor = Expression.Lambda<Func<object>>(castExpr).Compile();
        }

        public TEntity Materialize<TEntity>(object[] row) where TEntity : class
        {
            var entity = (TEntity)_constructor();

            for (var i = 0; i < _setters.Length; i++)
            {
                var setter = _setters[i];
                var value = row[i];

                if (setter != null && value != null)
                {
                    setter(entity, value);
                }
            }

            return entity;
        }
    }
}

