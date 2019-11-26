using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using Shaman.Common.Utils.Helpers;

namespace Shaman.Common.Utils.Tests
{
    public class CompressionTests
    {
        [Test]
        public void GzipTest()
        {
            var enumerable = GenerateArray(10000);

            var compressed = CompressHelper.Compress(enumerable);
            var decompressed = CompressHelper.Decompress(compressed);
            enumerable.Should().BeEquivalentTo(decompressed);
            Console.Out.WriteLine("compressed.Length = {0}", compressed.Length);
        }

        private static byte[] GenerateArray(int count)
        {
            return new Fixture().Create<Generator<byte>>().Take(count/2).Concat(new byte[count/2]).ToArray();
        }
    }
}