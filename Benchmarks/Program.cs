using BenchmarkDotNet.Running;

namespace Benchmarks;

public class Program
{
	public static void Main(string[] args)
	{
		BenchmarkSwitcher.FromTypes(new Type[] {typeof(StorageBenchmarks)}).Run(args);
	}
}