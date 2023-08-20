using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ConcurrentCollections;

namespace Ignis.Storage
{
/// <summary>
/// A storage that doesn't store any values and throws exceptions on almost all
/// actions. Useful for testing purposes or when a component is just an empty 'tag'
/// (has no value).
/// </summary>
/// <typeparam name="T">Component type</typeparam>
public class NullStorage<T> : IComponentCollection<T>, IComponentCollectionStorage
	where T : new()
{
	private ConcurrentHashSet<int> _ids = new ConcurrentHashSet<int>();
	private readonly NullStorageEntityView<T> _view;

	public NullStorage()
	{
		_view = new NullStorageEntityView<T>(this);
	}

	public void Process(Func<int, T, T> action)
	{
		throw new InvalidOperationException();
	}

	public void Process<TState>(Func<int, T, TState, T> action, TState state)
	{
		throw new InvalidOperationException();
	}

	public void ForEach<TState>(Action<int, T, TState> action, TState state)
	{
		throw new InvalidOperationException();
	}

	public T Get(int entityId)
	{
		throw new InvalidOperationException();
	}

	public int GetCount()
	{
		return _ids.Count;
	}

	public CollectionEnumerable<int> GetEntityIds()
	{
		return new CollectionEnumerable<int>(_ids);
	}

	public CollectionEnumerable<T> GetValues()
	{
		throw new InvalidOperationException();
	}

	public IEntityView GetView()
	{
		return _view;
	}

	public bool RemoveComponentFromStorage(int entityId)
	{
		return _ids.TryRemove(entityId);
	}

	public bool StoreComponentForEntity(int entityId)
	{
		return _ids.Add(entityId);
	}

	public void Update(int entityId, T value)
	{
		throw new InvalidOperationException();
	}

	public void Update(int entityId, object value)
	{
		throw new InvalidOperationException();
	}

	public void UpdateCurrent(T value)
	{
		throw new InvalidOperationException();
	}

	public object GetValue(int entityId)
	{
		throw new InvalidOperationException();
	}

	private class NullStorageEntityView<U> : IEntityView
		where U : new()
	{
		private readonly NullStorage<U> _storage;

		public NullStorageEntityView(NullStorage<U> storage)
		{
			_storage = storage;
		}

		public int EntityCount => _storage._ids.Count;

		private static readonly Type[] s_filter = new Type[] {typeof(U)};
		public IReadOnlyCollection<Type> Filter => s_filter;

		public bool Contains(int id)
		{
			return _storage._ids.Contains(id);
		}

		public Span<int> CopyTo(Span<int> storage)
		{
			if (storage.Length <= 0) return storage;

			var e = GetItems().GetEnumerator();
			var i = 0;
			while (e.MoveNext() && i < storage.Length)
				storage[i++] = e.Current;
			return storage.Slice(0, i);
		}

		public CollectionEnumerable<int> GetItems()
		{
			return new CollectionEnumerable<int>(_storage._ids);
		}
	}
}
}