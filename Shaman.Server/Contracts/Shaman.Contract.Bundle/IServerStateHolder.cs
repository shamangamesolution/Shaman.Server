namespace Shaman.Contract.Bundle
{
    public interface IServerStateHolder
    {
        string Get();

        void Update(string state);
    }
}