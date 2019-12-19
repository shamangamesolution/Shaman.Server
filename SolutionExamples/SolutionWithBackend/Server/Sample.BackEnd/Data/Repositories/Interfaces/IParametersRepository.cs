using System.Threading.Tasks;

namespace Sample.BackEnd.Data.Repositories.Interfaces
{
    public interface IParametersRepository
    {
        Task<string> GetStringValue(string name);
        Task<int> GetIntValue(string name);
        Task<bool> GetBoolValue(string name);
        Task<float> GetFloatValue(string name);
    }
}