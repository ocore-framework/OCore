using System.Threading.Tasks;

namespace OCore.Entities.Data;

public interface IInterceptor<T>
{
    /// <summary>
    /// Create new data entity. This call will fail if the entity already exists.
    /// </summary>
    /// <param name="data">the data the entity will be created with</param>
    Task OnCreate(T data);

    /// <summary>
    /// Read data from entity. This call will fail if the entity does not exist.
    /// </summary>
    /// <param name="state">the state to be returned</param>
    /// <returns></returns>
    Task<T> OnRead(T state);

    /// <summary>
    /// Update the data in the entity. This call will fail is the entity does not exist.
    /// </summary>
    /// <param name="state">the current state of the entity</param>
    /// <param name="data">the data the entity will be updated with</param>
    /// <returns></returns>
    Task OnUpdate(T state, T data);

    /// <summary>
    /// Do a partial update of the data in the entity. This call will fail is the entity does not exist.
    ///
    /// This only updates the fields specified in the fields array.
    /// </summary>
    /// <param name="state">the current state of the entity</param>
    /// <param name="data">The incoming data</param>
    /// <param name="fields">Fields to copy over, if present</param>
    /// <returns></returns>
    Task OnPartialUpdate(T state, T data, string[] fields);

    /// <summary>
    /// Update the data if the entity exists or create the entity if it does not exist.
    /// </summary>
    /// <param name="state">the current state of the entity</param>
    /// <param name="data">The data to upsert with</param>
    /// <returns></returns>
    Task OnUpsert(T state, T data);

    /// <summary>
    /// Delete the entity. This call will fail if the entity does not exist.
    /// </summary>
    /// <param name="state">the current state of the entity</param> 
    /// <returns></returns>
    Task OnDelete(T state);

    /// <summary>
    /// Commit the state changes to backing store.
    /// </summary>
    /// <param name="state">the current state of the entity</param>
    Task OnCommit(T state);
}