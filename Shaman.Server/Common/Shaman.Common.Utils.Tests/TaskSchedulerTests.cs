using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common.Logging;
using TaskScheduler = Shaman.Common.Utils.TaskScheduling.TaskScheduler;

namespace Shaman.Common.Utils.Tests
{
    [NonParallelizable]
    public class TaskSchedulerTests
    {
        private static readonly int TaskSchedulerInternalPeriodicTimersCount = 2;
        private TaskScheduler _taskScheduler;

        [SetUp]
        public void Setup()
        {
            // to exclude test interferring
            // Thread.Sleep(100);
            _taskScheduler = new TaskScheduler(Mock.Of<IShamanLogger>());
        }

        [TearDown]
        public void TearDown()
        {
            _taskScheduler.Dispose();
            PendingTask.GetActivePeriodicTimersCount().Should().Be(0);
            PendingTask.GetActivePeriodicSlTimersCount().Should().Be(0);
            PendingTask.GetExecutingActionsCount().Should().Be(0);
            PendingTask.GetActiveTimersCount().Should().Be(0);
        }

        private static void BadMethod()
        {
            throw new Exception("TEST");
        }

        [Test]
        public void ScheduleOnceOnNowAsyncExceptionTest()
        {
            _taskScheduler.ScheduleOnceOnNow(async () =>
            {
                await Task.Delay(10);
                BadMethod();
            });
            Thread.Sleep(1000);
        }
        
        [Test]
        public void ScheduleOnceOnNowExceptionTest()
        {
            _taskScheduler.ScheduleOnceOnNow(() =>
            {
                Console.Out.WriteLine("Test");
                BadMethod();
            });
            Thread.Sleep(1000);
        }

        [Test]
        public void ScheduleAsyncExceptionTest()
        {
            _taskScheduler.Schedule(async () =>
            {
                await Task.Delay(10);
                BadMethod();
            }, 10);
            Thread.Sleep(1000);
        }

        [Test]
        public void ScheduleExceptionTest()
        {
            _taskScheduler.Schedule(() =>
            {
                Console.Out.WriteLine("Test");
                BadMethod();
            }, 10);
            Thread.Sleep(1000);
        }
        
        [Test]
        public void TaskCountingOneTimeTasksTest()
        {
            using (var taskScheduler2 = new TaskScheduler(Mock.Of<IShamanLogger>()))
            {
                _taskScheduler.ScheduleOnceOnNow(() => { Thread.Sleep(100); });
                _taskScheduler.ScheduleOnceOnNow(() => { Thread.Sleep(100); });
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
            using (var taskScheduler2 = new TaskScheduler(Mock.Of<IShamanLogger>()))
            {
                _taskScheduler.Schedule(() => { Thread.Sleep(100); }, 10);
                _taskScheduler.Schedule(() => { Thread.Sleep(100); }, 10);
                _taskScheduler.Schedule(() => { Thread.Sleep(100); }, 10);
                _taskScheduler.Schedule(() => { Thread.Sleep(100); }, 10);

                PendingTask.GetActiveTimersCount().Should().Be(4);

                taskScheduler2.Schedule(() => { Thread.Sleep(100); }, 10);

                PendingTask.GetActiveTimersCount().Should().Be(5);
            }

        }

        [Test]
        public void TaskCountingPeriodTasksTest()
        {
            using (var taskScheduler2 = new TaskScheduler(Mock.Of<IShamanLogger>()))
            {
                _taskScheduler.ScheduleOnInterval(() => { Thread.Sleep(100); }, 10, 10);
                _taskScheduler.ScheduleOnInterval(() => { Thread.Sleep(100); }, 10, 10);
                _taskScheduler.ScheduleOnInterval(() => { Thread.Sleep(100); }, 10, 10);
                _taskScheduler.ScheduleOnInterval(() => { Thread.Sleep(100); }, 10, 10);

                PendingTask.GetActivePeriodicTimersCount().Should().Be(4 + TaskSchedulerInternalPeriodicTimersCount);
                PendingTask.GetActivePeriodicSlTimersCount().Should().Be(0);

                taskScheduler2.ScheduleOnInterval(() => { Thread.Sleep(100); }, 10, 10);

                PendingTask.GetActivePeriodicTimersCount().Should().Be(5 + TaskSchedulerInternalPeriodicTimersCount);
                PendingTask.GetActivePeriodicSlTimersCount().Should().Be(0);
            }
        }
        [Test]
        public void TaskCountingPeriodShortLivingTasksTest()
        {
            using (var taskScheduler2 = new TaskScheduler(Mock.Of<IShamanLogger>()))
            {
                _taskScheduler.ScheduleOnInterval(() => { Thread.Sleep(100); }, 10, 10, true);
                _taskScheduler.ScheduleOnInterval(() => { Thread.Sleep(100); }, 10, 10, true);
                _taskScheduler.ScheduleOnInterval(() => { Thread.Sleep(100); }, 10, 10, true);
                _taskScheduler.ScheduleOnInterval(() => { Thread.Sleep(100); }, 10, 10, true);

                PendingTask.GetActivePeriodicTimersCount().Should().Be(TaskSchedulerInternalPeriodicTimersCount);
                PendingTask.GetActivePeriodicSlTimersCount().Should().Be(4);

                taskScheduler2.ScheduleOnInterval(() => { Thread.Sleep(100); }, 10, 10, true);

                PendingTask.GetActivePeriodicTimersCount().Should().Be(TaskSchedulerInternalPeriodicTimersCount);
                PendingTask.GetActivePeriodicSlTimersCount().Should().Be(5);
            }
        }

        [Test]
        public void TestActionExecutionCounting()
        {
            using (var taskScheduler2 = new TaskScheduler(Mock.Of<IShamanLogger>()))
            {
                _taskScheduler.Schedule(() => { Thread.Sleep(100); }, 10);
                _taskScheduler.Schedule(() => { Thread.Sleep(100); }, 10);
                taskScheduler2.Schedule(() => { Thread.Sleep(100); }, 10);
                Thread.Sleep(20);

                // schedulers tasks triggers only in second
                PendingTask.GetExecutingActionsCount().Should().Be(3);
            }
            Thread.Sleep(100);// wait task ends
        }
    }
}