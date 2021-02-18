using FluentAssertions;
using NUnit.Framework;

namespace Shaman.Common.Utils.Tests
{
    public class OnceDisposedTests
    {
        private class TestObj: OnceDisposable
        {
            public int DisposedCount { get; private set; }
            protected override void DisposeImpl()
            {
                lock (this)
                {
                    DisposedCount++;
                }
            }
        }
        
        [Test]
        public void Test()
        {
            var testObj = new TestObj();
            testObj.DisposedCount.Should().Be(0);
            
            testObj.Dispose();
            testObj.Dispose();
            testObj.Dispose();

            testObj.DisposedCount.Should().Be(1);
        }
    }
}