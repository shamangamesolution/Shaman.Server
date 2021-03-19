namespace LiteNetLib
{
    public interface IAssertableOnRecycle
    {
        bool IsRecycled { get; set;  }
    }
}