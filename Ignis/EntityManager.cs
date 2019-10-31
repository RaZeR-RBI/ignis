using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConcurrentCollections;
using static Ignis.InternalConstants;

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

		private int _lastEntityId = 1;
		private long _entityCount = 0;
		private ConcurrentHashSet<int> _existingEntityIds = new ConcurrentHashSet<int>();

		private ConcurrentHashSet<long> _entityComponentPairs = new ConcurrentHashSet<long>();
		private ConcurrentDictionary<Type, IComponentCollectionStorage> _storageCache =
			new ConcurrentDictionary<Type, IComponentCollectionStorage>();

		private Func<Type, IComponentCollectionStorage> _storageResolver = null;

		public EntityManager(Func<Type, IComponentCollectionStorage> storageResolver) =>
			_storageResolver = storageResolver;


		public int Create()
		{
			if (EntityCountLong >= long.MaxValue)
				throw new ArgumentOutOfRangeException("Entity limit reached");

			var createdEntityId = _lastEntityId;
			while (Exists(createdEntityId) || createdEntityId == NonExistingEntityId)
				unchecked
				{
					createdEntityId++;
				}

			_existingEntityIds.Add(createdEntityId);
			Interlocked.Increment(ref _entityCount);
			OnEntityCreated?.Invoke(this, new EntityIdEventArgs(createdEntityId));
			return createdEntityId;
		}

		public void Destroy(int entityId)
		{
			if (!Exists(entityId))
				return;

			_existingEntityIds.TryRemove(entityId);
			foreach (var componentType in _storageCache.Keys)
				RemoveComponent(entityId, componentType);

			Interlocked.Decrement(ref _entityCount);
			OnEntityDestroyed?.Invoke(this, new EntityIdEventArgs(entityId));
		}

		public bool Exists(int entityId) => _existingEntityIds.Contains(entityId);

		private IComponentCollectionStorage GetStorage(Type componentType) =>
			_storageCache.GetOrAdd(componentType, _storageResolver);

		public void AddComponent(int entityId, Type type)
		{
			if (HasComponent(entityId, type)) return;
			_entityComponentPairs.Add(HashPair(entityId, type));
			var storage = GetStorage(type);
			storage.StoreComponentForEntity(entityId);
			OnEntityComponentAdded?.Invoke(this, new EntityComponentEventArgs(entityId, type));
		}

		public void AddComponent<T>(int entityId) where T : new() =>
			AddComponent(entityId, typeof(T));

		public void RemoveComponent(int entityId, Type type)
		{
			_entityComponentPairs.TryRemove(HashPair(entityId, type));
			var storage = GetStorage(type);
			storage.RemoveComponentFromStorage(entityId);
			OnEntityComponentRemoved?.Invoke(this, new EntityComponentEventArgs(entityId, type));
		}

		public void RemoveComponent<T>(int entityId) where T : new() =>
			RemoveComponent(entityId, typeof(T));

		public bool HasComponent(int entityId, Type type) =>
			_entityComponentPairs.Contains(HashPair(entityId, type));

		public bool HasComponent<T>(int entityId) where T : new() =>
			HasComponent(entityId, typeof(T));

		private long HashPair(int entityId, Type componentType) =>
			((long)entityId << 32) + (long)componentType.GetHashCode();

		public IEnumerable<int> GetEntityIds() => _existingEntityIds;

		public IEnumerable<int> Query(params Type[] componentTypes) =>
			QuerySubset(GetEntityIds(), checkExistence: false, componentTypes);

		public IEnumerable<int> QuerySubset(IEnumerable<int> ids,
			bool checkExistence = true,
			params Type[] componentTypes)
		{
			var result = ids;
			if (checkExistence)
				result = result.Where(Exists);
			foreach (var type in componentTypes)
				result = result.Where(id => HasComponent(id, type));
			return result;
		}
	}
}