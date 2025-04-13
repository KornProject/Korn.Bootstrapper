using System.Diagnostics;
using Korn.Plugins.Core;
using Korn.Shared;
using System.Linq;
using System.Text;
using System;
using Korn.Bootstrapper;
using Korn;

class Program3
{
    public static void Main(AssemblyLoader assemblyLoader)
    {
        try
        {
            InitializeBootstrapperEnv();
            InitializeCoreEnv();
            InitializePlugins();
            InitializeEvents();
        }
        catch (Exception ex)
        {
            KornShared.Logger.Exception(ex);
        }

        void InitializeBootstrapperEnv()
        {
            var process = Process.GetCurrentProcess();

            BootstrapperEnv.Logger = new KornLogger(Korn.Interface.Bootstrapper.LogFile);
            BootstrapperEnv.Logger.WriteMessage($"Bootstrapper started in process {process.ProcessName}({process.Id})");
            BootstrapperEnv.Logger.WriteMessage($"Sucessfully loaded {assemblyLoader.LoadedAssemblies.Count} libraries: {GetLibrariesList()}");

            BootstrapperEnv.AssemblyLoader = assemblyLoader;

            string GetLibrariesList()
            {
                var builder = new StringBuilder();

                builder.Append("{ ");
                var libraries = assemblyLoader.LoadedAssemblies;
                var lastLibrary = libraries.LastOrDefault();
                foreach (var library in libraries)
                {
                    var name = library.GetName().Name;
                    name = 
                        name
                        .Replace("Korn.Utils.", "ku.")
                        .Replace("Korn.", "k.");

                    builder.Append(name);

                    if (library != lastLibrary)
                        builder.Append(", ");
                }

                builder.Append(" }");

                return builder.ToString();
            }
        }

        void InitializeCoreEnv()
        {
            CoreEnv.CurrentProcess = Process.GetCurrentProcess();

            CoreEnv.Logger = BootstrapperEnv.Logger;

            CoreEnv.PluginLoader = new PluginLoader();
        }

        void InitializePlugins()
        {
            var currentProcess = Process.GetCurrentProcess();

            var plugins = Korn.Interface.Plugins.GetPluginsNames();

            foreach (var plugin in plugins)
                CoreEnv.PluginLoader.LoadPlugin(plugin);                
        }

        void InitializeEvents()
        {
            var assemblyWatcher = BootstrapperEnv.AssemblyWatcher = new AssemblyWatcher();

            assemblyWatcher.AssemblyLoad += (assembly) =>
            {
                foreach (var plugin in CoreEnv.PluginLoader.Plugins)
                    plugin.Dispatcher.OnAssemblyLoaded(assembly);
            };
            assemblyWatcher.EnsureAllAssembliesLoaded();
        }
    }
}
