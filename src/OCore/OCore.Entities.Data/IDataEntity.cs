using Orleans;
using System.Threading.Tasks;

#nullable enable

namespace OCore.Entities.Data
{
    public interface IDataEntity : IGrainWithStringKey
    {
    }

    public interface IDataEntity<T> : IDataEntity
    {
        /// <summary>
        /// Check to see if the data entity exists
        /// </summary>
        /// <returns></returns>
        Task<bool> Exists();
        
        /// <summary>
        /// Create new data entity. This call will fail if the entity already exists.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task Create(T data);

        /// <summary>
        /// Read data from entity. This call will fail if the entity does not exist.
        /// </summary>
        /// <returns></returns>
        Task<T> Read();

        /// <summary>
        /// Update the data in the entity. This call will fail is the entity does not exist.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task Update(T data);

        /// <summary>
        /// Update the data if the entity exists or create the entity if it does not exist.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task Upsert(T data);

        /// <summary>
        /// Delete the entity. This call will fail if the entity does not exist.
        /// </summary>
        /// <returns></returns>
        Task Delete();

        /// <summary>
        /// Commit the state changes to backing store4
        /// </summary>
        Task Commit();
    }
}
