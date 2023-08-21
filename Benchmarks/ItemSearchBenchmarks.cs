#pragma warning disable HAA0301, HAA0302, HAA0303, HAA0401
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using BenchmarkDotNet.Attributes;

namespace Benchmarks;

public class ItemSearchBenchmarks
{
	[Params(100, 10000)] public int N;

	private List<int> _items = null!;

	private int[] _randomIds = null!;

	[GlobalSetup]
	public void Setup()
	{
		_items = new (N);
		for (var i = 0; i < N; i++)
			_items.Add(i);
		_randomIds = new int[100];
		// evenly spaced through all list
		for (var j = 0; j < 100; j++)
			_randomIds[j] = j * (N / 100);
		var rnd = new Random(42);
		_randomIds.AsSpan().Shuffle((min, max) => rnd.Next(min, max));
		ValidateImplementations();
	}

	private void ValidateImplementations()
	{
		var span = CollectionsMarshal.AsSpan(_items);
		foreach (var item in _randomIds.Append(-1))
		{
			var expected = _items.IndexOf(item);
			var actualVectorT = DoVectorTIndexOf(span, item);
			if (expected != actualVectorT)
				throw new Exception("Unexpected result from DoVectorTIndexOf");
		}
	}

	[Benchmark(Baseline = true)]
	public void ListIndexOf()
	{
		var max = _randomIds.Length;
		for (var i = 0; i < max; i++)
		{
			var index = _items.IndexOf(_randomIds[i]);
			if (index < 0)
				throw new Exception("should not happen");
		}
	}

	[Benchmark]
	public void SpanIndexOf()
	{
		var span = CollectionsMarshal.AsSpan(_items);
		var max = _randomIds.Length;
		for (var i = 0; i < max; i++)
		{
			var index = span.IndexOf(_randomIds[i]);
			if (index < 0)
				throw new Exception("should not happen");
		}
	}

	[Benchmark]
	public void VectorTIndexOf()
	{
		var span = CollectionsMarshal.AsSpan(_items);
		var max = _randomIds.Length;
		for (var i = 0; i < max; i++)
		{
			var index = DoVectorTIndexOf(span, _randomIds[i]);
			if (index < 0)
				throw new Exception("should not happen");
		}
	}

	private int DoVectorTIndexOf(ReadOnlySpan<int> items, int value)
	{
		var minSize = Vector<int>.Count;
		Span<int> src = stackalloc int[minSize];
		for (var i = 0; i < minSize; i++)
			src[i] = value;
		var search = new Vector<int>(src);

		var chunk = items;
		var index = 0;
		do
		{
			if (chunk.Length >= minSize)
			{
				var loaded = new Vector<int>(chunk);
				if (!Vector.EqualsAny(loaded, search))
				{
					chunk = chunk[minSize..];
					index += minSize;
					continue;
				}
			}

			for (var i = 0; i < chunk.Length; i++)
			{
				if (chunk[i] != value) continue;
				return index + i;
			}

			return -1;
		} while (true);
	}
}