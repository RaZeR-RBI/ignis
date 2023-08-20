#pragma warning disable HAA0101, HAA0301, HAA0302, HAA0303
using Ignis;
using Ignis.Storage;
using BenchmarkDotNet.Attributes;

namespace Benchmarks;

public unsafe struct TestComponent
{
	public Guid A, B, C, D; // 64 bytes of data

	public static TestComponent Create()
	{
		return new TestComponent
		{
			A = Guid.NewGuid(),
			B = Guid.NewGuid(),
			C = Guid.NewGuid(),
			D = Guid.NewGuid()
		};
	}

	public bool Equals(TestComponent other)
	{
		return A == other.A && B == other.B && C == other.C && D == other.D;
	}
}

[MemoryDiagnoser(false)]
public class StorageBenchmarks
{
	[Params(100, 1000, 10000)] public int N;

	private const int c_operationN = 100;

	private int[] _randomIds = null!;


	private DoubleListStorage<TestComponent> _dblListLookup = new ();
	private DoubleListStorage<TestComponent> _dblListAdd = new ();
	private DoubleListStorage<TestComponent> _dblListRemove = new ();
	private DoubleListStorage<TestComponent> _dblListUpdate = new ();

	private SparseLinearDictionaryStorage<TestComponent> _slDictLookup = new ();
	private SparseLinearDictionaryStorage<TestComponent> _slDictAdd = new ();
	private SparseLinearDictionaryStorage<TestComponent> _slDictRemove = new ();
	private SparseLinearDictionaryStorage<TestComponent> _slDictUpdate = new ();

	[GlobalSetup]
	public void Setup()
	{
		_randomIds = new int[N];
		var rnd = new Random(42);
		for (var i = 0; i < N; i++)
			_randomIds[i] = i + 1;
		_randomIds.AsSpan().Shuffle((min, max) => rnd.Next(min, max));

		FillWithRandom(_dblListLookup, _dblListRemove, _dblListUpdate);
		FillWithRandom(_slDictLookup, _slDictRemove, _slDictUpdate);
	}

	private void FillWithRandom<T>(params T[] storages)
		where T : class, IComponentCollection<TestComponent>, IComponentCollectionStorage
	{
		foreach (var storage in storages)
			FillWithRandom(storage);
	}

	private void FillWithRandom<T>(T storage)
		where T : class, IComponentCollection<TestComponent>, IComponentCollectionStorage
	{
		for (var i = 0; i < N; i++)
		{
			var id = i + 1;
			storage.StoreComponentForEntity(id);
			storage.Update(id, TestComponent.Create());
		}
	}

	[Benchmark]
	public void DoubleListAdd()
	{
		RunAddBenchmark(_dblListAdd);
	}

	[Benchmark]
	public void DoubleListRemove()
	{
		RunRemoveBenchmark(_dblListRemove);
	}

	[Benchmark]
	public void DoubleListRandomLookup()
	{
		RunRandomLookupBenchmark(_dblListLookup);
	}

	[Benchmark]
	public void DoubleListRandomUpdate()
	{
		RunRandomUpdateBenchmark(_dblListUpdate);
	}

	[Benchmark]
	public void DoubleListProcess()
	{
		RunProcessBenchmark(_dblListUpdate);
	}

	[Benchmark]
	public void DoubleListForEach()
	{
		RunForEachBenchmark(_dblListUpdate);
	}

	[Benchmark]
	public void SparseLinearDictionaryAdd()
	{
		RunAddBenchmark(_slDictAdd);
	}

	[Benchmark]
	public void SparseLinearDictionaryRemove()
	{
		RunRemoveBenchmark(_slDictRemove);
	}

	[Benchmark]
	public void SparseLinearDictionaryRandomLookup()
	{
		RunRandomLookupBenchmark(_slDictLookup);
	}

	[Benchmark]
	public void SparseLinearDictionaryRandomUpdate()
	{
		RunRandomUpdateBenchmark(_slDictUpdate);
	}

	[Benchmark]
	public void SparseLinearDictionaryProcess()
	{
		RunProcessBenchmark(_slDictUpdate);
	}

	[Benchmark]
	public void SparseLinearDictionaryForEach()
	{
		RunForEachBenchmark(_slDictUpdate);
	}

	private void RunAddBenchmark<T>(T storage)
		where T : class, IComponentCollection<TestComponent>, IComponentCollectionStorage
	{
		for (var i = 0; i < c_operationN; i++)
		{
			var id = _randomIds[i];
			storage.StoreComponentForEntity(id);
		}
	}

	private void RunRemoveBenchmark<T>(T storage)
		where T : class, IComponentCollection<TestComponent>, IComponentCollectionStorage
	{
		for (var i = 0; i < c_operationN; i++)
		{
			var id = _randomIds[i];
			storage.RemoveComponentFromStorage(id);
		}
	}

	private void RunRandomLookupBenchmark<T>(T storage)
		where T : class, IComponentCollection<TestComponent>, IComponentCollectionStorage
	{
		for (var i = 0; i < c_operationN; i++)
		{
			var id = _randomIds[i];
			var item = storage.Get(id);
			if (item.A == default) throw new Exception("should not happen");
		}
	}

	private void RunRandomUpdateBenchmark<T>(T storage)
		where T : class, IComponentCollection<TestComponent>, IComponentCollectionStorage
	{
		var val = TestComponent.Create();
		for (var i = 0; i < c_operationN; i++)
		{
			var id = _randomIds[i];
			storage.Update(id, val);
		}
	}

	private void RunProcessBenchmark<T>(T storage)
		where T : class, IComponentCollection<TestComponent>, IComponentCollectionStorage
	{
		var val = TestComponent.Create();
		storage.Process((id, v) => val);
	}

	private void RunForEachBenchmark<T>(T storage)
		where T : class, IComponentCollection<TestComponent>, IComponentCollectionStorage
	{
		var val = TestComponent.Create();
		storage.ForEach(
		(id, old, s) => s.Storage.UpdateCurrent(s.Value),
		(Storage: storage, Value: val));
	}
}