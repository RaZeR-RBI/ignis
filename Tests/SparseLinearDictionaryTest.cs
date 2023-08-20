using System;
using System.Collections.Generic;
using FluentAssertions;
using Ignis.Storage;
using Xunit;

namespace Tests;

public class SparseLinearDictionaryTest
{
	[Fact]
	public void ShouldBehaveLikeNormalDictionary()
	{
		var normal = new Dictionary<int, string>();
		var sparse = new SparseLinearDictionary<int, string>();

		for (var i = 0; i < 64; i++)
		{
			var kvp = new KeyValuePair<int, string>(i, i.ToString());
			normal.Add(kvp.Key, kvp.Value);
			sparse.Add(kvp);
		}

		CheckIfSame(normal, sparse);

		for (var i = 32; i < 64; i++)
		{
			sparse.Remove(i);
			normal.Remove(i);
		}

		CheckIfSame(normal, sparse);

		for (var i = 128; i < 256; i++)
		{
			var kvp = new KeyValuePair<int, string>(i, i.ToString());
			normal.Add(kvp.Key, kvp.Value);
			sparse.Add(kvp);
		}

		CheckIfSame(normal, sparse);

		sparse.Remove(-1).Should().BeFalse();
		sparse.ContainsKey(-1).Should().BeFalse();
		sparse.Keys.Contains(-1).Should().BeFalse();
		sparse.Values.Contains("-1").Should().BeFalse();
		sparse.Contains(new KeyValuePair<int, string>(-1, "-1")).Should().BeFalse();
		Func<string> nonExistingKey = () => sparse[-1];
		Action existingKey = () => sparse.Add(0, "0");
		nonExistingKey.Should().Throw<KeyNotFoundException>();
		existingKey.Should().Throw<ArgumentException>();


		sparse[-1] = "-1";
		sparse.ContainsKey(-1).Should().BeTrue();
		sparse[-1].Should().Be("-1");
		sparse[-1] = "-1";
		sparse.ContainsKey(-1).Should().BeTrue();
		sparse[-1].Should().Be("-1");

		sparse.Keys.Contains(-1).Should().BeTrue();
		sparse.Values.Contains("-1").Should().BeTrue();
		sparse.Remove(new KeyValuePair<int, string>(-1, "-1")).Should().BeTrue();

		sparse.Clear();
		normal.Clear();

		CheckIfSame(normal, sparse);
	}

	private static void CheckIfSame(IDictionary<int, string> normal,
	                                IDictionary<int, string> sparse)
	{
		sparse.Should().BeEquivalentTo(normal);
		sparse.Count.Should().Be(normal.Count);
		sparse.Keys.Should().BeEquivalentTo(normal.Keys);
		sparse.Values.Should().BeEquivalentTo(normal.Values);
		sparse.Keys.Count.Should().Be(normal.Count);
		sparse.Values.Count.Should().Be(normal.Count);

		foreach (var kvp in normal)
		{
			sparse.ContainsKey(kvp.Key).Should().BeTrue();
			sparse.Contains(kvp).Should().BeTrue();
			sparse[kvp.Key].Should().Be(kvp.Value);
		}

		foreach (var kvp in sparse)
		{
			normal.ContainsKey(kvp.Key).Should().BeTrue();
			normal.Contains(kvp).Should().BeTrue();
			normal[kvp.Key].Should().Be(kvp.Value);
		}
	}
}