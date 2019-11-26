using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ServiceStack.Text;
using Shaman.MM.Contract;

namespace Shaman.MM
{
    public class BundleLoader
    {
        private const string PublishDir = "/Users/ldv/src/robots/server/shaman/Shaman.Server/Servers/RW.MM.Bundle/publish";

        public static void LoadMmBundle(IMatchMakingConfigurator configurator)
        {
            var mmBundleAssembly = GetMmBundleAssembly();
            var resolverTypes = mmBundleAssembly.GetTypes().Where(t => t.HasInterface(typeof(IMmResolver)))
                .ToArray();
            if (!resolverTypes.Any())
            {
                throw new BundleLoadException(
                    $"No implementation of {typeof(IMmResolver)} found in assembly {mmBundleAssembly.FullName}");
            }

            if (resolverTypes.Length > 1)
            {
                throw new BundleLoadException(
                    $"More than one implementation of {typeof(IMmResolver)} found in assembly {mmBundleAssembly.FullName}. Found: {string.Join(",", resolverTypes.Select(t => t.FullName))}");
            }

            var instance = (IMmResolver) Activator.CreateInstance(resolverTypes.Single());
            instance.Configure(configurator);
        }

        private static Assembly GetMmBundleAssembly()
        {
            var files = Directory.GetFiles(PublishDir).Where(f=>f.EndsWith(".dll"));

            foreach (var s in files.Where(n => !n.Contains("RW.MM.Bundle.dll")))
            {
                try
                {
                    Console.Out.WriteLine("Loading dll: {0}", s);
                    var assembly = Assembly.LoadFrom(s);
                    Console.Out.WriteLine("Assembly = {0}", assembly.FullName);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error loading {s}: {e}");
                    throw;
                }
            }

            return Assembly.LoadFrom($"{PublishDir}/RW.MM.Bundle.dll");
        }
    }

    public class BundleLoadException : Exception
    {
        public BundleLoadException(string msg) : base(msg)
        {
        }
    }
}