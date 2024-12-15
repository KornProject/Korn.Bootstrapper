using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using mscoree;

namespace TestNetframeworkLibrary
{
    public static unsafe class Program
    {
        public static void Main() 
        {
            File.WriteAllText(@"C:\d3.txt", DateTime.Now.ToString() + "\n");

            try
            {
                IList<AppDomain> _IList = new List<AppDomain>();
                IntPtr enumHandle = IntPtr.Zero;
                ICorRuntimeHost host = new CorRuntimeHost();
                try
                {
                    host.EnumDomains(out enumHandle);
                    object domain = null;
                    while (true)
                    {
                        host.NextDomain(enumHandle, out domain);
                        if (domain == null) break;

                        File.AppendAllLines(@"C:\d3.txt", new string[] { $"type: {domain.GetType().FullName}" });

                        File.AppendAllLines(@"C:\d3.txt", new string[] { $"type: {string.Join(" ", domain.GetType().GetRuntimeFields().Select(f => $"[{f.Name} {f.FieldType.Name} - (is null: {f.GetValue(domain) == null}) {f.GetValue(domain)}]"))}" });

                        AppDomain appDomain = (AppDomain)domain;
                        _IList.Add(appDomain);
                    }

                    File.AppendAllLines(@"C:\d3.txt", _IList.Select(d => d.Id + "\n" + string.Join("\n", d.GetAssemblies().Select(a => a.FullName))));

                }
                catch (Exception e)
                {
                    File.AppendAllLines(@"C:\d3.txt", new string[] { e.ToString() });
                }
                finally
                {
                    host.CloseEnum(enumHandle);
                    Marshal.ReleaseComObject(host);
                }
            }
            catch (Exception ex)
            {
                File.AppendAllLines(@"C:\d3.txt", new string[] { ex.ToString() });
            }
        }
    }
}