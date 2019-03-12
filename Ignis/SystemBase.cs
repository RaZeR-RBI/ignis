namespace Ignis
{
    public abstract class SystemBase
    {
        IContainer Container { get; }
        public abstract void Execute();
        public SystemBase(IContainer owner) => Container = owner;
    }
}