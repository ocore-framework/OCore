using Orleans;
using System.Threading.Tasks;

namespace OCore.Entities
{
    public interface IEntity : IGrain
    {
        Task ReadStateAsync();
    }
}
