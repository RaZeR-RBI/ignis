#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ignis.Storage;

public abstract class SparseLinearDictionaryBase<TKey, TValue> : IDictionary<TKey, TValue>,
                                                                 ICollection
	where TKey : notnull, IEquatable<TKey>
{
	protected readonly List<byte> _presence;
	protected readonly List<TKey> _keys;
	protected readonly List<TValue> _values;
	protected int _count = 0;
	protected int _emptySlotCount = 0;

	public int Count => _count;
	public int EmptySlots => _emptySlotCount;

	[ExcludeFromCodeCoverage] public object SyncRoot => this;

	[ExcludeFromCodeCoverage] public bool IsSynchronized => false;

	[ExcludeFromCodeCoverage] public bool IsReadOnly => false;

	protected readonly SparseCollectionView<TKey> _keyView;
	protected readonly SparseCollectionView<TValue> _valueView;

	public SparseLinearDictionaryBase(int size = 128)
	{
		_presence = new (size);
		_keys = new (size);
		_values = new (size);
		_keyView = new (this, _presence, _keys);
		_valueView = new (this, _presence, _values);
	}

	public TValue this[TKey key]
	{
		get
		{
			if (!TryLookup(key, out var index))
				throw new KeyNotFoundException("Key is not present in dictionary");
			return _values[index];
		}
		set
		{
			if (!TryLookup(key, out var index))
				Add(key, value);
			else
				_values[index] = value;
		}
	}

	public ICollection<TKey> Keys => _keyView;

	public ICollection<TValue> Values => _valueView;

	[ExcludeFromCodeCoverage]
	public Span<byte> GetPresenceData()
	{
		return CollectionsMarshal.AsSpan(_presence);
	}

	[ExcludeFromCodeCoverage]
	public Span<TKey> GetKeyData()
	{
		return CollectionsMarshal.AsSpan(_keys);
	}

	[ExcludeFromCodeCoverage]
	public Span<TValue> GetValueData()
	{
		return CollectionsMarshal.AsSpan(_values);
	}

	public abstract bool TryLookup(TKey key, out int index);
	public abstract bool TryFindEmptySlot(out int foundIndex);

	public void Clear()
	{
		_presence.Clear();
		_keys.Clear();
		_values.Clear();
		_count = 0;
		OnClear();
	}

	protected abstract void OnClear();

	public abstract bool ContainsKey(TKey key);

	public bool Contains(KeyValuePair<TKey, TValue> kvp)
	{
		return ContainsKey(kvp.Key);
	}

	public void Add(KeyValuePair<TKey, TValue> kvp)
	{
		Add(kvp.Key, kvp.Value);
	}

	public void Add(TKey key, TValue value)
	{
		if (!TryAdd(key, value))
			throw new ArgumentException("Key already exists");
	}

	public bool TryAdd(TKey key, TValue value)
	{
		if (TryLookup(key, out _))
			return false;

		if (_emptySlotCount > 0 && TryFindEmptySlot(out var index))
		{
			_presence[index] = byte.MaxValue;
			_keys[index] = key;
			_values[index] = value;
			_count++;
			_emptySlotCount--;
			OnSet(index, key, value);
			return true;
		}

		// add to end
		Debug.Assert(_emptySlotCount == 0);
		_presence.Add(byte.MaxValue);
		_keys.Add(key);
		_values.Add(value);
		_count++;
		OnSet(_presence.Count - 1, key, value);
		return true;
	}

	public bool Remove(KeyValuePair<TKey, TValue> kvp)
	{
		return Remove(kvp.Key);
	}

	public bool Remove(TKey key)
	{
		return Remove(key, out _);
	}

	public bool Remove(TKey key, out int index)
	{
		if (!TryLookup(key, out index)) return false;
		_presence[index] = 0;
		_keys[index] = default!;
		_values[index] = default!;
		_count--;
		_emptySlotCount++;
		return true;
	}

	protected virtual void OnSet(int index, TKey key, TValue value)
	{
	}

	protected virtual void OnUnset(int index, TKey key, TValue value)
	{
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		value = default!;
		if (!TryLookup(key, out var index)) return false;
		value = _values[index];
		return true;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

#pragma warning disable HAA0601
	[ExcludeFromCodeCoverage]
	IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
	{
		return new Enumerator(this);
	}

	[ExcludeFromCodeCoverage]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}

	public void CopyTo(Array array, int offset)
	{
		foreach (var kvp in this)
		{
			array.SetValue(kvp, offset);
			offset++;
		}
	}
#pragma warning restore

	public void CopyTo(ICollection<KeyValuePair<TKey, TValue>> items)
	{
		foreach (var kvp in this)
			items.Add(kvp);
	}

	public void CopyTo(KeyValuePair<TKey, TValue>[] items, int offset)
	{
		foreach (var kvp in this)
		{
			items[offset] = kvp;
			offset++;
		}
	}

	public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
	{
		private SparseLinearDictionaryBase<TKey, TValue> _owner;

		public KeyValuePair<TKey, TValue> Current { get; private set; }


		private int _i;

		public Enumerator(SparseLinearDictionaryBase<TKey, TValue> owner)
		{
			_owner = owner;
			Current = default!;
			_i = -1;
		}

		public void Reset()
		{
			Current = default!;
			_i = -1;
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			var p = _owner.GetPresenceData();
			if (_i >= p.Length) return false;
			do
			{
				_i++;
				if (_i >= p.Length) return false;
			} while (p[_i] == 0);

			Current = new (_owner.GetKeyData()[_i],
			_owner.GetValueData()[_i]);
			return true;
		}

#pragma warning disable HAA0601
		[ExcludeFromCodeCoverage] object? IEnumerator.Current => Current;
#pragma warning restore
	}
}

public class SparseCollectionView<T> : ICollection<T>
{
	public int Count => _owner.Count;

	public bool IsReadOnly => true;

	private readonly ICollection _owner;
	private readonly List<byte> _presence;
	private readonly List<T> _values;

	public ReadOnlySpan<byte> GetPresenceData()
	{
		return CollectionsMarshal.AsSpan(_presence);
	}

	public ReadOnlySpan<T> GetValueData()
	{
		return CollectionsMarshal.AsSpan(_values);
	}

	public SparseCollectionView(ICollection owner, List<byte> presence, List<T> values)
	{
		_owner = owner;
		_presence = presence;
		_values = values;
	}

	public void Add(T item)
	{
		throw new InvalidOperationException();
	}

	public void Clear()
	{
		throw new InvalidOperationException();
	}

	public bool Contains(T item)
	{
		for (var i = 0; i < _values.Count; i++)
		{
			if (_presence[i] == 0) continue;
			if (EqualityComparer<T>.Default.Equals(_values[i], item)) return true;
		}

		return false;
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		var j = 0;
		for (var i = 0; i < _values.Count; i++)
		{
			if (_presence[i] == 0) continue;
			array[arrayIndex + j] = _values[i];
			j++;
		}
	}

	public bool Remove(T item)
	{
		throw new InvalidOperationException();
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

#pragma warning disable HAA0601
	[ExcludeFromCodeCoverage]
	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return new Enumerator(this);
	}

	[ExcludeFromCodeCoverage]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}
#pragma warning restore

	public struct Enumerator : IEnumerator<T>
	{
		private SparseCollectionView<T> _owner;

		public T Current { get; private set; }


		private int _i;

		public Enumerator(SparseCollectionView<T> owner)
		{
			_owner = owner;
			Current = default!;
			_i = -1;
		}

		public void Reset()
		{
			Current = default!;
			_i = -1;
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			var p = _owner.GetPresenceData();
			if (_i >= p.Length) return false;
			do
			{
				_i++;
				if (_i >= p.Length) return false;
			} while (p[_i] == 0);

			Current = _owner.GetValueData()[_i];
			return true;
		}

#pragma warning disable HAA0601
		[ExcludeFromCodeCoverage] object? IEnumerator.Current => Current;
#pragma warning restore
	}
}