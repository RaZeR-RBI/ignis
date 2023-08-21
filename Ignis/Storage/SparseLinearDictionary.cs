#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Ignis.Storage;

public class SparseLinearDictionary<TKey, TValue> : SparseLinearDictionaryBase<TKey, TValue>
	where TKey : notnull
{
	public SparseLinearDictionary(int size = 128) : base(size)
	{
	}

	public override bool TryLookup(TKey key, out int index)
	{
		index = _keys.IndexOf(key);
		return index >= 0;
	}

	public override bool TryFindEmptySlot(out int foundIndex)
	{
		foundIndex = _presence.IndexOf(0);
		return foundIndex >= 0;
	}

	protected override void OnClear()
	{
	}

	public override bool ContainsKey(TKey key)
	{
		return TryLookup(key, out _);
	}

	protected override void OnSet(int index, TKey key, TValue value)
	{
	}

	protected override void OnUnset(int index, TKey key, TValue value)
	{
	}
}