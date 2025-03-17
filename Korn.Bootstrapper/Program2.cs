using Korn.Bootstrapper;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

class Program2
{
    public const string CurrentTargetVersion = Korn.Shared.Internal.KornSharedInternal.CurrentTargetVersion;
    public const string LibrariesListFile = Korn.Interface.ServiceModule.Libraries.LibrariesListFile;
    public const string LibrariesDirectory = Korn.Interface.ServiceModule.Libraries.LibrariesDirectory;

    public static void Main(AssemblyLoader assemblyLoader)
    {
        LoadLibraries();
        Program3.Main(assemblyLoader);

        void LoadLibraries()
        {
            var librariesList = JsonConvert.DeserializeObject<LocalCopy.LibrariesList>(File.ReadAllText(LibrariesListFile)).Libraries;
            var librariesDirectory = Path.Combine(LibrariesDirectory, CurrentTargetVersion);
            foreach (var library in librariesList)
            {
                if (library.TargetVersion == CurrentTargetVersion)

                    if (!string.IsNullOrEmpty(library.LocalFilePath))
                        LoadLibrary(library.LocalFilePath);
                    else
                    {
                        var libraryPath = Path.Combine(librariesDirectory, library.Name + ".dll");
                        LoadLibrary(libraryPath);
                    }
            }

            // LoadFrom is necessary, LoadFile doesn't work in this context
            void LoadLibrary(string path)
            {
                if (!File.Exists(path))
                    throw new FileNotFoundException($"Korn.Bootstrapper.Program2->Main->LoadLibraries->LoadLibrary: The library not found", path);

                assemblyLoader.LoadFrom(path);
            }
        }
    }
}

namespace LocalCopy
{
    public class LibrariesList
    {
        public List<Library> Libraries;

        public class Library
        {
            public string Name;
            public string TargetVersion;
            public string CurrentEntrySha;
            public string LocalFilePath; // null if don't use local file
        }
    }
}