using Korn.Shared;
using System;
using System.Diagnostics;
using System.Linq;

namespace Korn.Bootstrapper
{
    class EntryPoint
    {
        public void Main()
        {
            try
            {
                InitializeBootstrapperEnv();
                InitializeCoreEnv();
                InitializePlugins();
            }
            catch (Exception ex)
            {
                KornShared.Logger.WriteException(ex);
            }

            void InitializeBootstrapperEnv()
            {
                var process = Process.GetCurrentProcess();

                BootstrapperEnv.Logger = new KornLogger(Korn.Interface.Bootstrapper.LogFile);
                BootstrapperEnv.Logger.WriteMessage($"Bootstrapper started in process {process.ProcessName}({process.Id})");

                BootstrapperEnv.PluginLoader = new PluginLoader();
            }

            void InitializeCoreEnv()
            {

            }

            void InitializePlugins()
            {
                var currentProcess = Process.GetCurrentProcess();

                var plugins = Korn.Interface.ServiceModule.Plugins.DeserializePluginsList();

                var pluginNames =
                    plugins.OfficialPlugins
                    .Select(plugin => plugin.Name)
                    .Concat(
                        plugins.LocalPlugins
                        .Select(plugin => plugin.Name)
                    );

                foreach (var pluginName in pluginNames)
                    BootstrapperEnv.PluginLoader.LoadPlugin(pluginName);                
            }
        }
    }
}