using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace Ignis.Storage
{
/// <summary>
/// A component storage that's implemented as list of pairs ('entity ID'-'component' tuples).
/// May offer a slight increase in cache locality when <see cref="ForEach(Action<int, T>)" /> is used.
/// Other scenarios may perform worse than the <see cref="DoubleListStorage<T>" />.
/// </summary>
/// <typeparam name="T">Component type</typeparam>
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

	public void Process(Func<int, T, T> action)
	{
		Reset();
		while (HasNext())
		{
			var pair = _pairs[_curIndex];
			var newValue = action(pair.EntityID, pair.ComponentValue);
			pair.ComponentValue = newValue;
			_pairs[_curIndex] = pair;
			_curIndex++;
		}
	}

	public T Get(int entityId)
	{
		foreach (var pair in _pairs)
			if (pair.EntityID == entityId)
				return pair.ComponentValue;
		return default;
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
		foreach (var pair in _pairs)
			if (pair.EntityID == entityId)
				return false;
		_pairs.Add(new EntityValuePair<T>(entityId));
		return true;
	}

	public void Update(int entityId, T value)
	{
		var entityIndex = -1;
		var i = 0;
		foreach (var pair in _pairs)
		{
			if (pair.EntityID == entityId)
			{
				entityIndex = i;
				break;
			}

			i++;
		}

		if (entityIndex == -1) return;
		_pairs[entityIndex] = new EntityValuePair<T>(entityId, value);
	}

	public void UpdateCurrent(T value)
	{
		if (_curIndex - 1 >= _pairs.Count || _curIndex < 1) return;
		var curPair = _pairs[_curIndex - 1];
		curPair.ComponentValue = value;
		_pairs[_curIndex - 1] = curPair;
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

	public int GetCount()
	{
		return _pairs.Count;
	}

	public IEnumerable<T> GetValues()
	{
		foreach (var pair in _pairs)
			yield return pair.ComponentValue;
	}

	public void ForEach<TState>(Action<int, T, TState> action, TState state)
	{
		Reset();
		while (HasNext())
		{
			var value = _pairs[_curIndex];
			_curIndex++;
			action(value.EntityID, value.ComponentValue, state);
		}
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

		private static readonly Type[] s_filter = new Type[] {typeof(T)};
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
			foreach (var pair in _storage._pairs)
			{
				if (index >= storage.Length)
					break;
				storage[index++] = pair.EntityID;
			}

			return storage.Slice(0, index);
		}

		public IEnumerator<int> GetEnumerator()
		{
			foreach (var pair in _storage._pairs)
				yield return pair.EntityID;
		}


#pragma warning disable HAA0401
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
}