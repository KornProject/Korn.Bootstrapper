/*
 *  IMPORTANT
 *  DO NOT USE OTHER KORN COMPONENTS IN Program
*/

using Korn.Bootstrapper;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

class Program
{
    const string WorkingDirectory = Korn.Interface.Bootstrapper.BinDirectory + "\\" + Korn.Shared.KornShared.CurrentTargetVersion;

    static void Main()
    {
        Thread.Sleep(30000);
        //Thread.Sleep(-1);

        var assemblyLoader = new AssemblyLoader();

        AddAssemblyResolver();
        LoadLibraries();
        Program2.Main(assemblyLoader);
        Thread.Sleep(int.MaxValue);

        void AddAssemblyResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += Handler;

            Assembly Handler(object sender, ResolveEventArgs args)
            {
                var name = args.Name.Split(',')[0];
                var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == name);
                return assembly;
            }
        }

        void LoadLibraries()
        {
            string[] libraryExtensions = new string[] { ".dll", ".exe" };

            var files = Directory.GetFiles(WorkingDirectory).Where(file => libraryExtensions.Contains(Path.GetExtension(file)));
            foreach (var file in files)
            {
                var assemblyName = Path.GetFileNameWithoutExtension(file);
                if (assemblyLoader.IsLoaded(assemblyName))
                    continue;

                assemblyLoader.LoadFrom(file);
            }
        }
    }
}