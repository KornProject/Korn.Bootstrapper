using System;
using System.Diagnostics;
using System.IO;

public class Program
{
    public static int ExternalMain(IntPtr args, int argLength)
    {
        var instance = new Program();
        instance.Main();
        return 0;
    }

    void Main()
    {
        File.WriteAllText(@"C:\d.txt", Process.GetCurrentProcess().Id + " " + DateTime.Now);
        Console.WriteLine("Hello, World!");
    }
}