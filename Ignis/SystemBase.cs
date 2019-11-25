using System;
using Ignis.Containers;

namespace Ignis
{
	public abstract class SystemBase : IDisposable
	{
		public IContainer Container { get; }
		public abstract void Execute();
		public virtual void Initialize() { }
		public SystemBase(ContainerProvider ownerProvider) => Container = ownerProvider.GetInstance();

		private bool isDisposed = false;
		protected virtual void OnDispose()
		{

		}

		public virtual void Dispose()
		{
			if (isDisposed) return;
			isDisposed = true;
			OnDispose();
		}
	}
}