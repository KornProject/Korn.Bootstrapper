/*
 *  IMPORTANT.
 *  DO NOT USE OTHER KORN COMPONENTS IN THIS CLASS.
*/

using System.Reflection;

string KornPath = Environment.GetEnvironmentVariable("KORN_PATH", EnvironmentVariableTarget.Machine)!;
    
LoadSharedLibraries();
new EntryPoint().Main();

void LoadSharedLibraries()
{
    var libraries = Path.Combine(KornPath, @"Data\Libraries\.net8");
    var files = Directory.GetFiles(libraries);
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