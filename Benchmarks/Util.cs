namespace Benchmarks;

public static class Util
{
	public static void Shuffle<T>(this Span<T> list, Func<int, int, int> rngNext)
	{
		var n = list.Length;
		while (n > 1)
		{
			n--;
			var k = rngNext(0, n);
			var value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}
}