using System;
using System.Collections.Generic;

namespace Ignis.Storage;

public class SparseLinearDictionaryStorage<T> : IComponentCollection<T>, IComponentCollectionStorage
	where T : new()
{
	private readonly SparseLinearDictionary<int, T> _data = new ();
	private readonly View _view;

	public SparseLinearDictionaryStorage()
	{
		_view = new View(this);
	}

	public void Process(Func<int, T, T> action)
	{
		var p = _data.GetPresenceData();
		var k = _data.GetKeyData();
		var v = _data.GetValueData();
		for (var i = 0; i < p.Length; i++)
		{
			if (p[i] == 0) continue;
			ref var val = ref v[i];
			val = action(k[i], val);
		}
	}

	public void Process<TState>(Func<int, T, TState, T> action, TState state)
	{
		var p = _data.GetPresenceData();
		var k = _data.GetKeyData();
		var v = _data.GetValueData();
		for (var i = 0; i < p.Length; i++)
		{
			if (p[i] == 0) continue;
			ref var val = ref v[i];
			val = action(k[i], val, state);
		}
	}

	private int _curIndex = 0;

	public void ForEach<TState>(Action<int, T, TState> action, TState state)
	{
		var p = _data.GetPresenceData();
		var k = _data.GetKeyData();
		var v = _data.GetValueData();
		for (_curIndex = 0; _curIndex < p.Length;)
		{
			if (p[_curIndex] == 0)
			{
				_curIndex++;
				continue;
			}

			var key = k[_curIndex];
			var val = v[_curIndex];
			_curIndex++;
			action(key, val, state);
		}
	}

	public T Get(int entityId)
	{
		return _data[entityId];
	}

	public int GetCount()
	{
		return _data.Count;
	}

	public CollectionEnumerable<int> GetEntityIds()
	{
		return new CollectionEnumerable<int>((SparseCollectionView<int>) _data.Keys);
	}

	public CollectionEnumerable<T> GetValues()
	{
		return new CollectionEnumerable<T>((SparseCollectionView<T>) _data.Values);
	}

	public IEntityView GetView()
	{
		return _view;
	}

	public bool RemoveComponentFromStorage(int entityId)
	{
		if (!_data.TryLookup(entityId, out var index)) return false;
		if (index < _curIndex) _curIndex--;
		_data.Remove(entityId);
		return true;
	}

	public bool StoreComponentForEntity(int entityId)
	{
		if (_data.TryLookup(entityId, out _)) return false;
		_data.Add(entityId, default);
		return true;
	}

	public void Update(int entityId, T value)
	{
		_data[entityId] = value;
	}

	public void Update(int entityId, object value)
	{
		_data[entityId] = (T) value;
	}

	public void UpdateCurrent(T value)
	{
		_data.GetValueData()[_curIndex - 1] = value;
	}

#pragma warning disable HAA0601
	public object GetValue(int entityId)
	{
		return _data[entityId];
	}
#pragma warning restore

	private class View : IEntityView
	{
		private readonly SparseLinearDictionaryStorage<T> _storage;

		public View(SparseLinearDictionaryStorage<T> storage)
		{
			_storage = storage;
		}

		public int EntityCount => _storage._data.Count;

		private static readonly Type[] s_filter = new Type[] {typeof(T)};
		public IReadOnlyCollection<Type> Filter => s_filter;

		public bool Contains(int id)
		{
			return _storage._data.ContainsKey(id);
		}

		public Span<int> CopyTo(Span<int> storage)
		{
			var index = 0;
			foreach (var id in GetItems())
			{
				if (index >= storage.Length)
					break;
				storage[index++] = id;
			}

			return storage.Slice(0, index);
		}

		public CollectionEnumerable<int> GetItems()
		{
			return new CollectionEnumerable<int>((SparseCollectionView<int>) _storage._data.Keys);
		}
	}
}