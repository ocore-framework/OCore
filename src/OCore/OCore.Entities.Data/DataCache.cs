﻿using System;
using System.Threading.Tasks;

namespace OCore.Entities.Data
{
    public class DataCache<T>
    {
        public DataCache(IDataEntity<T> dataSource)
        {
            this.dataSource = dataSource;
        }

        public Type DataSourceType { get; } = typeof(T);

        public DateTimeOffset RefreshedAt { get; private set; }

        public TimeSpan CacheFor { get; set; }

        IDataEntity<T> dataSource;

        /// <summary>
        /// Refresh the datasource if time has expired or force = true
        /// </summary>
        /// <param name="force"></param>
        /// <returns></returns>
        public async Task Refresh(bool force = false)
        {
            if (force == true || DateTimeOffset.UtcNow - RefreshedAt > CacheFor)
            {
                data = await dataSource.Read();
            }
            RefreshedAt = DateTimeOffset.UtcNow;
        }

        T data;
        public T Data
        {
            get
            {
                if (DateTimeOffset.UtcNow - RefreshedAt > CacheFor)
                {
                    // TODO: Is this dubious for deadlocking? I wouldn't think
                    // so, but I have seen "strange behavior", and I think this is
                    // the only place where ContinueWith is used.
                    dataSource.Read().ContinueWith(x =>
                    {
                        data = x.Result;
                        RefreshedAt = DateTimeOffset.UtcNow;
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
                }
                return data;
            }
            set => data = value;
        }
    }
}
