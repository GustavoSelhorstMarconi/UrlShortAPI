using System.Linq.Expressions;
using System.Reflection;

namespace UrlShort.Application.Mappers;

public static class GenericMapper<TEntity, TDto>
{
    private static readonly Func<TEntity, TDto> _entityToDto;
    private static readonly Func<TDto, TEntity> _dtoToEntity;
    private static readonly Dictionary<string, PropertyMapping> _propertyMappings;

    static GenericMapper()
    {
        _propertyMappings = BuildPropertyMappings();
        _entityToDto = CreateMapper<TEntity, TDto>();
        _dtoToEntity = CreateMapper<TDto, TEntity>();
    }

    public static TDto? ToDto(TEntity entity)
    {
        if (entity == null) return default;
        return _entityToDto(entity);
    }

    public static TEntity? FromDto(TDto dto)
    {
        if (dto == null) return default;
        return _dtoToEntity(dto);
    }

    public static IEnumerable<TDto?> ToDto(IEnumerable<TEntity>? entities)
    {
        if (entities == null) return Enumerable.Empty<TDto>();
        return entities.Select(ToDto);
    }

    public static List<TEntity> FromDto(IEnumerable<TDto>? dtos)
    {
        if (dtos == null) return new List<TEntity>();
        return dtos.Select(FromDto).ToList();
    }

    public static IQueryable<TDto> ProjectToDto(IQueryable<TEntity> query)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        var memberInit = CreateMemberInitExpression<TEntity, TDto>(parameter);
        var lambda = Expression.Lambda<Func<TEntity, TDto>>(memberInit, parameter);
        return query.Select(lambda);
    }

    private static Dictionary<string, PropertyMapping> BuildPropertyMappings()
    {
        var sourceProperties = typeof(TEntity).GetProperties();
        var targetProperties = typeof(TDto).GetProperties();
        var mappings = new Dictionary<string, PropertyMapping>();

        foreach (var targetProperty in targetProperties)
        {
            var sourceProperty = sourceProperties.FirstOrDefault(p =>
                p.Name == targetProperty.Name &&
                (p.PropertyType == targetProperty.PropertyType ||
                 IsAssignableOrConvertible(p.PropertyType, targetProperty.PropertyType)));

            if (sourceProperty != null)
                mappings[targetProperty.Name] = new PropertyMapping
                {
                    SourceProperty = sourceProperty,
                    TargetProperty = targetProperty,
                    RequiresConversion = sourceProperty.PropertyType != targetProperty.PropertyType
                };
        }

        return mappings;
    }

    private static bool IsAssignableOrConvertible(Type sourceType, Type targetType)
    {
        if (targetType.IsAssignableFrom(sourceType))
            return true;

        if ((sourceType.IsEnum && targetType == typeof(int)) ||
            (targetType.IsEnum && sourceType == typeof(int)) ||
            (sourceType == typeof(int) && targetType == typeof(string)) ||
            (sourceType == typeof(string) && targetType == typeof(int)) ||
            (sourceType == typeof(DateTime) && targetType == typeof(string)) ||
            (sourceType == typeof(string) && targetType == typeof(DateTime)))
            return true;

        return false;
    }

    private static Func<TSource, TTarget> CreateMapper<TSource, TTarget>()
    {
        var parameter = Expression.Parameter(typeof(TSource), "source");
        var memberInit = CreateMemberInitExpression<TSource, TTarget>(parameter);
        var lambda = Expression.Lambda<Func<TSource, TTarget>>(memberInit, parameter);
        return lambda.Compile();
    }

    private static MemberInitExpression CreateMemberInitExpression<TSource, TTarget>(ParameterExpression parameter)
    {
        var sourceProperties = typeof(TSource).GetProperties();
        var targetProperties = typeof(TTarget).GetProperties();
        var newTarget = Expression.New(typeof(TTarget));
        var bindings = new List<MemberBinding>();

        foreach (var targetProperty in targetProperties)
        {
            var sourceProperty = sourceProperties.FirstOrDefault(p =>
                p.Name == targetProperty.Name &&
                (p.PropertyType == targetProperty.PropertyType ||
                 IsAssignableOrConvertible(p.PropertyType, targetProperty.PropertyType)));

            if (sourceProperty != null)
            {
                var sourcePropertyAccess = Expression.Property(parameter, sourceProperty);
                Expression valueExpression = sourcePropertyAccess;

                if (sourceProperty.PropertyType != targetProperty.PropertyType)
                {
                    if (targetProperty.PropertyType == typeof(string))
                    {
                        var toStringMethod = typeof(object).GetMethod("ToString");
                        if (toStringMethod != null)
                            valueExpression = Expression.Call(sourcePropertyAccess, toStringMethod);
                    }
                    else if (sourceProperty.PropertyType == typeof(string) &&
                             targetProperty.PropertyType == typeof(int))
                    {
                        var parseMethod = typeof(int).GetMethod("Parse", new[] { typeof(string) });
                        if (parseMethod != null)
                            valueExpression = Expression.Call(parseMethod, sourcePropertyAccess);
                    }
                    else if (sourceProperty.PropertyType == typeof(string) &&
                             targetProperty.PropertyType == typeof(DateTime))
                    {
                        var parseMethod = typeof(DateTime).GetMethod("Parse", new[] { typeof(string) });
                        if (parseMethod != null)
                            valueExpression = Expression.Call(parseMethod, sourcePropertyAccess);
                    }
                    else if (sourceProperty.PropertyType == typeof(DateTime) &&
                             targetProperty.PropertyType == typeof(string))
                    {
                        var toStringMethod = typeof(DateTime).GetMethod("ToString", Type.EmptyTypes);
                        if (toStringMethod != null)
                            valueExpression = Expression.Call(sourcePropertyAccess, toStringMethod);
                    }
                    else
                    {
                        valueExpression = Expression.Convert(sourcePropertyAccess, targetProperty.PropertyType);
                    }
                }

                if (!sourceProperty.PropertyType.IsValueType)
                {
                    var nullCheck = Expression.Equal(sourcePropertyAccess, Expression.Constant(null));
                    var defaultValue = Expression.Default(targetProperty.PropertyType);
                    valueExpression = Expression.Condition(nullCheck, defaultValue, valueExpression);
                }

                var binding = Expression.Bind(targetProperty, valueExpression);
                bindings.Add(binding);
            }
        }

        return Expression.MemberInit(newTarget, bindings);
    }

    private class PropertyMapping
    {
        public PropertyInfo SourceProperty { get; set; }
        public PropertyInfo TargetProperty { get; set; }
        public bool RequiresConversion { get; set; }
    }
}