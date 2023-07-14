using System.Threading.Tasks;

namespace OCore.Entities.Data;

public interface IDataEntityUpdateEnumerable<T>
{
    Task UpdateState(T state);

    void Complete();
}