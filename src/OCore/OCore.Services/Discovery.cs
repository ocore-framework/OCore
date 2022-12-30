using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Services
{
    public static class Discovery
    {
        // I stole this implementation from here:
        //   https://stackoverflow.com/questions/851248/c-sharp-reflection-get-all-active-assemblies-in-a-solution
        // Pretty old post, but it does get OCore system-assemblies correctly it seems.

        // It comes with the potential downside that it forces load of assemblies
        private static IEnumerable<Assembly> GetAssemblies()
        {
            var list = new List<string>();
            var stack = new Stack<Assembly>();

            stack.Push(Assembly.GetEntryAssembly());

            do
            {
                var asm = stack.Pop();

                yield return asm;

                foreach (var reference in asm.GetReferencedAssemblies())
                    if (!list.Contains(reference.FullName))
                    {
                        stack.Push(Assembly.Load(reference));
                        list.Add(reference.FullName);
                    }

            }
            while (stack.Count > 0);
        }

        private static IEnumerable<Type> GetAllTypesThatHaveAttribute<T>(bool deep = true) where T : Attribute
        {
            var assemblies = deep == true ? GetAssemblies() : AppDomain
                .CurrentDomain
                .GetAssemblies();

            return assemblies
                .SelectMany(x => x.GetTypes())
                .Where(type => type.GetCustomAttribute<GeneratedCodeAttribute>() == null
                               && type.GetCustomAttributes(true).Where(z => z is ServiceAttribute).Any());
        }

        public static IEnumerable<Type> GetAll(bool deep = true)
        {
            return GetAllTypesThatHaveAttribute<ServiceAttribute>(deep);
        }
    }
}
