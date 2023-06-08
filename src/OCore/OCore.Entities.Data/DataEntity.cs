using OCore.Entities.Data.Extensions;
using Orleans;
using System.Threading.Tasks;

#nullable enable

namespace OCore.Entities.Data
{
    public abstract class DataEntity<T> : Entity<T>, IDataEntity<T> where T : new()
    {
        /// <summary>
        /// Check to see if the data entity exists
        /// </summary>
        /// <returns>true if exists, false if it doesn't</returns>
        public Task<bool> Exists()
        {
            return Task.FromResult(Created);
        }

        public virtual Task Create(T data)
        {
            if (Created == false)
            {
                State = data;
                return WriteStateAsync();
            }
            else
            {
                throw new DataCreationException($"DataEntity already created: {typeof(T)}");
            }
        }

        protected T1 Get<T1>(string? identity = null) where T1 : IDataEntity
        {
            if (identity == null)
            {
                identity = this.GetPrimaryKeyString();
            }
            return GrainFactory.GetDataEntity<T1>(identity);
        }

        public virtual Task<T> Read()
        {
            if (Created == true)
            {
                return Task.FromResult(State);
            }
            else
            {
                //throw new Exception($"DataEntity not created: {this.GetPrimaryKeyString()}/{typeof(T)}");
                throw new DataCreationException($"DataEntity not created: {typeof(T)}");
            }
        }

        /// <inheritdoc />
        public virtual Task Update(T data)
        {
            if (Created == true)
            {
                State = (T)data;
                return WriteStateAsync();
            }
            else
            {
                throw new DataCreationException($"DataEntity not created: {typeof(T)}");
            }
        }

        /// <inheritdoc />
        public async Task PartialUpdate(T data, string[] fields)
        {
            if (Created is true && State is not null)
            {
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
            }

            await WriteStateAsync();
        }

        public virtual Task Upsert(T data)
        {
            State = (T)data;
            return WriteStateAsync();
        }

        Task IDataEntity<T>.Delete()
        {
            if (Created == true)
            {
                return Delete();
            }
            else
            {
                throw new DataCreationException($"DataEntity not created: {typeof(T)}");
            }
        }

        public Task Commit()
        {
            return WriteStateAsync();
        }
    }
}
