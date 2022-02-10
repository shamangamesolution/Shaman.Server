using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Shaman.Bundling.Common
{
    public interface IBundleLoader
    {
        T LoadTypeFromBundle<T>();
        HashSet<string> GetConfigs();
    }
    
    public class BundleLoader : IBundleLoader
    {
        private readonly string _bundleTempSubFolder = "shaman.bundles";
        private readonly IBundleInfoProvider _bundleInfoProvider;
        private readonly HashSet<string> _dll = new HashSet<string>();
        private readonly HashSet<string> _configs = new HashSet<string>();

        private string _publishDir;
        
        public BundleLoader(IBundleInfoProvider bundleInfoProvider) 
        {
            _bundleInfoProvider = bundleInfoProvider;
        }

        private string LoadFromHttp(string url, bool overwriteExisting = false)
        {
            var uri = new Uri(url);

            var bundlesFolder = Path.Combine(Path.GetTempPath(), $"{_bundleTempSubFolder}/{_bundleInfoProvider.GetServerRole().Result}");
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

            return newBundleFolder;
        }
        
        private void LoadAll(string publishDir)
        {
            var files = Directory.GetFiles(publishDir).Where(f => f.EndsWith(".dll") || f.EndsWith(".json"));
            Type targetType = null;
            foreach (var s in files)
            {
                if (s.EndsWith(".dll"))
                    _dll.Add(s);
                if (s.EndsWith(".json"))
                    _configs.Add(s);
            }
        }

        private async Task LoadBundle()
        {
            var uri = await _bundleInfoProvider.GetBundleUri();
            if (uri.StartsWith("http"))
                _publishDir = LoadFromHttp(uri, await _bundleInfoProvider.GetToOverwriteExisting());
            else
                _publishDir = uri;
            
            LoadAll(_publishDir);
        }

        public T LoadTypeFromBundle<T>()
        {
            LoadBundle().Wait();
            Type targetType = null;
            foreach (var s in _dll)
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
                    $"No implementation of {typeof(T)} found in assemblies from {Path.GetFullPath(_publishDir)}");
            }

            try
            {
                return (T) Activator.CreateInstance(targetType);
            }
            catch (Exception e)
            {
                throw new BundleLoadException($"Error activating {typeof(T).FullName} as {targetType.FullName}", e);
            }
        }

        public HashSet<string> GetConfigs()
        {
            return _configs;
        }
    }
}