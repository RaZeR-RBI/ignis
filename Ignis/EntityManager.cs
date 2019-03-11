using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ConcurrentCollections;

namespace Ignis
{
    internal class EntityManager : IEntityManager
    {
        public EventHandler<EntityIdEventArgs> OnEntityCreated { get; set; } = null;
        public EventHandler<EntityIdEventArgs> OnEntityDestroyed { get; set; } = null;
        public EventHandler<EntityComponentEventArgs> OnEntityComponentAdded { get; set; } = null;
        public EventHandler<EntityComponentEventArgs> OnEntityComponentRemoved { get; set; } = null;

        public int EntityCount => (int)EntityCountLong;
        public long EntityCountLong => Interlocked.Read(ref _entityCount);

        private long _lastEntityId = 1;
        private long _entityCount = 0;
        private ConcurrentHashSet<long> _existingEntityIds = new ConcurrentHashSet<long>();
        private ConcurrentDictionary<long, ICollection<Type>> _componentMap =
            new ConcurrentDictionary<long, ICollection<Type>>();

        private ConcurrentDictionary<Type, IComponentCollectionStorage> _storageCache =
            new ConcurrentDictionary<Type, IComponentCollectionStorage>();
        
        private Func<Type, IComponentCollectionStorage> _storageResolver = null;

        public EntityManager(Func<Type, IComponentCollectionStorage> storageResolver) =>
            _storageResolver = storageResolver;


        public long Create()
        {
            if (EntityCountLong >= long.MaxValue)
                throw new ArgumentOutOfRangeException("Entity limit reached");

            var createdEntityId = _lastEntityId;
            while (Exists(createdEntityId))
                createdEntityId++;

            _existingEntityIds.Add(createdEntityId);
            _componentMap.AddOrUpdate(createdEntityId, new ConcurrentHashSet<Type>(), (k, v) =>
            {
                v.Clear();
                return v;
            });
            Interlocked.Increment(ref _entityCount);
            OnEntityCreated?.Invoke(this, new EntityIdEventArgs(createdEntityId));
            return createdEntityId;
        }

        public void Destroy(long entityId)
        {
            if (!Exists(entityId))
                return;

            _existingEntityIds.TryRemove(entityId);
            _componentMap.TryRemove(entityId, out _);
            Interlocked.Decrement(ref _entityCount);
            OnEntityDestroyed?.Invoke(this, new EntityIdEventArgs(entityId));
        }

        public bool Exists(long entityId) => _existingEntityIds.Contains(entityId);

        public void AddComponent(long entityId, Type type)
        {
            throw new NotImplementedException();
        }

        public void AddComponent<T>(long entityId) where T : struct
        {
            throw new NotImplementedException();
        }

        public void RemoveComponent(long entityId, Type type)
        {
            throw new NotImplementedException();
        }

        public void RemoveComponent<T>(long entityId) where T : struct
        {
            throw new NotImplementedException();
        }

        public bool HasComponent(long entityId, Type type)
        {
            throw new NotImplementedException();
        }

        public bool HasComponent<T>(long entityId)
        {
            throw new NotImplementedException();
        }
    }
}