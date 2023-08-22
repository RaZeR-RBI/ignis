using System;
using System.Collections;
using System.Collections.Generic;
using ConcurrentCollections;
using Ignis.Storage;

namespace Ignis;

internal enum EnumeratorType
{
	None = 0,
	List,
	ConcurrentHashSet,
	SparseArray
}

public struct CollectionEnumerator<T> : IEnumerator<T>, IEnumerator
{
	private readonly EnumeratorType _type;
	private List<T>.Enumerator _listEnum;
	private ConcurrentHashSet<T>.Enumerator _chsEnum;
	private SparseArrayEnumerator<T> _saEnum;

	public static readonly CollectionEnumerator<T> Empty = new CollectionEnumerator<T>();

	public CollectionEnumerator(List<T> items)
	{
		_type = EnumeratorType.List;
		_listEnum = items.GetEnumerator();
		_chsEnum = default;
		_saEnum = default;
	}

	public CollectionEnumerator(ConcurrentHashSet<T> items)
	{
		_type = EnumeratorType.ConcurrentHashSet;
		_chsEnum = items.GetEnumerator();
		_listEnum = default;
		_saEnum = default;
	}

	public CollectionEnumerator(SparseArrayEnumerable<T> items)
	{
		_type = EnumeratorType.SparseArray;
		_chsEnum = default;
		_listEnum = default;
		_saEnum = items.GetEnumerator();
	}

	public T Current
	{
		get
		{
			return _type switch
			{
				EnumeratorType.List => _listEnum.Current,
				EnumeratorType.ConcurrentHashSet => _chsEnum.Current,
				EnumeratorType.SparseArray => _saEnum.Current,
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
		case EnumeratorType.SparseArray:
			_saEnum.Dispose();
			break;
		}
	}

	public bool MoveNext()
	{
		return _type switch
		{
			EnumeratorType.List => _listEnum.MoveNext(),
			EnumeratorType.ConcurrentHashSet => _chsEnum.MoveNext(),
			EnumeratorType.SparseArray => _saEnum.MoveNext(),
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
		case EnumeratorType.SparseArray:
			ResetHelper(_saEnum);
			break;
		}
	}
}