using Korn.Plugins.Core;
using Korn.Plugins.ExternalInterface;
using System.Diagnostics;
using System.Reflection;

class EntryPoint
{
    public void Main()
    {
        var kornPath = Env.KornPath;
        try
        {
            InitializeCoreEnv();
            InitializeLogger();
            InitializePlugins();
        }
        catch (Exception ex)
        {
            Env.Logger.WriteException(ex);
        }

        void InitializeCoreEnv()
        {
            CoreEnv.KornPath = Env.KornPath;
        }

        void InitializeLogger()
        {
            var loggerPath = Path.Combine(kornPath, @"Data\bootstrapper-log.txt");
            Env.Logger = new(loggerPath);

            var currentProcess = Process.GetCurrentProcess();
            Env.Logger.WriteLine($"Bootstrapper started in process {currentProcess.ProcessName}({currentProcess.Id})");
        }

        void InitializePlugins()
        {
            var pluginsPath = Path.Combine(kornPath, @"Data\Plugins");
            var currentProcess = Process.GetCurrentProcess();

            var pluginsJson = KornPluginsList.Get()!;

            var pluginNames =
                pluginsJson.OfficialPlugins
                .Select(plugin => plugin.Name)
                .Concat(
                    pluginsJson.LocalPlugins
                    .Select(plugin => plugin.Name)
                );

            foreach (var pluginName in pluginNames)
                LoadPlugin(pluginName);

            void LoadPlugin(string pluginName)
            {
                var pluginDirectory = Path.Combine(pluginsPath, pluginName);
                if (!Directory.Exists(pluginDirectory))
                    Env.Logger.WriteError($"EntryPoint->Main->InitPlugins: Plugin \"{pluginName}\" directory doesn't exist.");

                var manifestFileName = "manifest.plugin";
                var manifestFile = Path.Combine(pluginDirectory, manifestFileName);
                if (!File.Exists(manifestFile))
                    Env.Logger.WriteError($"EntryPoint->Main->InitPlugins: Plugin \"{pluginName}\" manifest file \"{manifestFileName}\" doesn't exist.");

                var manifest = PluginManifest.Deserialize(File.ReadAllText(manifestFile));
                if (manifest is null)
                {
                    var message = $"EntryPoint->Main->InitPlugins: Plugin \"{pluginName}\" has invalid manifest file. Unable to deserialize.";
                    Env.Logger.Error(message);
                    throw new Exception(message);
                }

                if (manifest.Name is null ||
                    manifest.DisplayName is null ||
                    manifest.Version is null ||
                    manifest.Authors is null ||
                    manifest.Authors.Length == 0 ||
                    manifest.Targets is null ||
                    manifest.Targets.Length == 0)
                {
                    var message = $"EntryPoint->Main->InitPlugins: Plugin \"{pluginName}\" has invalid manifest file. Missing components.";
                    Env.Logger.Error(message);
                    throw new Exception(message);
                }

                PluginTarget? foundPluginTarget = null; 
                foreach (var target in manifest.Targets)
                {
                    if (target.TargetProcesses is not null)
                    {
                        var isSuit = false;
                        foreach (var targetProcess in target.TargetProcesses)
                        {
                            if (targetProcess == currentProcess.ProcessName)
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
                    Env.Logger.WriteMessage($"The process is not suitable for plugin \"{pluginName}\".");
                    return;
                }

                var (executablePath, executableClass) = (foundPluginTarget.ExecutableFilePath, foundPluginTarget.PluginClass);
                if (!File.Exists(executablePath))
                {
                    var message = $"EntryPoint->Main->InitPlugins: Plugin \"{pluginName}\" has invalid manifest file target. Executable file is doesn't exists.";
                    Env.Logger.Error(message);
                    throw new Exception(message);
                }

                var assembly = Assembly.LoadFrom(executablePath);
                var type = assembly.GetType(executableClass);
                if (type is null)
                {
                    var message = $"EntryPoint->Main->InitPlugins: Plugin \"{pluginName}\" has invalid manifest file target. Executable class is doesn't exists.";
                    Env.Logger.Error(message);
                    throw new Exception(message);
                }

                if (type.BaseType != typeof(Plugin))
                {
                    var message = $"EntryPoint->Main->InitPlugins: Plugin \"{pluginName}\" has invalid manifest file target. Base class of executable class is not Plugin.";
                    Env.Logger.Error(message);
                    throw new Exception(message);
                }

                InitializePlugin();

                void InitializePlugin()
                {
                    var pluginInstace = Activator.CreateInstance(type) as Plugin;
                    if (pluginInstace is null)
                    {
                        var message = $"EntryPoint->Main->InitPlugins->InitializePlugin: Plugin \"{pluginName}\": Unable create instance for plugin type \"{type.FullName}\".";
                        Env.Logger.Error(message);
                        throw new Exception(message);
                    }

                    pluginInstace.PluginDirectory = pluginDirectory;

                    CoreEnv.PluginInstances.Add(pluginInstace);

                    pluginInstace.OnLoad();
                }
            }
        }
    }
}