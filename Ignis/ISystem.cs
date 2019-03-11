namespace Ignis
{
    public interface ISystem
    {
        IEntityManager EntityManager { get; }
        void Execute();
        bool DependsOnComponent<T>() where T : struct;
    }
}