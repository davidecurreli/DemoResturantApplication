using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.AspNetCore.OData.Query;

namespace Infrastructure.BaseController;

public static class ControllerQueryHelper
{
    private static readonly AssemblyName AssemblyName = new("DynamicLinqTypes");
    private static readonly ModuleBuilder ModuleBuilder;
    private static readonly Dictionary<string, Type> BuiltTypes = [];

    static ControllerQueryHelper()
    {
        ModuleBuilder = AssemblyBuilder.DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.Run).DefineDynamicModule(AssemblyName.Name ?? "");
    }

    public static Type CreateDynamicType(Dictionary<string, Type> fields)
    {
        ArgumentNullException.ThrowIfNull(fields);
        if (fields.Count == 0)
            throw new ArgumentException("Fields must have at least 1 field definition", nameof(fields));

        string className = $"anonymous_{GetTypeKey(fields).GetHashCode()}";

        if (BuiltTypes.TryGetValue(className, out Type? value))
            return value;

        TypeBuilder typeBuilder = ModuleBuilder.DefineType(className, TypeAttributes.Public | TypeAttributes.Class);

        foreach (var (fieldName, fieldType) in fields)
            typeBuilder.DefineField(fieldName, fieldType, FieldAttributes.Public);

        BuiltTypes[className] = typeBuilder.CreateType();
        return BuiltTypes[className];
    }

    public static Type GetDynamicType(IEnumerable<PropertyInfo>? fields)
    {
        ArgumentNullException.ThrowIfNull(fields);

        var keyValuePairs = fields.ToDictionary(
            field => field.Name,
            field => field.PropertyType
        );
        return CreateDynamicType(keyValuePairs);
    }

    public static T? GetFieldValue<T>(object obj, string fieldName)
    {
        ArgumentNullException.ThrowIfNull(obj);

        var field = obj.GetType().GetField(fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new ArgumentException("No such field was found.", nameof(fieldName));

        if (!typeof(T).IsAssignableFrom(field.FieldType))
            throw new InvalidOperationException("Field type and requested type are not compatible.");

        return (T?)field.GetValue(obj);
    }

    public static Expression<Func<T, bool>>? GetFilterExpression<T>(FilterQueryOption filter)
    {
        var enumerable = Enumerable.Empty<T>().AsQueryable();
        var param = Expression.Parameter(typeof(T));

        if (filter != null)
        {
            enumerable = (IQueryable<T>)filter.ApplyTo(enumerable, new ODataQuerySettings());

            if (enumerable.Expression is MethodCallExpression mce && mce.Arguments[1] is UnaryExpression quote)
                return quote.Operand as Expression<Func<T, bool>>;
        }
        return Expression.Lambda<Func<T, bool>>(Expression.Constant(true), param);
    }

    public static Expression<Func<T, object>> GetOrderByExpression<T>(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(T));
        var property = Expression.Property(parameter, propertyName);
        var propAsObject = Expression.Convert(property, typeof(object));

        return Expression.Lambda<Func<T, object>>(propAsObject, parameter);
    }

    public static Func<T, T1> BuildSelectClause<T1, T>(List<string> columns, IQueryable<T> source)
    {
        var sourceProperties = columns.ToDictionary(
            column => column.ToLower(),
            column => source.ElementType.GetProperty(column, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
        );

        var dynamicType = GetDynamicType(sourceProperties.Values);

        ParameterExpression sourceItem = Expression.Parameter(source.ElementType, "t");
        var bindings = dynamicType.GetFields()
            .Select(p => Expression.Bind(p, Expression.Property(sourceItem, sourceProperties[p.Name.ToLower()])))
            .OfType<MemberBinding>();

        var xInit = Expression.MemberInit(Expression.New(dynamicType.GetConstructor(Type.EmptyTypes)), bindings);

        var lambda = Expression.Lambda<Func<T, T1>>(xInit, sourceItem);

        return lambda.Compile();
    }

    private static string GetTypeKey(Dictionary<string, Type> fields)
    {
        return string.Join(";", fields.Select(kvp => $"{kvp.Key};{kvp.Value.Name}"));
    }
}