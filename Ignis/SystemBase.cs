using System;
using Ignis.Containers;

namespace Ignis
{
	public abstract class SystemBase : IDisposable
	{
		public IContainer Container => _container;
		protected readonly IContainer _container;

		public IEntityManager EntityManager => _em;
		protected readonly IEntityManager _em;

		public abstract void Execute();
		public virtual void Initialize() { }
		public SystemBase(ContainerProvider ownerProvider)
		{
			_container = ownerProvider.GetInstance();
			_em = _container.EntityManager;
		}

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