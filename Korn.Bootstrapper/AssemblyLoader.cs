using System;
using System.Collections.Generic;
using System.Reflection;

namespace Korn.Bootstrapper
{
    public class AssemblyLoader
    {
        public AssemblyLoader()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            AddAlreadyLoaded();
        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var hash = args.Name.GetHashCode();
            HashedAssemblies.TryGetValue(hash, out Assembly result);
            return result;
        }

        public readonly List<Assembly> UserLoadedAssemblies = new List<Assembly>();
        public readonly Dictionary<int, Assembly> HashedAssemblies = new Dictionary<int, Assembly>();

        public Assembly LoadFrom(string path)
        {
            var assembly = Assembly.LoadFrom(path);
            UserLoadedAssemblies.Add(assembly);
            return assembly;
        }

        public bool IsLoaded(string name) => HashedAssemblies.ContainsKey(name.GetHashCode());

        void AddAlreadyLoaded()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
                AddAssembly(assembly);
        }

        void AddAssembly(Assembly assembly)
        {
            var name = assembly.GetName().Name;
            var hash = name.GetHashCode();
            if (!HashedAssemblies.ContainsKey(hash))
                HashedAssemblies.Add(hash, assembly);
        }
    }
}