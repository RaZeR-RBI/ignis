using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ignis.Storage;

public unsafe class SparseArrayStorage<T> : IComponentCollection<T>, IComponentCollectionStorage,
                                            IDisposable
	where T : new()
{
	private int* _ids;
	private T[] _values;
	private int _size;
	private readonly bool _valueHasReferences;

	public int Size => _size;

	private int _filledCount = 0;
	private int _curIndex = 0;
	private int _totalCount = 0;
	private bool disposedValue;
	private const int c_align = 64;

	private readonly SparseArrayEnumerable<int> _keyWrapper;
	private readonly SparseArrayEnumerable<T> _valueWrapper;
	private readonly View _view;

	public SparseArrayStorage()
	{
		_size = 128;
		_ids = (int*) NativeMemory.AlignedAlloc(sizeof(int) * (nuint) _size, c_align);
		_values = new T[_size];
		new Span<int>(_ids, _size).Clear();
		_valueHasReferences = RuntimeHelpers.IsReferenceOrContainsReferences<T>();
		_keyWrapper = new (new KeyWrapper(this));
		_valueWrapper = new (new ValueWrapper(this));
		_view = new View(this);
	}

	public void ForEach<TState>(Action<int, T, TState> action, TState state)
	{
		var max = _totalCount;
		var ids = new Span<int>(_ids, max);
		var values = _values.AsSpan();
		for (_curIndex = 0; _curIndex < max;)
		{
			var entityId = ids[_curIndex];
			if (entityId == 0)
			{
				_curIndex++;
				continue;
			}

			var componentValue = values[_curIndex];
			_curIndex++;
			action(entityId, componentValue, state);
		}
	}

	public T Get(int entityId)
	{
		return _values[new Span<int>(_ids, _totalCount).IndexOf(entityId)];
	}

	public int GetCount()
	{
		return _filledCount;
	}

	public CollectionEnumerable<int> GetEntityIds()
	{
		return new CollectionEnumerable<int>(_keyWrapper);
	}

	public CollectionEnumerable<T> GetValues()
	{
		return new CollectionEnumerable<T>(_valueWrapper);
	}

	public IEntityView GetView()
	{
		return _view;
	}

	public void Process(Func<int, T, T> action)
	{
		var count = _totalCount;
		var ids = new Span<int>(_ids, count);
		var values = _values.AsSpan()[..count];
		for (var i = 0; i < count; i++)
		{
			var id = ids[i];
			if (id == 0) continue;
			ref var val = ref values[i];
			val = action(id, val);
		}
	}

	public void Process<TState>(Func<int, T, TState, T> action, TState state)
	{
		var count = _totalCount;
		var ids = new Span<int>(_ids, count);
		var values = _values.AsSpan()[..count];
		for (var i = 0; i < count; i++)
		{
			var id = ids[i];
			if (id == 0) continue;
			ref var val = ref values[i];
			val = action(id, val, state);
		}
	}

	public void Read<TState>(ComponentReader<T, TState> action, TState state)
	{
		var count = _totalCount;
		var ids = new Span<int>(_ids, count);
		var values = _values.AsSpan()[..count];
		for (var i = 0; i < count; i++)
		{
			var id = ids[i];
			if (id == 0) continue;
			ref var val = ref values[i];
			action(id, val, state);
		}
	}

	public bool RemoveComponentFromStorage(int entityId)
	{
		var ids = new Span<int>(_ids, _totalCount);
		var index = ids.IndexOf(entityId);
		if (index < 0) return false;
		_ids[index] = 0;
		if (_valueHasReferences) _values[index] = default;
		_filledCount--;
		return true;
	}

	public bool StoreComponentForEntity(int entityId)
	{
		var ids = new Span<int>(_ids, _totalCount);
		var emptySlotIndex = ids.IndexOf(0);
		if (emptySlotIndex >= 0) // insert
		{
			ids[emptySlotIndex] = entityId;
			_filledCount++;
			return true;
		}

		if (_filledCount == _size) // grow if not enough space
		{
			_size *= 2;
			_ids = (int*) NativeMemory.AlignedRealloc(_ids, sizeof(int) * (nuint) _size, c_align);
			var tmp = new T[_size];
			_values.AsSpan().CopyTo(tmp.AsSpan());
			_values = tmp;
		}

		// add to end
		_ids[_totalCount] = entityId;
		_totalCount++;
		_filledCount++;
		return true;
	}

	public void Update(int entityId, T value)
	{
		_values[new Span<int>(_ids, _totalCount).IndexOf(entityId)] = value;
	}

	public void UpdateCurrent(T value)
	{
		_values[_curIndex - 1] = value;
	}

#pragma warning disable HAA0601
	public object GetValue(int entityId)
	{
		return Get(entityId);
	}

	public void Update(int entityId, object value)
	{
		Update(entityId, (T) value);
	}
#pragma warning restore

	private class KeyWrapper : ISparseArrayView<int>
	{
		private readonly SparseArrayStorage<T> _owner;

		public KeyWrapper(SparseArrayStorage<T> owner)
		{
			_owner = owner;
		}

		Span<int> ISparseArrayView<int>.GetKeyData()
		{
			return new Span<int>(_owner._ids, _owner._totalCount);
		}

		Span<int> ISparseArrayView<int>.GetValueData()
		{
			return new Span<int>(_owner._ids, _owner._totalCount);
		}
	}

	private class ValueWrapper : ISparseArrayView<T>
	{
		private readonly SparseArrayStorage<T> _owner;

		public ValueWrapper(SparseArrayStorage<T> owner)
		{
			_owner = owner;
		}

		Span<int> ISparseArrayView<T>.GetKeyData()
		{
			return new Span<int>(_owner._ids, _owner._totalCount);
		}

		Span<T> ISparseArrayView<T>.GetValueData()
		{
			return _owner._values.AsSpan()[.._owner._totalCount];
		}
	}

	private class View : IEntityView
	{
		private readonly SparseArrayStorage<T> _owner;

		public View(SparseArrayStorage<T> owner)
		{
			_owner = owner;
		}

		public int EntityCount => _owner._filledCount;

		private static readonly Type[] s_filter = new Type[] {typeof(T)};
		public IReadOnlyCollection<Type> Filter => s_filter;

		public bool Contains(int id)
		{
			return new Span<int>(_owner._ids, _owner._totalCount).IndexOf(id) >= 0;
		}

		public Span<int> CopyTo(Span<int> storage)
		{
			var i = 0;
			var ids = GetItems();
			if (storage.Length == 0) return default;
			foreach (var id in ids)
			{
				storage[i] = id;
				i++;
				if (i >= storage.Length) break;
			}

			return storage[..i];
		}

		public CollectionEnumerable<int> GetItems()
		{
			return _owner.GetEntityIds();
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing) _values = null;

			NativeMemory.AlignedFree(_ids);
			disposedValue = true;
		}
	}

	~SparseArrayStorage()
	{
		Dispose(false);
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}