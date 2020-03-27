using System.IO;
using System.IO.Compression;

namespace Shaman.Common.Utils.Helpers
{
    public static class CompressHelper
    {
        public static byte[] Compress(byte[] bytesToCompress)
        {
            byte[] returnData;

            using (MemoryStream output = new MemoryStream())
            using (MemoryStream input = new MemoryStream(bytesToCompress))
            {
                using (var zip = new GZipStream(output, CompressionLevel.Fastest))
                {
                    input.CopyTo(zip);
                    input.Flush();
                    output.Flush();
                    zip.Close();
                    returnData = output.ToArray();
                }
            }

            return returnData;
        }

        public static byte[] Decompress(byte[] bytesToDecompress)
        {
            byte[] returnData;

            using (MemoryStream output = new MemoryStream())
            using (MemoryStream input = new MemoryStream(bytesToDecompress))
            {
                using (var zip = new GZipStream(input, CompressionMode.Decompress))
                {
                    zip.CopyTo(output);
                    zip.Flush();
                    output.Flush();
                    zip.Close();
                    returnData = output.ToArray();
                }
            }

            return returnData;
        }
    }
}
