using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Routing.Tree;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack.Text;
using Shaman.Game.Contract;
using Shaman.GameBundleContract;

namespace Shaman.Game
{
    public class BundleLoader
    {
        private const string PublishDir = "/Users/ldv/src/robots/server/shaman/Shaman.Server/Servers/RW.Game.Bundle/publish";

        public static IGameResolver LoadGameBundle()
        {
            var gameBundleAssembly = GetGameBundleAssembly();
//            var assembly = typeof(IGameResolver).Assembly;
//            var tass = gameBundleAssembly.GetTypes().Where(t => t.Name.Contains("Resolver")).Single().GetInterfaces()
//                .First().Assembly;
            var resolverTypes = gameBundleAssembly.GetTypes().Where(t => t.HasInterface(typeof(IGameResolver)))
                .ToArray();
            if (!resolverTypes.Any())
            {
                throw new BundleLoadException(
                    $"No implementation of {typeof(IGameResolver)} found in assembly {gameBundleAssembly.FullName}");
            }

            if (resolverTypes.Length > 1)
            {
                throw new BundleLoadException(
                    $"More than one implementation of {typeof(IGameResolver)} found in assembly {gameBundleAssembly.FullName}. Found: {string.Join(",", resolverTypes.Select(t => t.FullName))}");
            }

            return (IGameResolver) Activator.CreateInstance(resolverTypes.Single());
        }

        private static Assembly GetGameBundleAssembly()
        {
            
            
            var files = Directory.GetFiles(PublishDir).Where(f=>f.EndsWith(".dll"));
//
//            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
//            Console.Out.WriteLine("baseDirectory = {0}", baseDirectory);
//
//            Assembly bundle = null;
//            foreach (var file in files)
//            {
//                Console.Out.WriteLine($"loading {file}");
//                var dest = $"{baseDirectory}/{Path.GetFileName(file)}";
//                if (File.Exists(dest))
//                {
//                    Console.Out.WriteLine($"{Path.GetFileName(file)} exists in Shaman bin");
//                    continue;
//                }
//                File.Copy(file,dest);
//                var la = Assembly.LoadFrom(dest);
//                if (dest.Contains("RW.Game.Bundle.dll"))
//                {
//                    bundle = la;
//                }
//            }
//            
//            if (bundle == null)
//            {
//                throw new BundleLoadException($"Game bundle not found");
//            }
//
//            return bundle;


            foreach (var s in files.Where(n => !n.Contains("RW.Game.Bundle.dll")))
            {
                try
                {
                    Console.Out.WriteLine("Loading dll: {0}", s);
                    var assembly = Assembly.LoadFrom(s);
//                    var assembly = Assembly.Load(File.ReadAllBytes(s));
                    Console.Out.WriteLine("Assembly = {0}", assembly.FullName);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error loading {s}: {e}");
                    throw;
                }
            }

            return Assembly.LoadFrom($"{PublishDir}/RW.Game.Bundle.dll");
        }
    }

    public class BundleLoadException : Exception
    {
        public BundleLoadException(string msg) : base(msg)
        {
        }
    }
}