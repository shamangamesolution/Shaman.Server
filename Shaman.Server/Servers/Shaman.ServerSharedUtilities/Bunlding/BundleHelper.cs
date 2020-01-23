using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Shaman.ServerSharedUtilities.Bunlding
{
    public class BundleHelper
    {
        public static T LoadTypeFromBundle<T>(string uri, bool overwriteExisting = false)
        {
            Console.Out.WriteLine($"OverwriteExisting: {overwriteExisting}");

            return uri.StartsWith("http") ? LoadTypeFromHttpBundle<T>(uri, overwriteExisting) : LoadTypeFromLocalBundle<T>(uri);
        }

        private static T LoadTypeFromHttpBundle<T>(string url, bool overwriteExisting = false)
        {
            var uri = new Uri(url);

            var bundlesFolder = Path.Combine(Path.GetTempPath(), "shaman.bundles");
            var bundleDest = Path.Combine(bundlesFolder, uri.Segments.Last());

            if (!Directory.Exists(bundlesFolder))
                Directory.CreateDirectory(bundlesFolder);
            
            var newBundleFolder = Path.Combine(bundlesFolder,Path.GetFileNameWithoutExtension(bundleDest));
            var folderExists = Directory.Exists(newBundleFolder);
            if (!folderExists || overwriteExisting)
            {
                if (folderExists)
                {
                    Directory.Delete(newBundleFolder, true);
                }
                Console.Out.WriteLine($"Downloading bundle from {uri}");
                using (var wc = new WebClient())
                    wc.DownloadFile(uri, bundleDest);
                ZipFile.ExtractToDirectory(bundleDest, newBundleFolder);
                File.Delete(bundleDest);
            }

            Console.Out.WriteLine($"Using bundle from {newBundleFolder}");
            
            return GetTypeInstance<T>(newBundleFolder);
        }

        private static T GetTypeInstance<T>(string newBundleFolder)
        {
            var type = LoadAndGet<T>(newBundleFolder);
            Console.Out.WriteLine($"Bundle mapped as {typeof(T).FullName}: {type.FullName}");
            try
            {
                return (T) Activator.CreateInstance(type);
            }
            catch (Exception e)
            {
                throw new BundleLoadException($"Error activating {typeof(T).FullName} as {type.FullName}", e);
            }
        }

        private static T LoadTypeFromLocalBundle<T>(string publishDir)
        {
            return GetTypeInstance<T>(publishDir);
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
                            .SingleOrDefault(t => !t.IsAbstract && t.GetInterfaces().Any(obj => obj == typeof(T)));
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
        public BundleLoadException(string msg, Exception e) : base(msg, e)
        {
        }
    }
}