#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Ignis.Storage;

public class
	SparseLinearDictionaryWithLookup<TKey, TValue> : SparseLinearDictionaryBase<TKey, TValue>
	where TKey : notnull, IEquatable<TKey>
{
	private readonly SortedDictionary<TKey, int> _indexLookup;

	public SparseLinearDictionaryWithLookup(int size = 128) : base(size)
	{
		_indexLookup = new ();
	}

	public override bool TryLookup(TKey key, out int index)
	{
		return _indexLookup.TryGetValue(key, out index);
	}

	public override bool TryFindEmptySlot(out int foundIndex)
	{
		var span = CollectionsMarshal.AsSpan(_presence);
		foundIndex = span.IndexOf((byte) 0);
		return foundIndex >= 0;
	}

	protected override void OnClear()
	{
		_indexLookup.Clear();
	}

	public override bool ContainsKey(TKey key)
	{
		return _indexLookup.ContainsKey(key);
	}

	protected override void OnSet(int index, TKey key, TValue value)
	{
		_indexLookup.Add(key, index);
	}

	protected override void OnUnset(int index, TKey key, TValue value)
	{
		_indexLookup.Remove(key);
	}
}