/*
 *  IMPORTANT.
 *  DO NOT USE OTHER KORN COMPONENTS IN THIS CLASS.
*/

using Korn.Shared;
using System.IO;
using System.Reflection;

namespace Korn.Bootstrapper
{
    class Program
    {
        static void Main()
        {
            LoadSharedLibraries();
            new EntryPoint().Main();

            void LoadSharedLibraries()
            {
                var librariesPath = Path.Combine(Korn.Interface.ServiceModule.Libraries, BootstrapperEnv.TargetVersion);
                var files = Directory.GetFiles(librariesPath);
                foreach (var file in files)
                {
                    var extension = Path.GetExtension(file);

                    if (extension == ".dll")
                        LoadLibrary(file);
                    else if (extension == ".txt")
                    {
                        var path = File.ReadAllText(file);
                        LoadLibrary(path);
                    }
                }

                // LoadFrom is necessary, LoadFile doesn't work in this context
                void LoadLibrary(string path) => Assembly.LoadFrom(path);
            }
        }
    }
}