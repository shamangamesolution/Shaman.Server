using System.IO;
using System.Linq;
using Ionic.Zip;

namespace Shaman.Common.Utils.Helpers
{
    public static class CompressHelper
    {
        public static byte[] Compress(byte[] bytesToCompress)
        {
            byte[] returnData;

            using (MemoryStream stream = new MemoryStream(bytesToCompress))
            {
                using (ZipFile zip = new ZipFile())
                {
                    zip.ParallelDeflateThreshold = -1;
                    zip.AddEntry("compress", stream);

                    using (MemoryStream newStream = new MemoryStream())
                    {
                        zip.Save(newStream);
                        returnData = newStream.ToArray();
                    }
                }
            }

            return returnData;
        }

        public static byte[] Decompress(byte[] bytesToDecompress)
        {
            using (MemoryStream stream = new MemoryStream(bytesToDecompress))
            using (ZipFile zout = ZipFile.Read(stream))
            {
                zout.ParallelDeflateThreshold = -1;
                ZipEntry entry = zout.FirstOrDefault();
                using (MemoryStream newStream = new MemoryStream())
                {
                    entry.Extract(newStream);
                    return newStream.ToArray();
                }
            }
        }
    }
}
