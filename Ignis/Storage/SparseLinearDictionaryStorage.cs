using System;
using System.Collections.Generic;

namespace Ignis.Storage;

public abstract class SparseLinearDictionaryStorageBase<T> : IComponentCollection<T>, IComponentCollectionStorage
	where T : new()
{
	private readonly SparseLinearDictionaryBase<int, T> _data;
	private readonly SparseLinearDictionaryEntityView<T> _view;

	public SparseLinearDictionaryStorageBase(bool useLookup)
	{
		_data = useLookup ?
			new SparseLinearDictionaryWithLookup<int, T>() :
			new SparseLinearDictionary<int, T>();
		_view = new(_data);
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
		var result = _data.Remove(entityId, out var index);
		if (result && index < _curIndex) _curIndex--;
		return result;
	}

	public bool StoreComponentForEntity(int entityId)
	{
		return _data.TryAdd(entityId, default);
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

}

public class SparseLinearDictionaryStorage<T> : SparseLinearDictionaryStorageBase<T>
	where T : new()
{
	public SparseLinearDictionaryStorage() : base(false) {}
}

public class SparseLinearDictionaryWithLookupStorage<T> : SparseLinearDictionaryStorageBase<T>
	where T : new()
{
	public SparseLinearDictionaryWithLookupStorage() : base(true) {}
}