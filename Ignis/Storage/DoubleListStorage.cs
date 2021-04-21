using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

/// <summary>
/// Contains various <see cref="Ignis.IComponentCollection<T>" /> implementations
/// and related types.
/// </summary>
namespace Ignis.Storage
{
/// <summary>
/// A component storage implemented by using two lists - one for entity IDs and
/// one for component values. It's a default implementation when specific storage
/// type is not supplied.
/// </summary>
/// <typeparam name="T">Component type</typeparam>
/// <seealso cref="Ignis.IContainer<TState>.AddComponent{TComponent}" />
/// <seealso cref="Ignis.IContainer<TState>.AddComponent{TComponent, TStorage}" />
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

	public IEntityView GetView()
	{
		return _view;
	}

	public int GetCount()
	{
		return _ids.Count;
	}

	public IEnumerable<T> GetValues()
	{
		return _values;
	}

	public void ForEach<TState>(Action<int, T, TState> action, TState state)
	{
		Reset();
		while (HasNext())
		{
			var entityId = _ids[_curIndex];
			var componentValue = _values[_curIndex];
			_curIndex++;
			action(entityId, componentValue, state);
		}
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

		private static readonly Type[] s_filter = new Type[] {typeof(T)};
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

#pragma warning disable HAA0401 // list's enumerator is a struct, should be no allocations here
		public IEnumerator<int> GetEnumerator()
		{
			return ((IEnumerable<int>) _storage._ids).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
}