#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Ignis.Storage;

public class SparseLinearDictionaryWithLookup<TKey, TValue> : SparseLinearDictionaryBase<TKey, TValue>
	where TKey : notnull
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
		foundIndex = -1;

		var presence = CollectionsMarshal.AsSpan(_presence);
		var chunk = presence;
		var index = 0;
		var minSize = Vector<byte>.IsSupported ? Vector<byte>.Count : -1;
		do
		{
			if (Vector<byte>.IsSupported && chunk.Length >= minSize)
			{
				var loaded = new Vector<byte>(chunk);
				if (Vector.GreaterThanAll(loaded, Vector<byte>.Zero))
				{
					// all slots occupied
					chunk = chunk[minSize..];
					index += minSize;
					continue;
				}
			}

			for (var i = 0; i < chunk.Length; i++)
			{
				if (chunk[i] > 0) continue;
				// found empty space
				foundIndex = index + i;
				return true;
			}

			break;
		} while (true);

		return false;
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