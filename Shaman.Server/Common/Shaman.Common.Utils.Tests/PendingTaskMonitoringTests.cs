using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common.Logging;

namespace Shaman.Common.Utils.Tests
{
    [NonParallelizable]
    public class PendingTaskMonitoringTests
    {
        [Test]
        public async Task TestPendingTaskStopping()
        {
            var mock = new Mock<IShamanLogger>();
            mock.Setup(c => c.Error(It.Is<string>(v => v.Contains("SHORT-LIVING"))));
            
            var taskSchedulerFactory = new TaskSchedulerFactory(mock.Object);
            var taskScheduler = taskSchedulerFactory.GetTaskScheduler();

            var counter = 0;

            PendingTask.DurationMonitoringTime(TimeSpan.FromMilliseconds(100));
            var task = taskScheduler.ScheduleOnInterval(() => counter++, 0, 10, true);

            await Task.Delay(300);
            
            mock.Verify(c => c.Error(It.Is<string>(v => v.Contains("SHORT-LIVING"))), Times.Once);

            counter.Should().BeInRange(9, 11);
        }
    }
}