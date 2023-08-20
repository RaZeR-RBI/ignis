using System.Collections.Generic;
using System.Linq;
using ConcurrentCollections;
using Ignis.Storage;

namespace Ignis;

public struct CollectionEnumerable<T>
{
	private readonly EnumeratorType _type;
	private readonly ConcurrentHashSet<T> _cset;
	private readonly List<T> _list;
	private readonly SparseCollectionView<T> _sparseView;

	public CollectionEnumerable(ConcurrentHashSet<T> items)
	{
		_cset = items;
		_type = EnumeratorType.ConcurrentHashSet;
		_list = null;
		_sparseView = default;
	}

	public CollectionEnumerable(List<T> items)
	{
		_list = items;
		_type = EnumeratorType.List;
		_cset = null;
		_sparseView = default;
	}

	public CollectionEnumerable(SparseCollectionView<T> items)
	{
		_list = null;
		_type = EnumeratorType.SparseCollectionView;
		_cset = null;
		_sparseView = items;
	}

	public CollectionEnumerator<T> GetEnumerator()
	{
		return _type switch
		{
			EnumeratorType.ConcurrentHashSet => new CollectionEnumerator<T>(_cset),
			EnumeratorType.List => new CollectionEnumerator<T>(_list),
			EnumeratorType.SparseCollectionView => new CollectionEnumerator<T>(_sparseView),
			_ => CollectionEnumerator<T>.Empty
		};
	}

	public IEnumerable<T> AsEnumerable()
	{
		return _type switch
		{
			EnumeratorType.ConcurrentHashSet => _cset,
			EnumeratorType.List => _list,
			EnumeratorType.SparseCollectionView => _sparseView,
			_ => Enumerable.Empty<T>()
		};
	}
}