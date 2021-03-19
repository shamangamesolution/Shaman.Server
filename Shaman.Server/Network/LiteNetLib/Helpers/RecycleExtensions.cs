using System;

namespace LiteNetLib.Helpers
{
    public static class RecycleExtensions
    {
        public static void SetRecycledWithAssert(this IAssertableOnRecycle assertable, bool value)
        {
            lock (assertable)
            {
                if (assertable.IsRecycled == value)
                    throw new Exception($"NetPacket already has Recycled value = {value}");
                assertable.IsRecycled = value;
            }
        }
    }
}