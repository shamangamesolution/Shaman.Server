using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Shaman.ServerSharedUtilities
{
    public class BundleHelper
    {
        public static T LoadTypeFromBundle<T>(string uri)
        {
            return uri.StartsWith("http") ? LoadTypeFromHttpBundle<T>(uri) : LoadTypeFromLocalBundle<T>(uri);
        }

        private static T LoadTypeFromHttpBundle<T>(string url)
        {
            var uri = new Uri(url);

            var bundlesFolder = Path.Combine(Path.GetTempPath(), "shaman.bundles");
            var bundleDest = Path.Combine(bundlesFolder, uri.Segments.Last());

            if (!Directory.Exists(bundlesFolder))
                Directory.CreateDirectory(bundlesFolder);
            
            var newBundleFolder = Path.Combine(bundlesFolder,Path.GetFileNameWithoutExtension(bundleDest));
            if (!Directory.Exists(newBundleFolder))
            {
                Console.Out.WriteLine($"Downloading bundle from {uri}");
                using (var wc = new WebClient())
                    wc.DownloadFile(uri, bundleDest);
                ZipFile.ExtractToDirectory(bundleDest, newBundleFolder);
                File.Delete(bundleDest);
            }

            Console.Out.WriteLine($"Using bundle from {newBundleFolder}");
            
            return (T) Activator.CreateInstance(LoadAndGet<T>(newBundleFolder));
        }

        private static T LoadTypeFromLocalBundle<T>(string publishDir)
        {

            try
            {
                return (T) Activator.CreateInstance(LoadAndGet<T>(publishDir));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static Type LoadAndGet<T>(string publishDir)
        {
            var files = Directory.GetFiles(publishDir).Where(f => f.EndsWith(".dll"));
            Type targetType = null;
            foreach (var s in files)
            {
                try
                {
                    Console.Out.WriteLine("Loading dll: {0}", s);
                    var assembly = Assembly.LoadFrom(s);
                    if (targetType == null)
                    {
                        targetType = assembly.GetTypes()
                            .SingleOrDefault(t => t.GetInterfaces().Any(obj => obj == typeof(T)));
                    }

                    Console.Out.WriteLine("Assembly = {0}", assembly.FullName);
                }
                catch (FileLoadException e)
                {
                    if (!e.Message.Equals("Assembly with same name is already loaded"))
                    {
                        throw;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error loading {s}: {e}");
                    throw;
                }
            }

            if (targetType == null)
            {
                throw new BundleLoadException(
                    $"No implementation of {typeof(T)} found in assemblies from  {publishDir}");
            }

            return targetType;
        }
    }

    public class BundleLoadException : Exception
    {
        public BundleLoadException(string msg) : base(msg)
        {
        }
    }
}