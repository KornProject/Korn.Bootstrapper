using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

unsafe class Program
{
    public static int ExternalMain(IntPtr args, int argLength)
    {
        var instance = new Program();
        instance.Main();
        return 0;
    }

    void Main()
    {
        File.WriteAllText(@"C:\d2.txt", DateTime.Now.ToString() + "\n");
        Console.WriteLine("Injected!");

        File.AppendAllLines(@"C:\d2.txt", AppDomain.CurrentDomain.GetAssemblies().Select(a => a.FullName));

        var path = @"C:\Data\programming\vs projects\korn\Korn.Bootstrapper\TestNetframeworkLibrary\bin\Debug\TestNetframeworkLibrary.dll";
        var assembly = Assembly.LoadFile(path);
        File.AppendAllLines(@"C:\d2.txt", new string[] { "Loaded assembly!" });

        TestNetframeworkLibrary.Program.Main();
    }
}