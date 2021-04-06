using System;

namespace Tests
{
public struct SampleComponent
{
	public float Foo;
	public bool Bar;

	public override string ToString()
	{
		return $"Foo: {Foo}, Bar: {Bar}";
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Foo, Bar);
	}

	private bool Equals(SampleComponent other)
	{
		return Foo == other.Foo && Bar == other.Bar;
	}

	public override bool Equals(object obj)
	{
		if (obj is SampleComponent component)
			return Equals(component);
		return false;
	}
}
}