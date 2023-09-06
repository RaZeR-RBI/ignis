using System;
using System.Collections;
using System.Collections.Generic;

namespace Ignis.Storage;

public unsafe interface ISparseArrayView<T>
{
	Span<int> GetKeyData();
	Span<T> GetValueData();
}

public unsafe struct SparseArrayEnumerator<T> : IEnumerator<T>
{
	public T Current { get; private set; }

	private ISparseArrayView<T> _view;
	private int _i;

	public SparseArrayEnumerator(ISparseArrayView<T> view)
	{
		Current = default;
		_view = view;
		_i = -1;
	}

#pragma warning disable HAA0601
	object IEnumerator.Current => Current;
#pragma warning restore

	public void Dispose()
	{
		_view = null;
		Current = default;
	}

	public bool MoveNext()
	{
		var p = _view.GetKeyData();
		if (_i >= p.Length) return false;
		do
		{
			_i++;
			if (_i >= p.Length) return false;
		} while (p[_i] == 0);

		Current = _view.GetValueData()[_i];
		return true;
	}

	public void Reset()
	{
		Current = default;
		_i = -1;
	}
}

public class SparseArrayEnumerable<T> : IEnumerable<T>
{
	private ISparseArrayView<T> _view;

	public SparseArrayEnumerable(ISparseArrayView<T> view)
	{
		_view = view;
	}

	public SparseArrayEnumerator<T> GetEnumerator()
	{
		return new SparseArrayEnumerator<T>(_view);
	}

#pragma warning disable HAA0601
	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
#pragma warning restore
}