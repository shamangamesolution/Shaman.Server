using System.Collections.Generic;
using System.Threading.Tasks;
using Sample.Shared.Data.Entity.Timers;

namespace Sample.BackEnd.Data.Timers
{

    public interface ITimerRepository
    {
        Task StartTimer(int playerId, TimerType timerType, int relatedObjectId, int secondsToComplete);
        Task<List<Timer>> GetTimer(int playerId);
        Task StopTimer(int timerId);
    }
}