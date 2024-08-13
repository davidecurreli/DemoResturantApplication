using Domain;
using System.Reflection;

namespace Infrastructure.Helpers;

public static class GetTypeHelper
{
    public static Type[] GetTypesFromNamespace(string nameSpace)
    {
        var assembly = Assembly.LoadFrom(Assembly.GetAssembly(typeof(BaseEntity))?.Location ?? "");

        return
          assembly.GetTypes()
          .Where(t => t.Namespace is not null && t.Namespace.Contains(nameSpace) && !t.Name.Contains("BaseEntity") && !t.IsEnum)
          .ToArray();
    }
}
