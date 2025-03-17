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
        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var hash = args.Name.GetHashCode();
            HashedAssemblies.TryGetValue(hash, out Assembly result);
            return result;
        }

        public readonly List<Assembly> LoadedAssemblies = new List<Assembly>();
        public readonly Dictionary<int, Assembly> HashedAssemblies = new Dictionary<int, Assembly>();

        public void LoadFrom(string path)
        {
            var assembly = Assembly.LoadFrom(path);
            Load(assembly);
        }

        public void Load(Assembly assembly)
        {
            LoadedAssemblies.Add(assembly);

            var name = assembly.GetName().Name;
            var hash = name.GetHashCode();
            HashedAssemblies.Add(hash, assembly);
        }
    }
}