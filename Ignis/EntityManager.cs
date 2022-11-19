using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConcurrentCollections;
using static Ignis.IgnisConstants;

namespace Ignis
{
internal class EntityManager : IEntityManager
{
	public EventHandler<EntityIdEventArgs> OnEntityCreated { get; set; } = null;
	public EventHandler<EntityIdEventArgs> OnEntityDestroying { get; set; } = null;
	public EventHandler<EntityIdEventArgs> OnEntityDestroyed { get; set; } = null;
	public EventHandler<EntityComponentEventArgs> OnEntityComponentAdded { get; set; } = null;
	public EventHandler<EntityComponentEventArgs> OnEntityComponentRemoving { get; set; } = null;
	public EventHandler<EntityComponentEventArgs> OnEntityComponentRemoved { get; set; } = null;

	public int EntityCount => (int) EntityCountLong;
	public long EntityCountLong => Interlocked.Read(ref _entityCount);

	private int _lastEntityId = 1;
	private long _entityCount = 0;
	private readonly ConcurrentHashSet<int> _existingEntityIds = new ConcurrentHashSet<int>();

	private readonly ConcurrentHashSet<long> _entityComponentPairs = new ConcurrentHashSet<long>();

	private readonly Dictionary<Type, IComponentCollectionStorage> _storageCache =
		new Dictionary<Type, IComponentCollectionStorage>();

	private readonly Func<Type, IComponentCollectionStorage> _storageResolver = null;

	private readonly List<IEntityView> _views = new List<IEntityView>();

	public EntityManager(Func<Type, IComponentCollectionStorage> storageResolver)
	{
		_storageResolver = storageResolver;
	}


	// TODO: Locks?
	public int Create()
	{
		if (EntityCountLong >= long.MaxValue)
			throw new ArgumentOutOfRangeException("Entity limit reached");

		var createdEntityId = _lastEntityId + 1;
		while (Exists(createdEntityId) || createdEntityId == NonExistingEntityId)
			unchecked
			{
				createdEntityId++;
			}

		_lastEntityId = createdEntityId;
		_existingEntityIds.Add(createdEntityId);
		Interlocked.Increment(ref _entityCount);
		OnEntityCreated?.Invoke(this, new EntityIdEventArgs(createdEntityId));
		return createdEntityId;
	}

	public bool Create(int requestedEntityId)
	{
		if (requestedEntityId == NonExistingEntityId) return false;
		if (Exists(requestedEntityId)) return false;
		_existingEntityIds.Add(requestedEntityId);
		Interlocked.Increment(ref _entityCount);
		return true;
	}

	public void Destroy(int entityId)
	{
		if (!Exists(entityId))
			return;

		OnEntityDestroying?.Invoke(this, new EntityIdEventArgs(entityId));
		_existingEntityIds.TryRemove(entityId);
		foreach (var kvp in _storageCache)
			RemoveComponent(entityId, kvp.Key);

		Interlocked.Decrement(ref _entityCount);
		OnEntityDestroyed?.Invoke(this, new EntityIdEventArgs(entityId));
	}

	public bool Exists(int entityId)
	{
		return _existingEntityIds.Contains(entityId);
	}

	private object _syncRoot = new object();

	private IComponentCollectionStorage GetStorage(Type componentType)
	{
		if (!_storageCache.ContainsKey(componentType))
			lock (_syncRoot)
			{
				if (!_storageCache.ContainsKey(componentType))
				{
					var storage = _storageResolver(componentType);
					_storageCache.Add(componentType, storage);
				}
			}

		return _storageCache[componentType];
	}

	public void AddComponent(int entityId, Type type)
	{
		if (HasComponent(entityId, type)) return;
		_entityComponentPairs.Add(HashPair(entityId, type));
		var storage = GetStorage(type);
		storage.StoreComponentForEntity(entityId);
		OnEntityComponentAdded?.Invoke(this, new EntityComponentEventArgs(entityId, type));
	}

	public void AddComponent<T>(int entityId) where T : new()
	{
		AddComponent(entityId, typeof(T));
	}

	public void RemoveComponent(int entityId, Type type)
	{
		if (!HasComponent(entityId, type)) return;
		OnEntityComponentRemoving?.Invoke(this, new EntityComponentEventArgs(entityId, type));
		_entityComponentPairs.TryRemove(HashPair(entityId, type));
		var storage = GetStorage(type);
		storage.RemoveComponentFromStorage(entityId);
		OnEntityComponentRemoved?.Invoke(this, new EntityComponentEventArgs(entityId, type));
	}

	public void RemoveComponent<T>(int entityId) where T : new()
	{
		RemoveComponent(entityId, typeof(T));
	}

	public bool HasComponent(int entityId, Type type)
	{
		return _entityComponentPairs.Contains(HashPair(entityId, type));
	}

	public bool HasComponent<T>(int entityId) where T : new()
	{
		return HasComponent(entityId, typeof(T));
	}

	private long HashPair(int entityId, Type componentType)
	{
		return ((long) entityId << 32) + (long) componentType.GetHashCode();
	}

	public CollectionEnumerable<int> GetEntityIds()
	{
		return new CollectionEnumerable<int>(_existingEntityIds);
	}

	public IEnumerable<int> QuerySubset<T, C>(T ids,
	                                          C componentTypes,
	                                          bool checkExistence = true)
		where T : IEnumerable<int>
		where C : IEnumerable<Type>
	{
		foreach (var id in ids)
		{
			if (checkExistence && !Exists(id))
				continue;
			foreach (var type in componentTypes)
				if (!HasComponent(id, type))
					goto next;
			yield return id;
			next: ;
		}
	}

	public ReadOnlySpan<int> QuerySubset(ReadOnlySpan<int> ids, Span<int> storage,
	                                     ReadOnlySpan<Type> componentTypes,
	                                     bool checkExistence = true)
	{
		var count = 0;
		for (var i = 0; i < ids.Length; i++)
		{
			var id = ids[i];
			if (count >= storage.Length) break;
			if (checkExistence && !Exists(id))
				continue;
			var hasComponents = true;
			for (var j = 0; j < componentTypes.Length; j++)
				if (!HasComponent(id, componentTypes[j]))
				{
					hasComponents = false;
					break;
				}

			if (!hasComponents) continue;
			storage[count++] = id;
		}

		return storage.Slice(0, count);
	}

	public ReadOnlySpan<int> Query(Span<int> storage, ReadOnlySpan<Type> componentTypes)
	{
		var count = 0;
		foreach (var id in _existingEntityIds)
		{
			if (count >= storage.Length) break;
			var hasComponents = true;
			for (var i = 0; i < componentTypes.Length; i++)
				if (!HasComponent(id, componentTypes[i]))
				{
					hasComponents = false;
					break;
				}

			if (!hasComponents) continue;
			storage[count++] = id;
		}

		return storage.Slice(0, count);
	}

	public ReadOnlySpan<int> Query(Span<int> storage, Type component1)
	{
		var count = 0;
		foreach (var id in _existingEntityIds)
		{
			if (count >= storage.Length) break;
			if (!HasComponent(id, component1)) continue;
			storage[count++] = id;
		}

		return storage.Slice(0, count);
	}

	public ReadOnlySpan<int> Query(Span<int> storage, Type component1, Type component2)
	{
		var count = 0;
		foreach (var id in _existingEntityIds)
		{
			if (count >= storage.Length) break;
			if (!HasComponent(id, component1)) continue;
			if (!HasComponent(id, component2)) continue;
			storage[count++] = id;
		}

		return storage.Slice(0, count);
	}

	public ReadOnlySpan<int> Query(Span<int> storage, Type component1, Type component2,
	                               Type component3)
	{
		var count = 0;
		foreach (var id in _existingEntityIds)
		{
			if (count >= storage.Length) break;
			if (!HasComponent(id, component1)) continue;
			if (!HasComponent(id, component2)) continue;
			if (!HasComponent(id, component3)) continue;
			storage[count++] = id;
		}

		return storage.Slice(0, count);
	}

	public ReadOnlySpan<int> Query(Span<int> storage, Type component1, Type component2,
	                               Type component3, Type component4)
	{
		var count = 0;
		foreach (var id in _existingEntityIds)
		{
			if (count >= storage.Length) break;
			if (!HasComponent(id, component1)) continue;
			if (!HasComponent(id, component2)) continue;
			if (!HasComponent(id, component3)) continue;
			if (!HasComponent(id, component4)) continue;
			storage[count++] = id;
		}

		return storage.Slice(0, count);
	}

	public ReadOnlySpan<int> QuerySubset(ReadOnlySpan<int> ids, Span<int> storage, Type component1,
	                                     bool checkExistence = true)
	{
		var count = 0;
		for (var i = 0; i < ids.Length; i++)
		{
			var id = ids[i];
			if (count >= storage.Length) break;
			if (checkExistence && !Exists(id))
				continue;
			if (!HasComponent(id, component1)) continue;
			storage[count++] = id;
		}

		return storage.Slice(0, count);
	}

	public ReadOnlySpan<int> QuerySubset(ReadOnlySpan<int> ids, Span<int> storage, Type component1,
	                                     Type component2, bool checkExistence = true)
	{
		var count = 0;
		for (var i = 0; i < ids.Length; i++)
		{
			var id = ids[i];
			if (count >= storage.Length) break;
			if (checkExistence && !Exists(id))
				continue;
			if (!HasComponent(id, component1)) continue;
			if (!HasComponent(id, component2)) continue;
			storage[count++] = id;
		}

		return storage.Slice(0, count);
	}

	public ReadOnlySpan<int> QuerySubset(ReadOnlySpan<int> ids, Span<int> storage, Type component1,
	                                     Type component2, Type component3,
	                                     bool checkExistence = true)
	{
		var count = 0;
		for (var i = 0; i < ids.Length; i++)
		{
			var id = ids[i];
			if (count >= storage.Length) break;
			if (checkExistence && !Exists(id))
				continue;
			if (!HasComponent(id, component1)) continue;
			if (!HasComponent(id, component2)) continue;
			if (!HasComponent(id, component3)) continue;
			storage[count++] = id;
		}

		return storage.Slice(0, count);
	}

	public ReadOnlySpan<int> QuerySubset(ReadOnlySpan<int> ids, Span<int> storage, Type component1,
	                                     Type component2, Type component3, Type component4,
	                                     bool checkExistence = true)
	{
		var count = 0;
		for (var i = 0; i < ids.Length; i++)
		{
			var id = ids[i];
			if (count >= storage.Length) break;
			if (checkExistence && !Exists(id))
				continue;
			if (!HasComponent(id, component1)) continue;
			if (!HasComponent(id, component2)) continue;
			if (!HasComponent(id, component3)) continue;
			if (!HasComponent(id, component4)) continue;
			storage[count++] = id;
		}

		return storage.Slice(0, count);
	}

	public void QueryTo(ICollection<int> storage, params Type[] componentTypes)
	{
		var ids = _existingEntityIds;
		foreach (var id in ids)
		{
			var hasComponents = true;
			foreach (var component in componentTypes)
				if (!HasComponent(id, component))
				{
					hasComponents = false;
					break;
				}

			if (hasComponents)
				storage.Add(id);
		}
	}

	public void QueryTo(ICollection<int> storage, Type component1)
	{
		var ids = _existingEntityIds;
		foreach (var id in ids)
		{
			if (!HasComponent(id, component1)) continue;
			storage.Add(id);
		}
	}

	public void QueryTo(ICollection<int> storage, Type component1, Type component2)
	{
		var ids = _existingEntityIds;
		foreach (var id in ids)
		{
			if (!HasComponent(id, component1)) continue;
			if (!HasComponent(id, component2)) continue;
			storage.Add(id);
		}
	}

	public void QueryTo(ICollection<int> storage, Type component1, Type component2, Type component3)
	{
		var ids = _existingEntityIds;
		foreach (var id in ids)
		{
			if (!HasComponent(id, component1)) continue;
			if (!HasComponent(id, component2)) continue;
			if (!HasComponent(id, component3)) continue;
			storage.Add(id);
		}
	}

	public void QueryTo(ICollection<int> storage, Type component1, Type component2, Type component3,
	                    Type component4)
	{
		var ids = _existingEntityIds;
		foreach (var id in ids)
		{
			if (!HasComponent(id, component1)) continue;
			if (!HasComponent(id, component2)) continue;
			if (!HasComponent(id, component3)) continue;
			if (!HasComponent(id, component4)) continue;
			storage.Add(id);
		}
	}

	private IEntityView GetViewByFilter(IEnumerable<Type> filter)
	{
		var set = filter.ToHashSet();
		foreach (var view in _views)
		{
			if (view.Filter.Count != set.Count)
				continue;
			foreach (var type in set)
				if (!view.Filter.Contains(type))
					goto next;
			return view;
			next: ;
		}

		return null;
	}

	public IEntityView GetView(IEnumerable<Type> filter)
	{
		var set = filter.ToHashSet();
		var view = GetViewByFilter(set);
		if (view != null) return view;
		view = new EntityView(this, filter);
		_views.Add(view);
		return view;
	}

	public IEntityView GetView(params Type[] filter)
	{
		return GetView((IEnumerable<Type>) filter);
	}

	public bool DestroyView(IEnumerable<Type> filter)
	{
		var view = GetViewByFilter(filter);
		if (view == null) return false;
		_views.Remove(view);
		return true;
	}

	public bool DestroyView(params Type[] filter)
	{
		return DestroyView((IEnumerable<Type>) filter);
	}
}
}