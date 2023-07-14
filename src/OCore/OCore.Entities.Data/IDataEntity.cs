using System.Collections.Generic;
using Orleans;
using System.Threading.Tasks;

#nullable enable

namespace OCore.Entities.Data
{
    public interface IIdentifyable
    {
        /// <summary>
        /// The ID for this DataEntity
        /// </summary>
        string Id { get; }
    }

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
        /// Do a partial update of the data in the entity. This call will fail is the entity does not exist.
        ///
        /// This only updates the fields specified in the fields array.
        /// </summary>
        /// <param name="data">The incoming data</param>
        /// <param name="fields">Fields to copy over, if present</param>
        /// <returns></returns>
        Task PartialUpdate(T data, string[] fields);
        
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
        /// Commit the state changes to backing store.
        /// </summary>
        Task Commit();
        
        IAsyncEnumerable<string> GetJsonUpdates(bool jsonDiff = true);

        IAsyncEnumerable<T> GetUpdates();

        void Subscribe(IDataEntityUpdateEnumerable<T> subscriber);
        
        void Unsubscribe(IDataEntityUpdateEnumerable<T> subscriber);
    }
}
