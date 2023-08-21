using System;
using System.Collections.Generic;
using Ignis;

namespace Ignis.Storage;

public class SparseLinearDictionaryEntityView<T> : IEntityView
{
	private readonly SparseLinearDictionaryBase<int, T> _data;

	public SparseLinearDictionaryEntityView(SparseLinearDictionaryBase<int, T> data)
	{
		_data = data;
	}

	public int EntityCount => _data.Count;

	private static readonly Type[] s_filter = new Type[] { typeof(T) };
	public IReadOnlyCollection<Type> Filter => s_filter;

	public bool Contains(int id)
	{
		return _data.ContainsKey(id);
	}

	public Span<int> CopyTo(Span<int> storage)
	{
		var index = 0;
		foreach (var id in GetItems())
		{
			if (index >= storage.Length)
				break;
			storage[index++] = id;
		}

		return storage.Slice(0, index);
	}

	public CollectionEnumerable<int> GetItems()
	{
		return new CollectionEnumerable<int>((SparseCollectionView<int>)_data.Keys);
	}
}