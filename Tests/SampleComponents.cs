using System;

namespace Tests
{
	public struct SampleComponent
	{
		public float Foo;
		public bool Bar;

		public override string ToString() => $"Foo: {Foo}, Bar: {Bar}";

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + Foo.GetHashCode();
				hash = hash * 23 + Bar.GetHashCode();
				return hash;
			}
		}

		private bool Equals(SampleComponent other) =>
		    (this.Foo == other.Foo) && (this.Bar == other.Bar);

		public override bool Equals(object obj)
		{
			if (obj is SampleComponent)
				return Equals((SampleComponent)obj);
			return false;
		}
	}
}