using System;
using Ignis.Containers;

namespace Ignis
{
    public abstract class SystemBase
    {
        public IContainer Container { get; }
        public abstract void Execute();
        public virtual void Initialize() {}
        public SystemBase(ContainerProvider ownerProvider) => Container = ownerProvider.GetInstance();
    }
}