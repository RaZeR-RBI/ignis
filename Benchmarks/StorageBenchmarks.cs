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

public enum ActionType
{
	Add100,
	Remove100,
	RemoveAdd100,
	RandomLookup100,
	RandomUpdate100,
	Process,
	ForEach
}

public enum StorageType
{
	DoubleList,
	SparseLinearDictionary
}

[MemoryDiagnoser(false)]
public class StorageBenchmarks
{
	[Params(100, 10000)] public int N;

	[ParamsAllValues] public ActionType Action;
	// [Params(ActionType.RandomLookup100)] public ActionType Action;

	private TestComponent _targetValue = TestComponent.Create();

	private int[] _randomIds = null!;


	private DoubleListStorage<TestComponent> _dblListLookup = new ();
	private DoubleListStorage<TestComponent> _dblListAdd = new ();
	private DoubleListStorage<TestComponent> _dblListRemove = new ();
	private DoubleListStorage<TestComponent> _dblListRemoveAdd = new ();
	private DoubleListStorage<TestComponent> _dblListUpdate = new ();

	private SparseLinearDictionaryStorage<TestComponent> _slDictLookup = new ();
	private SparseLinearDictionaryStorage<TestComponent> _slDictAdd = new ();
	private SparseLinearDictionaryStorage<TestComponent> _slDictRemove = new ();
	private SparseLinearDictionaryStorage<TestComponent> _slDictRemoveAdd = new ();
	private SparseLinearDictionaryStorage<TestComponent> _slDictUpdate = new ();

	private SparseLinearDictionaryWithLookupStorage<TestComponent> _slwlDictLookup = new ();
	private SparseLinearDictionaryWithLookupStorage<TestComponent> _slwlDictAdd = new ();
	private SparseLinearDictionaryWithLookupStorage<TestComponent> _slwlDictRemove = new ();
	private SparseLinearDictionaryWithLookupStorage<TestComponent> _slwlDictRemoveAdd = new ();
	private SparseLinearDictionaryWithLookupStorage<TestComponent> _slwlDictUpdate = new ();

	[GlobalSetup]
	public void Setup()
	{
		_randomIds = new int[100];
		var rnd = new Random(42);
		for (var i = 0; i < _randomIds.Length; i++)
			_randomIds[i] = Math.Min((i + 1) * (N / 100), N - 1); // evenly spaced through all list
		// mix them up
		_randomIds.AsSpan().Shuffle((min, max) => rnd.Next(min, max));
		Console.WriteLine("IDS: " + string.Join(", ", _randomIds));

		FillWithRandom(_dblListLookup, _dblListRemove, _dblListUpdate, _dblListRemoveAdd);
		FillWithRandom(_slDictLookup, _slDictRemove, _slDictUpdate, _slDictRemoveAdd);
		FillWithRandom(_slwlDictLookup, _slwlDictRemove, _slwlDictUpdate, _slwlDictRemoveAdd);
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

	[Benchmark(Baseline = true)]
	public void DoubleList()
	{
		RunSelected(_dblListAdd, _dblListRemove, _dblListLookup, _dblListUpdate, _dblListRemoveAdd);
	}

	[Benchmark]
	public void SparseLinearDictionary()
	{
		RunSelected(_slDictAdd, _slDictRemove, _slDictLookup, _slDictUpdate, _slDictRemoveAdd);
	}

	[Benchmark]
	public void SparseLinearDictionaryWithLookup()
	{
		RunSelected(_slwlDictAdd, _slwlDictRemove, _slwlDictLookup, _slwlDictUpdate,
		            _slwlDictRemoveAdd);
	}

	private void RunSelected<T>(T storageAdd, T storageRemove, T storageLookup, T storageUpdate,
	                            T storageRemoveAdd)
		where T : class, IComponentCollection<TestComponent>, IComponentCollectionStorage
	{
		switch (Action)
		{
		case ActionType.Add100:
			RunAddBenchmark(storageAdd);
			break;
		case ActionType.Remove100:
			RunRemoveBenchmark(storageRemove);
			break;
		case ActionType.RemoveAdd100:
			RunRemoveAddBenchmark(storageRemoveAdd);
			break;
		case ActionType.RandomLookup100:
			RunRandomLookupBenchmark(storageLookup);
			break;
		case ActionType.RandomUpdate100:
			RunRandomUpdateBenchmark(storageUpdate);
			break;
		case ActionType.Process:
			RunProcessBenchmark(storageUpdate);
			break;
		case ActionType.ForEach:
			RunForEachBenchmark(storageUpdate);
			break;
		}
	}

	private void RunAddBenchmark<T>(T storage)
		where T : class, IComponentCollection<TestComponent>, IComponentCollectionStorage
	{
		for (var i = 0; i < _randomIds.Length; i++)
		{
			var id = _randomIds[i];
			storage.StoreComponentForEntity(id);
		}
	}

	private void RunRemoveBenchmark<T>(T storage)
		where T : class, IComponentCollection<TestComponent>, IComponentCollectionStorage
	{
		for (var i = 0; i < _randomIds.Length; i++)
		{
			var id = _randomIds[i];
			storage.RemoveComponentFromStorage(id);
		}
	}

	private void RunRemoveAddBenchmark<T>(T storage)
		where T : class, IComponentCollection<TestComponent>, IComponentCollectionStorage
	{
		for (var i = 0; i < _randomIds.Length / 2; i++)
		{
			var id = _randomIds[i];
			var id2 = _randomIds[i + _randomIds.Length / 2];
			storage.RemoveComponentFromStorage(id);
			storage.StoreComponentForEntity(id2);
		}
	}

	private void RunRandomLookupBenchmark<T>(T storage)
		where T : class, IComponentCollection<TestComponent>, IComponentCollectionStorage
	{
		for (var i = 0; i < _randomIds.Length; i++)
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
		for (var i = 0; i < _randomIds.Length; i++)
		{
			var id = _randomIds[i];
			storage.Update(id, val);
		}
	}

	private void RunProcessBenchmark<T>(T storage)
		where T : class, IComponentCollection<TestComponent>, IComponentCollectionStorage
	{
		storage.Process((id, old, v) => v, _targetValue);
	}

	private void RunForEachBenchmark<T>(T storage)
		where T : class, IComponentCollection<TestComponent>, IComponentCollectionStorage
	{
		storage.ForEach(
		(id, old, s) => s.Storage.UpdateCurrent(s.Value),
		(Storage: storage, Value: _targetValue));
	}
}