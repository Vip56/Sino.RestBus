using System;
using System.Linq;
using System.Reflection;

namespace Sino.RestBus.Client.Http.Internal
{
    internal static class TypeExtensions
    {
        private static bool EqualTo(this Type[] t1, Type[] t2)
        {
            if (t1.Length != t2.Length)
            {
                return false;
            }

            for (int idx = 0; idx < t1.Length; ++idx)
            {
                if (t1[idx] != t2[idx])
                {
                    return false;
                }
            }

            return true;
        }

        public static ConstructorInfo GetConstructor(this Type type, Type[] types)
        {
            return type.GetTypeInfo().DeclaredConstructors
                                     .Where(c => c.IsPublic)
                                     .SingleOrDefault(c => c.GetParameters()
                                                            .Select(p => p.ParameterType).ToArray().EqualTo(types));
        }

        public static Type ExtractGenericInterface(this Type queryType, Type interfaceType)
        {
            Func<Type, bool> matchesInterface = t => t.IsGenericType() && t.GetGenericTypeDefinition() == interfaceType;
            return (matchesInterface(queryType)) ? queryType : queryType.GetInterfaces().FirstOrDefault(matchesInterface);
        }

        public static Type[] GetGenericArguments(this Type type)
        {
            return type.GetTypeInfo().GenericTypeArguments;
        }

        public static Type[] GetInterfaces(this Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces.ToArray();
        }

        public static bool IsAssignableFrom(this Type type, Type c)
        {
            return type.GetTypeInfo().IsAssignableFrom(c.GetTypeInfo());
        }

        public static bool IsGenericType(this Type type)
        {
            return type.IsGenericType();
        }

        public static bool IsInterface(this Type type)
        {
            return type.IsInterface();
        }

        public static bool IsValueType(this Type type)
        {
            return type.IsValueType();
        }
    }
}
