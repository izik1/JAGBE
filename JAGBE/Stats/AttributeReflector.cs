using System;
using System.Collections.Generic;
using System.Reflection;
using JAGBE.Attributes;

namespace JAGBE.Stats
{
    internal static class AttributeReflector
    {
        /// <summary>
        /// Gets the methods of <paramref name="type"/> with the <see cref="Attribute"/> of type
        /// <paramref name="attribute"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="parentList">The parent list.</param>
        public static void GetMethodsOfTypeWithAttribute(Type type, Type attribute, ICollection<string> parentList)
        {
            foreach (MethodInfo info in type.GetMethods(
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public))
            {
                Attribute a = info.GetCustomAttribute(attribute);
                if (a != null)
                {
                    parentList.Add(type.FullName + "." + info.Name);
                }
            }
        }

        /// <summary>
        /// Writes all method stubs to the console.
        /// </summary>
        public static List<string> GetAllStubs()
        {
            // Reflection doesn't seem to grab this method.
            List<string> strs = new List<string>(0);
            foreach (Type t in typeof(AttributeReflector).Assembly.GetTypes())
            {
                GetMethodsOfTypeWithAttribute(t, typeof(StubAttribute), strs);
            }

            return strs;
        }
    }
}
