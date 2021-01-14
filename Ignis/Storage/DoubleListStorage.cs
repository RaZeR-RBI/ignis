using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Ignis.Storage
{
public class DoubleListStorage<T> : IComponentCollection<T>, IComponentCollectionStorage
	where T : new()
{
	private int _curIndex = 0;
	private readonly DoubleListStorageView _view;
	private readonly List<int> _ids = new List<int>();
	private readonly List<T> _values = new List<T>();

	public DoubleListStorage()
	{
		_view = new DoubleListStorageView(this);
	}


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool HasNext()
	{
		return _curIndex < _ids.Count;
	}

	public IEnumerator<T> GetEnumerator()
	{
		Reset();
		while (HasNext())
		{
			var value = _values[_curIndex];
			_curIndex++;
			yield return value;
		}
	}

	public bool RemoveComponentFromStorage(int entityId)
	{
		var entityIndex = _ids.IndexOf(entityId);
		if (entityIndex == -1) return false;
		_ids.RemoveAt(entityIndex);
		_values.RemoveAt(entityIndex);
		if (entityIndex <= _curIndex)
			_curIndex--;
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void Reset()
	{
		_curIndex = 0;
	}

	public bool StoreComponentForEntity(int entityId)
	{
		if (_ids.Contains(entityId)) return false;
		_ids.Add(entityId);
		_values.Add(new T());
		return true;
	}

	public void Update(int entityId, T value)
	{
		var entityIndex = _ids.IndexOf(entityId);
		if (entityIndex == -1) return;
		_values[entityIndex] = value;
	}

	public void UpdateCurrent(T value)
	{
		if (_curIndex - 1 >= _ids.Count || _curIndex < 1) return;
		_values[_curIndex - 1] = value;
	}

	public void ForEach(Action<int, T> action)
	{
		Reset();
		while (HasNext())
		{
			var entityId = _ids[_curIndex];
			var componentValue = _values[_curIndex];
			_curIndex++;
			action(entityId, componentValue);
		}
	}

	public T Get(int entityId)
	{
		return _values[_ids.IndexOf(entityId)];
	}

	[ExcludeFromCodeCoverage]
	public void Update(int entityId, object value)
	{
		Update(entityId, (T) value);
	}

	[ExcludeFromCodeCoverage]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEntityView GetView()
	{
		return _view;
	}

	[ExcludeFromCodeCoverage]
	private class DoubleListStorageView : IEntityView
	{
		private readonly DoubleListStorage<T> _storage;

		public DoubleListStorageView(DoubleListStorage<T> storage)
		{
			_storage = storage;
		}

		public int EntityCount => _storage._ids.Count;

		private static Type[] s_filter = new Type[] {typeof(T)};
		public IReadOnlyCollection<Type> Filter => s_filter;

		public bool Contains(int id)
		{
			return _storage._ids.Contains(id);
		}

		public Span<int> CopyTo(Span<int> storage)
		{
			var index = 0;
			foreach (var id in _storage._ids)
			{
				if (index >= storage.Length)
					break;
				storage[index++] = id;
			}

			return storage.Slice(0, index);
		}

		public IEnumerator<int> GetEnumerator()
		{
			return _storage._ids.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
}