using System.Threading;

namespace Ignis.Containers
{
    public class ContainerProvider
    {
        private static ManualResetEvent _mre = new ManualResetEvent(true);
        private static object _syncRoot = new object();
        private volatile static IContainer _lastInstance = null;

        public static void BeginCreation(IContainer instance)
        {
            lock (_syncRoot)
            {
                _mre.Reset();
                _lastInstance = instance;
            }
        }

        public IContainer GetInstance() => _lastInstance;

        public static void EndCreation()
        {
            lock (_syncRoot)
            {
                _lastInstance = null;
                _mre.Set();
            }
        }
    }
}