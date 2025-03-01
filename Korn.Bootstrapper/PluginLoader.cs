using Korn.Interface.ServiceModule;
using Korn.Plugins.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Korn.Bootstrapper
{
    public class PluginLoader
    {
        public readonly List<Plugin> LoadedPlugins = new List<Plugin>();

        public Plugin LoadPlugin(string pluginName)
        {
            var process = Process.GetCurrentProcess();

            var pluginDirectory = Korn.Interface.ServiceModule.Plugins.GetDirectoryForPlugin(pluginName);
            if (!pluginDirectory.IsDirectoryExists)
                throw new Exception($"Korn.Bootstrapper.EntryPoint.Main->InitPlugins: Plugin \"{pluginName}\" directory doesn't exist.");

            if (!pluginDirectory.HasManifestFile)
                throw new Exception($"Korn.Bootstrapper.EntryPoint.Main->InitPlugins: Plugin \"{pluginName}\" manifest file \"{pluginDirectory.ManifestFilePath}\" doesn't exist.");

            var manifest = pluginDirectory.DeserializeManifest();
            if (manifest is null)
                throw new Exception($"Korn.Bootstrapper.EntryPoint.Main->InitPlugins: Plugin \"{pluginName}\" has invalid manifest file. Unable to deserialize.");

            if (!manifest.IsValid())
                throw new KornException($"Korn.Bootstrapper.EntryPoint.Main->InitPlugins: Plugin \"{pluginName}\" has invalid manifest file. Missing components.");

            PluginTarget foundPluginTarget = null;
            foreach (var target in manifest.Targets)
            {
                if (target.TargetProcesses != null)
                {
                    var isSuit = false;
                    foreach (var targetProcess in target.TargetProcesses)
                    {
                        if (targetProcess == process.ProcessName)
                        {
                            isSuit = true;
                            break;
                        }
                    }

                    if (isSuit)
                    {
                        if (target.TargetFramework == PluginFrameworkTarget.Net8)
                            foundPluginTarget = target;
                    }

                    break;
                }

                if (target.TargetFramework == PluginFrameworkTarget.Net8)
                    foundPluginTarget = target;
            }

            if (foundPluginTarget is null)
            {
                BootstrapperEnv.Logger.WriteMessage($"The process is not suitable for plugin \"{pluginName}\".");
                return null;
            }

            var (executablePath, executableClass) = (foundPluginTarget.ExecutableFilePath, foundPluginTarget.PluginClass);
            if (!File.Exists(executablePath))
                throw new Exception($"Korn.Bootstrapper.EntryPoint.Main->InitPlugins: Plugin \"{pluginName}\" has invalid manifest file target. Executable file is doesn't exists.");

            var assembly = Assembly.LoadFrom(executablePath);
            var type = assembly.GetType(executableClass);
            if (type == null)
                throw new KornException($"Korn.Bootstrapper.EntryPoint.Main->InitPlugins: Plugin \"{pluginName}\" has invalid manifest file target. Executable class is doesn't exists.");

            if (type.BaseType != typeof(Plugin))            
                throw new KornException($"Korn.Bootstrapper.EntryPoint.Main->InitPlugins: Plugin \"{pluginName}\" has invalid manifest file target. Base class of executable class is not Plugin.");

            return InitializePlugin();

            Plugin InitializePlugin()
            {
                var pluginInstace = Activator.CreateInstance(type) as Plugin;
                if (pluginInstace is null)
                    throw new KornException($"Korn.Bootstrapper.EntryPoint.Main->InitPlugins->InitializePlugin: Plugin \"{pluginName}\": Unable create instance for plugin type \"{type.FullName}\".");

                pluginInstace.Dispatcher.PluginDirectory = pluginDirectory;
                AddPlugin(pluginInstace);
                pluginInstace.OnLoad();
                pluginInstace.Dispatcher.IsAssemblyAlreadyLoaded = true;

                return pluginInstace;
            }
        }

        public void UnloadPlugin(Plugin plugin)
        {
            plugin.OnUnload();
            RemovePlugin(plugin);
        }

        void AddPlugin(Plugin plugin)
        {
            LoadedPlugins.Add(plugin);
            CoreEnv.PluginInstances.Add(plugin);
        }

        void RemovePlugin(Plugin plugin)
        {
            var f1 = LoadedPlugins.Remove(plugin);
            var f2 = CoreEnv.PluginInstances.Remove(plugin);

            if ((!f1 && f2) || (f1 && !f2))
                BootstrapperEnv.Logger.WriteWarning("Korn.Bootstrapper.PluginLoader->RemovePlugin: Unsynchronization between PluginLoader and CoreEnv in loaded plugins.");
        }
    }
}