using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Haondt.Identity.StorageKey
{
    public class SimpleTypeConverter
    {
        private static readonly Dictionary<string, Assembly> _assemblyCache = [];

        private static Assembly LoadAssembly(string assemblyName)
        {
            if (!_assemblyCache.TryGetValue(assemblyName, out var assembly))
            {
                assembly = Assembly.Load(assemblyName)
                    ?? throw new InvalidOperationException($"Assembly '{assemblyName}' could not be loaded.");
                _assemblyCache[assemblyName] = assembly;
            }
            return assembly;
        }


        public static string TypeToString(Type type)
        {
            if (type.IsGenericType)
            {
                if (type.ContainsGenericParameters)
                    return $"{GetBaseTypeName(type)}<{string.Join("", Enumerable.Range(0, type.GetGenericArguments().Length - 1).Select(a => '|'))}>";

                var genericTypeDefinition = type.GetGenericTypeDefinition();
                var genericArguments = type.GetGenericArguments();

                var genericArgsString = string.Join('|', genericArguments.Select(TypeToString));
                return $"{GetBaseTypeName(genericTypeDefinition)}<{genericArgsString}>";
            }

            return GetBaseTypeName(type);
        }

        private static string GetBaseTypeName(Type type)
        {
            var assemblyName = type.Assembly.GetName().Name;
            var stringBuilder = new StringBuilder();
            if (type.Namespace != null && type.Namespace.Length > 0)
                stringBuilder.Append($"{type.Namespace}.");
            var backtickIndex = type.Name.IndexOf('`');
            if (backtickIndex > 0)
                stringBuilder.Append(type.Name.Substring(0, backtickIndex));
            else
                stringBuilder.Append(type.Name);
            stringBuilder.Append(", ");
            stringBuilder.Append(assemblyName);

            return stringBuilder.ToString();
        }

        public static Type StringToType(string typeString)
        {
            if (typeString.EndsWith("<>"))
                return GetTypeFromString(typeString[..^2], 1);
            else if (typeString.EndsWith("|>"))
            {
                var parts = typeString.Split('<');
                return GetTypeFromString(parts[0], parts[1].Length);
            }
            var tree = BuildTypeTree(typeString);
            return CollapseTree(tree);
        }

        private class TypeNode
        {
            public string? Name { get; set; }
            public List<TypeNode> Arguments { get; set; } = [];
            public TypeNode? Parent { get; set; } = null;
        }

        private static TypeNode BuildTypeTree(string typeString)
        {
            var current = new TypeNode();
            var root = current;

            var start = 0;
            var next = 0;
            while (true)
            {
                if (next == typeString.Length)
                {
                    if (start < typeString.Length)
                        root.Name ??= typeString[start..next];
                    break;
                }
                switch (typeString[next])
                {
                    case '<':
                    {
                        current.Name ??= typeString[start..next];
                        var parent = current;
                        current = new TypeNode
                        {
                            Parent = current
                        };
                        parent.Arguments.Add(current);
                        start = next + 1;
                        break;
                    }
                    case '|':
                    {
                        current.Name ??= typeString[start..next];
                        var parent = current.Parent;
                        current = new TypeNode
                        {
                            Parent = parent
                        };
                        parent?.Arguments.Add(current);
                        start = next + 1;
                        break;
                    }
                    case '>':
                    {
                        current.Name ??= typeString[start..next];
                        current = current.Parent
                            ?? throw new ArgumentException($"Failed to parse type string at position {next}");
                        start = next + 1;
                        break;
                    }
                }
                next++;
            }
            return root;
        }

        private static Type CollapseTree(TypeNode root)
        {
            if (string.IsNullOrEmpty(root.Name))
                throw new ArgumentException("root node name cannot be null");
            var type = GetTypeFromString(root.Name, root.Arguments.Count);
            if(root.Arguments.Count > 0)
            {
                var argumentTypes = root.Arguments.Select(CollapseTree).ToArray();
                return type.MakeGenericType(argumentTypes);
            }
            return type;
        }

        private static Type GetTypeFromString(string typeString, int numArgs)
        {
            var parts = typeString.Split(", ");
            if (parts.Length != 2)
                throw new InvalidOperationException($"Invalid type string format: '{typeString}'");

            var typeName = parts[0];
            if (numArgs > 0)
                typeName = $"{typeName}`{numArgs}";
            var assemblyName = parts[1];

            var assembly = LoadAssembly(assemblyName);

            var type = assembly.GetType(typeName)
                ?? throw new InvalidOperationException($"Type '{typeName}' could not be found in assembly '{assemblyName}'.");

            return type;
        }
    }
}
