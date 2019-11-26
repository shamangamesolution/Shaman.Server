using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Shaman.ServerSharedUtilities
{
    public class BundleHelper
    {
        public static T LoadTypeFromBundle<T>(string publishDir)
        {
            return (T) Activator.CreateInstance(LoadAndGet<T>(publishDir));
        }

        private static Type LoadAndGet<T>(string publishDir)
        {
            var files = Directory.GetFiles(publishDir).Where(f=>f.EndsWith(".dll"));
            Type targetType = null;
            foreach (var s in files)
            {
                try
                {
                    Console.Out.WriteLine("Loading dll: {0}", s);
                    var assembly = Assembly.LoadFrom(s);
                    if (targetType == null)
                    {
                        targetType = assembly.GetTypes().SingleOrDefault(t => t.GetInterfaces().Any(obj => obj == typeof(T)));    
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