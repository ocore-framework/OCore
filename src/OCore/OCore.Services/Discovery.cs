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
        private static IEnumerable<Type> GetAllTypesThatHaveAttribute<T>() where T : Attribute
        {
            return AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(type => type.GetCustomAttribute<GeneratedCodeAttribute>() == null
                               && type.GetCustomAttributes(true).Where(z => z is ServiceAttribute).Any());
        }

        public static IEnumerable<Type> GetAll()
        {
            return GetAllTypesThatHaveAttribute<ServiceAttribute>();
        }
    }
}
