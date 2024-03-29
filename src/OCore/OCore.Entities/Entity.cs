﻿using Microsoft.Extensions.Logging;
using OCore.Core.Extensions;
using Orleans;
using System;
using System.Threading;
using System.Threading.Tasks;
using OCore.Authorization.Request;

namespace OCore.Entities
{
    public class Entity<T> : Grain<EntityState<T>>, IEntity
        where T : new()
    {
        EntityLogic<T> entityLogic;

        ILogger Logger => (ILogger<Entity<T>>)ServiceProvider.GetService(typeof(ILogger<Entity<T>>));


        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            entityLogic = new EntityLogic<T>(base.State,
                base.WriteStateAsync,
                OnCreating,
                Upgrade,
                this,
                Logger,
                Version);

            if (base.State.Created == false)
            {
                if (GetType().GetCustomAttributes(typeof(SuppressIndexingAttribute), true).Length == 0)
                {
                    var requestPayload = Payload.GetOrDefault();

                    if (requestPayload != null)
                    {
                        base.State.TenantId = requestPayload.TenantId;
                    }

                    base.State.Key = this.Key();
                }
            }

            await entityLogic.OnActivateAsync();
            activated = true;
            await base.OnActivateAsync(cancellationToken);
        }

        bool activated;

        protected DateTimeOffset CreatedAt => base.State.CreatedAt;

        protected bool Created
        {
            get => base.State.Created;
            set => base.State.Created = value;
        }

        protected DateTimeOffset UpdatedAt => base.State.UpdatedAt;

        protected int Version { get; set; }

        void EnsureEntityActivated()
        {
            if (activated == false)
            {
                throw new InvalidOperationException("Make sure the entity has been activated before trying to access keys. Did you forget to call base.OnActivateAsync()?");
            }
        }

        public string PrimaryKeyString
        {
            get
            {
                EnsureEntityActivated();
                return base.State.Key.String;
            }
        }

        public Guid PrimaryKeyGuid
        {
            get
            {
                EnsureEntityActivated();
                return base.State.Key.Guid.Value;
            }
        }
        public long PrimaryKeyLong
        {
            get
            {
                EnsureEntityActivated();
                return base.State.Key.Long.Value;
            }
        }
        public string KeyExtension
        {
            get
            {
                EnsureEntityActivated();
                return base.State.Key.Extension;
            }
        }

        protected virtual Task<int> Upgrade(int from)
        {
            return Task.FromResult(0);
        }

        public virtual Task Delete()
        {
            return entityLogic.Delete();
        }

        protected new T State
        {
            get
            {
                EntityState<T> s = base.State;
                if (s.Data == null)
                {
                    s.Data = new T();
                }
                return s.Data;
            }

            set
            {
                EntityState<T> s = base.State;
                s.Data = value;
            }
        }

        protected virtual Task OnCreating()
        {
            return Task.CompletedTask;
        }

        protected override Task WriteStateAsync()
        {
            return entityLogic.WriteStateAsync();
        }

        Task IEntity.ReadStateAsync()
        {
            return base.ReadStateAsync();
        }
    }
}
