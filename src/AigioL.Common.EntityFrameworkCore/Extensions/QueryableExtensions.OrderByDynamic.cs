using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace AigioL.Common.EntityFrameworkCore.Extensions;

static partial class QueryableExtensions
{
    [RequiresDynamicCode("Delegate creation requires dynamic code generation.")]
    public static IQueryable<T> OrderByPropertyName<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        this IQueryable<T> source,
        string propertyName,
        bool? desc,
        bool throwPropertyIsNull = true)
    {
        var command = (desc ?? false) ? "OrderByDescending" : "OrderBy";
        var type = typeof(T);
        var property = type.GetProperty(propertyName);
        if (property == null)
        {
            if (throwPropertyIsNull)
            {
                throw new ArgumentException(
                    $"Property '{propertyName}' not found on type '{type.Name}'");
            }
            return source;
        }
        var parameter = Expression.Parameter(type, "p");
        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);
        var resultExpression = Expression.Call(typeof(Queryable), command, [type, property.PropertyType], source.Expression, Expression.Quote(orderByExpression));
        return source.Provider.CreateQuery<T>(resultExpression);
    }
}
