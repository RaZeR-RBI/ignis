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
		unchecked
		{
			var hash = 17;
			hash = hash * 23 + Foo.GetHashCode();
			hash = hash * 23 + Bar.GetHashCode();
			return hash;
		}
	}

	private bool Equals(SampleComponent other)
	{
		return Foo == other.Foo && Bar == other.Bar;
	}

	public override bool Equals(object obj)
	{
		if (obj is SampleComponent)
			return Equals((SampleComponent) obj);
		return false;
	}
}
}