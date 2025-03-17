/*
 *  IMPORTANT
 *  DO NOT USE OTHER KORN COMPONENTS IN Program AND Program2 CLASSES
*/

using Korn.Bootstrapper;
using Korn.Shared;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

class Program
{
    const string NewtonsoftAssemblyName = "Newtonsoft.Json";
    const string NewtonsoftFileName = NewtonsoftAssemblyName + ".dll";
    const string NewtonsoftPath = Korn.Interface.Bootstrapper.BinDirectory + "\\" + KornShared.CurrentTargetVersion + "\\" + NewtonsoftFileName;

    static void Main()
    {
        var assemblyLoader = new AssemblyLoader();

        AddAssemblyResolver();
        LoadNewtonsoftJson();
        Program2.Main(assemblyLoader);
        Thread.Sleep(int.MaxValue); // otherwise it crashes 🥺

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

        void LoadNewtonsoftJson()
        {
            if (IsAssemblyAlreadyLoaded())
                return;

            var path = NewtonsoftPath;
            if (!File.Exists(path))
                throw new FileNotFoundException($"Korn.Bootstrapper.Program->Main: The newtonsoft json library not found", path);

            assemblyLoader.LoadFrom(path);

            bool IsAssemblyAlreadyLoaded() => AppDomain.CurrentDomain.GetAssemblies().Any(assembly => assembly.GetName().Name == NewtonsoftAssemblyName);
        }
    }
}