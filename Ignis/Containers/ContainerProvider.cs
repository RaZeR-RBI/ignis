using System;
using System.Threading;

namespace Ignis.Containers
{
	public class ContainerProvider
	{
		private static object _syncRoot = new object();
		private volatile static IContainer _lastInstance = null;

		public static bool IsBusy() => Monitor.IsEntered(_syncRoot);

		public static void BeginCreation(IContainer instance)
		{
			if (!Monitor.TryEnter(_syncRoot, 500))
				throw new AbandonedMutexException("ContainerProvider is locked");
			_lastInstance = instance;
		}

		public IContainer GetInstance()
		{
			lock (_syncRoot)
			{
				return _lastInstance;
			}
		}

		public static void EndCreation()
		{
			_lastInstance = null;
			Monitor.Exit(_syncRoot);
		}
	}
}