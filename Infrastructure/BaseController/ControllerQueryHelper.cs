using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Reflection;
using Microsoft.AspNetCore.OData.Query;

namespace Infrastructure.BaseController;

public static class ControllerQueryHelper
{
    private static readonly AssemblyName _assemblyName = new() { Name = "DynamicLinqTypes" };
    private static readonly ModuleBuilder _moduleBuilder;
    private static readonly Dictionary<string, Type> _builtTypes = [];

    static ControllerQueryHelper()
    {
        _moduleBuilder = AssemblyBuilder.DefineDynamicAssembly(_assemblyName, AssemblyBuilderAccess.Run).DefineDynamicModule(_assemblyName.Name);
    }

    public static Type CreateDynamicType(Dictionary<string, Type>? fields)
    {
        ArgumentNullException.ThrowIfNull(fields);

        if (fields.Count == 0)
            throw new ArgumentOutOfRangeException(nameof(fields), "fields must have at least 1 field definition");

        string className = $"anonymous_{GetTypeKey(fields).GetHashCode()}";

        if (_builtTypes.TryGetValue(className, out Type? value))
            return value;

        TypeBuilder typeBuilder = _moduleBuilder.DefineType(className, TypeAttributes.Public | TypeAttributes.Class);

        foreach (var field in fields)
            typeBuilder.DefineField(field.Key, field.Value, FieldAttributes.Public);

        _builtTypes[className] = typeBuilder.CreateType();

        return _builtTypes[className];
    }

    private static string GetTypeKey(Dictionary<string, Type> fields)
    {
        string key = string.Empty;
        foreach (var field in fields)
            key += field.Key + ";" + field.Value.Name + ";";

        return key;
    }

    public static Type GetDynamicType(IEnumerable<PropertyInfo?> fields)
    {
        var keyValuePairs = new Dictionary<string, Type>();
        foreach (var field in fields)
        {
            if (field is null)
                throw new Exception();

            keyValuePairs.Add(field.Name, field.PropertyType);
        }
        return CreateDynamicType(keyValuePairs);
    }

    public static T? GetFieldValue<T>(object obj, string fieldName)
    {
        ArgumentNullException.ThrowIfNull(obj);

        var field = obj.GetType().GetField(fieldName, BindingFlags.IgnoreCase |
                                                      BindingFlags.Public |
                                                      BindingFlags.NonPublic |
                                                      BindingFlags.Instance)
            ?? throw new ArgumentException("No such field was found.", nameof(fieldName));

        if (!typeof(T).IsAssignableFrom(field.FieldType))
            throw new InvalidOperationException("Field type and requested type are not compatible.");

        return (T?)field.GetValue(obj);
    }

    public static Expression<Func<T, bool>>? GetFilterExpression<T>(FilterQueryOption filter)
    {
        var enumerable = Enumerable.Empty<T>().AsQueryable();
        var param = Expression.Parameter(typeof(T));

        if (filter is not null)
        {
            enumerable = (IQueryable<T>)filter.ApplyTo(enumerable, new ODataQuerySettings());

            if (enumerable.Expression is MethodCallExpression mce)
                if (mce.Arguments[1] is UnaryExpression quote)
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
        var sourceProperties = new Dictionary<string, PropertyInfo?>();

        foreach (var column in columns)
            sourceProperties.Add(column.ToLower(), source.ElementType.GetProperty(column, BindingFlags.IgnoreCase |
                                                                                          BindingFlags.Public |
                                                                                          BindingFlags.Instance));
        var dynamicType = GetDynamicType(sourceProperties.Values);

        ParameterExpression sourceItem = Expression.Parameter(source.ElementType, "t");
        IEnumerable<MemberBinding> bindings = dynamicType.GetFields()
            .Select(
                p => Expression.Bind(p, Expression.Property(sourceItem, sourceProperties[p.Name.ToLower()] ?? throw new Exception()))
            ).OfType<MemberBinding>();

        var xInit = Expression.MemberInit(Expression.New(dynamicType.GetConstructor(Type.EmptyTypes) ?? throw new Exception()), bindings);

        var lambda = Expression.Lambda<Func<T, T1>>(xInit, sourceItem);

        return lambda.Compile();
    }
}

