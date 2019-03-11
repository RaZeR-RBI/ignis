namespace Tests
{
    public struct SampleComponent
    {
        public float Foo;
        public bool Bar;

        public override string ToString() => $"Foo: {Foo}, Bar: {Bar}";
    }
}