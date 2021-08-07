using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using ConcurrentCollections;

namespace Ignis
{
/// <summary>
/// Represents an entity ID set which contains all entity IDs that have the
/// specified component types (sometimes it's called an 'archetype').
/// </summary>
public interface IEntityView
{
	/// <summary>
	/// Returns the entity count in this view.
	/// </summary>
	int EntityCount { get; }

	[ExcludeFromCodeCoverage]
	/// <summary>
	/// Returns the entity count in this view.
	/// </summary>
	int Count => EntityCount;

	/// <summary>
	/// Checks if this view contains the specified entity ID.
	/// </summary>
	/// <param name="id">Entity ID to check</param>
	bool Contains(int id);

	/// <summary>
	/// Returns the component set against which the entity IDs are filtered
	/// for containment.
	/// </summary>
	IReadOnlyCollection<Type> Filter { get; }

	/// <summary>
	/// Copies the entity IDs from this view to a span.
	/// </summary>
	/// <param name="storage">Target span</param>
	/// <returns>Slice of the original span. Not fitting entries are skipped.</returns>
	Span<int> CopyTo(Span<int> storage);

	/// <summary>
	/// Returns an enumerator for this view.
	/// </summary>
	CollectionEnumerator<int> GetEnumerator()
	{
		return GetItems().GetEnumerator();
	}

	/// <summary>
	/// Returns an enumerable for this view.
	/// </summary>
	CollectionEnumerable<int> GetItems();

	/// <summary>
	/// Returns an IEnumerable for this view. Not recommended for use in
	/// hot paths because of enumerator boxing.
	/// </summary>
	IEnumerable<int> AsEnumerable()
	{
		return GetItems().AsEnumerable();
	}
}

internal class EntityView : IEntityView
{
	private readonly List<Type> _filter;
	private readonly ConcurrentHashSet<int> _ids;
	private readonly IEntityManager _em;
	private volatile int _entityCount = 0;

	public int EntityCount => _entityCount;

	public IReadOnlyCollection<Type> Filter => _filter;

	public EntityView(IEntityManager em, params Type[] components)
		: this(em, (IEnumerable<Type>) components)
	{
	}

	public EntityView(IEntityManager em, IEnumerable<Type> components)
	{
		_em = em;
		_filter = new List<Type>(components);
		_ids = new ConcurrentHashSet<int>();
		_em.OnEntityComponentAdded += OnEntityComponentAdded;
		_em.OnEntityComponentRemoved += OnEntityComponentRemoved;
		_em.OnEntityDestroyed += OnEntityDestroyed;
		FillSet();
	}

	private void OnEntityComponentAdded(object sender, EntityComponentEventArgs e)
	{
		TryAdd(e.EntityID);
	}

	private void OnEntityComponentRemoved(object sender, EntityComponentEventArgs e)
	{
		TryRemove(e.EntityID);
	}

	private void OnEntityDestroyed(object sender, EntityIdEventArgs e)
	{
		TryRemove(e.EntityID);
	}

	private void TryAdd(int id)
	{
		if (!Belongs(id))
			return;
		if (_ids.Add(id))
			Interlocked.Increment(ref _entityCount);
	}

	private void TryRemove(int id)
	{
		if (_em.Exists(id) && Belongs(id))
			return;
		if (_ids.TryRemove(id))
			Interlocked.Decrement(ref _entityCount);
	}

	public CollectionEnumerable<int> GetItems()
	{
		return new CollectionEnumerable<int>(_ids);
	}

	private bool Belongs(int id)
	{
		foreach (var type in _filter)
			if (!_em.HasComponent(id, type))
				return false;
		return true;
	}

	private void FillSet()
	{
		var lockTaken = false;
		try
		{
			Monitor.Enter(_em, ref lockTaken);
			foreach (var id in _em.GetEntityIds())
				if (Belongs(id))
				{
					_ids.Add(id);
					_entityCount++;
				}
		}
		finally
		{
			if (lockTaken)
				Monitor.Exit(_em);
		}
	}

	public bool Contains(int id)
	{
		return _ids.Contains(id);
	}

	public Span<int> CopyTo(Span<int> storage)
	{
		var lockTaken = false;
		try
		{
			Monitor.Enter(_ids);
			var i = 0;
			foreach (var id in _ids)
			{
				if (i >= storage.Length)
					break;
				storage[i++] = id;
			}

			return storage.Slice(0, i);
		}
		finally
		{
			if (lockTaken)
				Monitor.Exit(_ids);
		}
	}
}
}