using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.TaskScheduling;

namespace Shaman.Common.Utils.Tests
{
    public class TaskSchedulerTests
    {
        private static readonly int TaskSchedulerInternalPeriodicTimersCount = 2;

        [SetUp]
        public void Setup()
        {
            // to exclude test interferring
            Thread.Sleep(100);
        }

        [Test]
        public void TaskCountingOneTimeTasksTest()
        {
            using (var taskScheduler1 = new TaskScheduler(Mock.Of<IShamanLogger>()))
            using (var taskScheduler2 = new TaskScheduler(Mock.Of<IShamanLogger>()))
            {
                taskScheduler1.ScheduleOnceOnNow(() => { Thread.Sleep(100); });
                taskScheduler1.ScheduleOnceOnNow(() => { Thread.Sleep(100); });
                taskScheduler2.ScheduleOnceOnNow(() => { Thread.Sleep(100); });
                taskScheduler2.ScheduleOnceOnNow(() => { Thread.Sleep(100); });

                TaskScheduler.GetGlobalScheduledOnceTasksCount().Should().Be(4);

                taskScheduler2.ScheduleOnceOnNow(() => { Thread.Sleep(100); });

                TaskScheduler.GetGlobalScheduledOnceTasksCount().Should().Be(5);

                Thread.Sleep(150);

                TaskScheduler.GetGlobalScheduledOnceTasksCount().Should().Be(0);
            }
        }

        [Test]
        public void TaskCountingDelayedTasksTest()
        {
            using (var taskScheduler1 = new TaskScheduler(Mock.Of<IShamanLogger>()))
            using (var taskScheduler2 = new TaskScheduler(Mock.Of<IShamanLogger>()))
            {
                taskScheduler1.Schedule(() => { Thread.Sleep(100); }, 10);
                taskScheduler1.Schedule(() => { Thread.Sleep(100); }, 10);
                taskScheduler1.Schedule(() => { Thread.Sleep(100); }, 10);
                taskScheduler1.Schedule(() => { Thread.Sleep(100); }, 10);

                PendingTask.GetActiveTimersCount().Should().Be(4);

                taskScheduler2.Schedule(() => { Thread.Sleep(100); }, 10);

                PendingTask.GetActiveTimersCount().Should().Be(5);
            }

            PendingTask.GetActiveTimersCount().Should().Be(0);
        }

        [Test]
        public void TaskCountingPeriodTasksTest()
        {
            using (var taskScheduler1 = new TaskScheduler(Mock.Of<IShamanLogger>()))
            using (var taskScheduler2 = new TaskScheduler(Mock.Of<IShamanLogger>()))
            {
                taskScheduler1.ScheduleOnInterval(() => { Thread.Sleep(100); }, 10, 10);
                taskScheduler1.ScheduleOnInterval(() => { Thread.Sleep(100); }, 10, 10);
                taskScheduler1.ScheduleOnInterval(() => { Thread.Sleep(100); }, 10, 10);
                taskScheduler1.ScheduleOnInterval(() => { Thread.Sleep(100); }, 10, 10);

                PendingTask.GetActivePeriodicTimersCount().Should().Be(4 + TaskSchedulerInternalPeriodicTimersCount);
                PendingTask.GetActivePeriodicSlTimersCount().Should().Be(0);

                taskScheduler2.ScheduleOnInterval(() => { Thread.Sleep(100); }, 10, 10);

                PendingTask.GetActivePeriodicTimersCount().Should().Be(5 + TaskSchedulerInternalPeriodicTimersCount);
                PendingTask.GetActivePeriodicSlTimersCount().Should().Be(0);
            }

            PendingTask.GetActivePeriodicTimersCount().Should().Be(0);
            PendingTask.GetActivePeriodicSlTimersCount().Should().Be(0);
        }
        [Test]
        public void TaskCountingPeriodShortLivingTasksTest()
        {
            using (var taskScheduler1 = new TaskScheduler(Mock.Of<IShamanLogger>()))
            using (var taskScheduler2 = new TaskScheduler(Mock.Of<IShamanLogger>()))
            {
                taskScheduler1.ScheduleOnInterval(() => { Thread.Sleep(100); }, 10, 10, true);
                taskScheduler1.ScheduleOnInterval(() => { Thread.Sleep(100); }, 10, 10, true);
                taskScheduler1.ScheduleOnInterval(() => { Thread.Sleep(100); }, 10, 10, true);
                taskScheduler1.ScheduleOnInterval(() => { Thread.Sleep(100); }, 10, 10, true);

                PendingTask.GetActivePeriodicTimersCount().Should().Be(TaskSchedulerInternalPeriodicTimersCount);
                PendingTask.GetActivePeriodicSlTimersCount().Should().Be(4);

                taskScheduler2.ScheduleOnInterval(() => { Thread.Sleep(100); }, 10, 10, true);

                PendingTask.GetActivePeriodicTimersCount().Should().Be(TaskSchedulerInternalPeriodicTimersCount);
                PendingTask.GetActivePeriodicSlTimersCount().Should().Be(5);
            }

            PendingTask.GetActivePeriodicTimersCount().Should().Be(0);
            PendingTask.GetActivePeriodicSlTimersCount().Should().Be(0);
        }

        [Test]
        public void TestActionExecutionCounting()
        {
            using (var taskScheduler1 = new TaskScheduler(Mock.Of<IShamanLogger>()))
            using (var taskScheduler2 = new TaskScheduler(Mock.Of<IShamanLogger>()))
            {
                taskScheduler1.Schedule(() => { Thread.Sleep(100); }, 10);
                taskScheduler1.Schedule(() => { Thread.Sleep(100); }, 10);
                taskScheduler2.Schedule(() => { Thread.Sleep(100); }, 10);
                Thread.Sleep(20);

                // schedulers tasks triggers only in second
                PendingTask.GetExecutingActionsCount().Should().Be(3);
            }

            Thread.Sleep(100);
            PendingTask.GetExecutingActionsCount().Should().Be(0);
        }
    }
}