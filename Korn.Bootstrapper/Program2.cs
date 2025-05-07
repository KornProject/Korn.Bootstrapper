using System.Diagnostics;
using Korn.Plugins.Core;
using Korn.Bootstrapper;
using Korn.Shared;
using System;
using Korn;

#pragma warning disable CS0028 // {type} has the wrong signature to be an entry point
class Program2
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
            BootstrapperEnv.Logger.WriteMessage($"Bootstrapper started in \"{process.ProcessName}\"({process.Id})");

            BootstrapperEnv.AssemblyLoader = assemblyLoader;
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
