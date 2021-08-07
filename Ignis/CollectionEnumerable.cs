using System.Collections.Generic;
using System.Linq;
using ConcurrentCollections;

namespace Ignis
{
public struct CollectionEnumerable<T>
{
	private readonly EnumeratorType _type;
	private readonly ConcurrentHashSet<T> _cset;
	private readonly List<T> _list;

	public CollectionEnumerable(ConcurrentHashSet<T> items)
	{
		_cset = items;
		_type = EnumeratorType.ConcurrentHashSet;
		_list = null;
	}

	public CollectionEnumerable(List<T> items)
	{
		_list = items;
		_type = EnumeratorType.List;
		_cset = null;
	}

	public CollectionEnumerator<T> GetEnumerator()
	{
		return _type switch
		{
			EnumeratorType.ConcurrentHashSet => new CollectionEnumerator<T>(_cset),
			EnumeratorType.List => new CollectionEnumerator<T>(_list),
			_ => CollectionEnumerator<T>.Empty
		};
	}

	public IEnumerable<T> AsEnumerable()
	{
		return _type switch
		{
			EnumeratorType.ConcurrentHashSet => _cset,
			EnumeratorType.List => _list,
			_ => Enumerable.Empty<T>()
		};
	}
}
}