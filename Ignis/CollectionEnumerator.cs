using System;
using System.Collections;
using System.Collections.Generic;
using ConcurrentCollections;

namespace Ignis
{
internal enum EnumeratorType
{
	None = 0,
	List,
	ConcurrentHashSet
}

public struct CollectionEnumerator<T> : IEnumerator<T>, IEnumerator
{
	private readonly EnumeratorType _type;
	private List<T>.Enumerator _listEnum;
	private ConcurrentHashSet<T>.Enumerator _chsEnum;

	public static readonly CollectionEnumerator<T> Empty = new CollectionEnumerator<T>();

	public CollectionEnumerator(List<T> items)
	{
		_type = EnumeratorType.List;
		_listEnum = items.GetEnumerator();
		_chsEnum = default;
	}

	public CollectionEnumerator(ConcurrentHashSet<T> items)
	{
		_type = EnumeratorType.ConcurrentHashSet;
		_chsEnum = items.GetEnumerator();
		_listEnum = default;
	}

	public T Current
	{
		get
		{
			return _type switch
			{
				EnumeratorType.List => _listEnum.Current,
				EnumeratorType.ConcurrentHashSet => _chsEnum.Current,
				_ => throw new InvalidOperationException()
			};
		}
	}

	object IEnumerator.Current => (object) Current;


	public void Dispose()
	{
		switch (_type)
		{
		case EnumeratorType.List:
			_listEnum.Dispose();
			break;
		case EnumeratorType.ConcurrentHashSet:
			_chsEnum.Dispose();
			break;
		}
	}

	public bool MoveNext()
	{
		return _type switch
		{
			EnumeratorType.List => _listEnum.MoveNext(),
			EnumeratorType.ConcurrentHashSet => _chsEnum.MoveNext(),
			_ => false
		};
	}

	private void ResetHelper<U>(U enumerator)
		where U : IEnumerator
	{
		enumerator.Reset();
	}

	void IEnumerator.Reset()
	{
		switch (_type)
		{
		case EnumeratorType.List:
			ResetHelper(_listEnum);
			break;
		case EnumeratorType.ConcurrentHashSet:
			ResetHelper(_chsEnum);
			break;
		}
	}
}
}