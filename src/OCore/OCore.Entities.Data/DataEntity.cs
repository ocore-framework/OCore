using System.Collections.Generic;
using OCore.Entities.Data.Extensions;
using Orleans;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

#nullable enable

namespace OCore.Entities.Data
{
    public abstract class DataEntity<T> :
        Entity<T>,
        IIdentifyable,
        IDataEntity<T> where T : new()

    {
        private readonly IEnumerable<IInterceptor<T>> _interceptors;

        public DataEntity()
        {
            _interceptors = ServiceProvider.GetServices<IInterceptor<T>>();
        }

        /// <summary>
        /// The ID for this DataEntity
        /// </summary>
        public string Id => this.GetPrimaryKeyString();

        /// <summary>
        /// Check to see if the data entity exists
        /// </summary>
        /// <returns>true if exists, false if it doesn't</returns>
        public Task<bool> Exists()
        {
            return Task.FromResult(Created);
        }

        public virtual async Task Create(T data)
        {
            if (Created == false)
            {
                if (_interceptors is not null)
                {
                    foreach (var interceptor in _interceptors)
                    {
                        await interceptor.OnCreate(data);
                    }
                }
                State = data;
                await WriteStateAsync();
                foreach (var subscriber in _subscribers)
                {
                    await subscriber.UpdateState(State);
                }
            }
            else
            {
                throw new DataCreationException($"DataEntity already created: {typeof(T)}");
            }
        }

        /// <summary>
        /// Get a separate DataEntity.
        /// </summary>
        /// <param name="identity">The identity of the DataEntity. If the parameter is omitted, the new DataEntity will share the same identity as the current DataEntity.</param>
        /// <typeparam name="T1">Type of the DataEntity.</typeparam>
        /// <returns>A reference to the DataEntity.</returns>
        protected T1 Get<T1>(string? identity = null) where T1 : IDataEntity
        {
            if (identity == null)
            {
                identity = this.GetPrimaryKeyString();
            }

            return GrainFactory.GetDataEntity<T1>(identity);
        }

        public virtual async Task<T> Read()
        {
            if (Created == true)
            {
                if (_interceptors is not null)
                {
                    foreach (var interceptor in _interceptors)
                    {
                        await interceptor.OnRead(State);
                    }
                }

                return State;
            }
            else
            {
                throw new DataCreationException($"DataEntity not created: {typeof(T)}");
            }
        }

        /// <inheritdoc />
        public virtual async Task Update(T data)
        {
            if (Created == true)
            {
                if (_interceptors is not null)
                {
                    foreach (var interceptor in _interceptors)
                    {
                        await interceptor.OnUpdate(State, data);
                    }
                }

                State = data;
                await WriteStateAsync();
                foreach (var subscriber in _subscribers)
                {
                    await subscriber.UpdateState(State);
                }
            }
            else
            {
                throw new DataCreationException($"DataEntity not created: {typeof(T)}");
            }
        }

        /// <inheritdoc />
        public virtual async Task PartialUpdate(T data, string[] fields)
        {
            if (Created is true && State is not null)
            {
                if (_interceptors is not null)
                {
                    foreach (var interceptor in _interceptors)
                    {
                        await interceptor.OnPartialUpdate(State, data, fields);
                    }
                }

                foreach (var field in fields)
                {
                    // Check if the data has the field using reflection
                    var property = State.GetType().GetProperty(field);

                    // If the property exists, update the value
                    if (property != null)
                    {
                        property.SetValue(State, property.GetValue(data));
                    }
                }

                await WriteStateAsync();
                foreach (var subscriber in _subscribers)
                {
                    await subscriber.UpdateState(State);
                }
            }
            else
            {
                throw new DataCreationException($"DataEntity not created: {typeof(T)}");
            }
        }

        public virtual async Task Upsert(T data)
        {
            if (_interceptors is not null)
            {
                foreach (var interceptor in _interceptors)
                {
                    await interceptor.OnUpsert(State, data);
                }
            }
            State = data;
            await WriteStateAsync();
            foreach (var subscriber in _subscribers)
            {
                await subscriber.UpdateState(State);
            }
        }

        public override async Task Delete()
        {
            if (Created == true)
            {
                if (_interceptors is not null)
                {
                    foreach (var interceptor in _interceptors)
                    {
                        await interceptor.OnDelete(State);
                    }
                }
                await base.Delete();
                foreach (var subscriber in _subscribers)
                {
                    subscriber.Complete();
                }
            }
            else
            {
                throw new DataCreationException($"DataEntity not created: {typeof(T)}");
            }
        }

        public virtual async Task Commit()
        {
            if (_interceptors is not null)
            {
                foreach (var interceptor in _interceptors)
                {
                    await interceptor.OnCommit(State);
                }
                foreach (var subscriber in _subscribers)
                {
                    await subscriber.UpdateState(State);
                }
            }
            await WriteStateAsync();
        }
        
        public IAsyncEnumerable<T> GetUpdates()
        {
            return new DataEntityUpdateEnumerable<T>(this);
        }

        List<IDataEntityUpdateEnumerable<T>> _subscribers = new();

        public void Subscribe(IDataEntityUpdateEnumerable<T> subscriber)
        {
            _subscribers.Add(subscriber);
        }

        public void Unsubscribe(IDataEntityUpdateEnumerable<T> subscriber)
        {
            _subscribers.Remove(subscriber);
        }

        public IAsyncEnumerable<string> GetJsonUpdates(bool jsonDiff = true)
        {
            return new DataEntityUpdateJsonEnumerable<T>(this, jsonDiff);
        }
    }
}