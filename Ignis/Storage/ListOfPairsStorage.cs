using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace Ignis.Storage
{
public class ListOfPairsStorage<T> : IComponentCollection<T>, IComponentCollectionStorage
	where T : new()
{
	private int _curIndex = 0;
	private readonly ListOfPairsEntityView _view;
	private readonly List<EntityValuePair<T>> _pairs = new List<EntityValuePair<T>>();

	public ListOfPairsStorage()
	{
		_view = new ListOfPairsEntityView(this);
	}

	public void ForEach(Action<int, T> action)
	{
		Reset();
		while (HasNext())
		{
			var value = _pairs[_curIndex];
			_curIndex++;
			action(value.EntityID, value.ComponentValue);
		}
	}

	public T Get(int entityId)
	{
		return _pairs.First(p => p.EntityID == entityId).ComponentValue;
	}

	public IEnumerator<T> GetEnumerator()
	{
		Reset();
		while (HasNext())
		{
			var value = _pairs[_curIndex];
			_curIndex++;
			yield return value.ComponentValue;
		}
	}

	public bool RemoveComponentFromStorage(int entityId)
	{
		for (var i = 0; i < _pairs.Count; i++)
			if (_pairs[i].EntityID == entityId)
			{
				_pairs.RemoveAt(i);
				if (i <= _curIndex)
					_curIndex--;
				return true;
			}

		return false;
	}

	public bool StoreComponentForEntity(int entityId)
	{
		if (_pairs.Any(p => p.EntityID == entityId))
			return false;
		_pairs.Add(new EntityValuePair<T>(entityId));
		return true;
	}

	public void Update(int entityId, T value)
	{
		var entityIndex = _pairs.FindIndex(p => p.EntityID == entityId);
		if (entityIndex == -1) return;
		var pair = _pairs[entityIndex];
		_pairs[entityIndex] = new EntityValuePair<T>(entityId, value);
	}

	public void UpdateCurrent(T value)
	{
		if (_curIndex - 1 >= _pairs.Count || _curIndex < 1) return;
		var curPair = _pairs[_curIndex - 1];
		curPair.ComponentValue = value;
		_pairs[_curIndex - 1] = curPair;
	}

	[ExcludeFromCodeCoverage]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void Reset()
	{
		_curIndex = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool HasNext()
	{
		return _curIndex < _pairs.Count;
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

	[ExcludeFromCodeCoverage]
	private class ListOfPairsEntityView : IEntityView
	{
		private readonly ListOfPairsStorage<T> _storage;

		public ListOfPairsEntityView(ListOfPairsStorage<T> storage)
		{
			_storage = storage;
		}

		public int EntityCount => _storage._pairs.Count;

		private static Type[] s_filter = new Type[] {typeof(T)};
		public IReadOnlyCollection<Type> Filter => s_filter;

		public bool Contains(int id)
		{
			foreach (var pair in _storage._pairs)
				if (pair.EntityID == id)
					return true;
			return false;
		}

		public Span<int> CopyTo(Span<int> storage)
		{
			var index = 0;
			foreach (var id in this)
			{
				if (index >= storage.Length)
					break;
				storage[index++] = id;
			}

			return storage.Slice(0, index);
		}

		public IEnumerator<int> GetEnumerator()
		{
			foreach (var pair in _storage._pairs)
				yield return pair.EntityID;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
}