using System;
using System.IO;
using System.Linq;

namespace Sample.BackEnd.Helpers
{
    public static class StorageHelper
    {
        //move to parameters
        public static string StorageFolder = @"C:\RobotWarfare\storage\";
        public static string StorageFileName = "storage";
        
        public static string GetStorageFileName(string version)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(StorageFolder + version);

            if (!di.Exists || !di.GetFiles().Any())
                return null;

            var storageFile = di.GetFiles().FirstOrDefault();

            return storageFile.FullName;

        }

        public static byte[] GetStorage(string version)
        {
            var fileName = StorageFolder + version + @"\" + StorageFileName;
            if (!File.Exists(fileName))
                throw new Exception($"Storage file for version {version} was not found");
            return System.IO.File.ReadAllBytes(fileName);
        }
    }
}