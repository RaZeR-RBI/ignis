using System;

namespace Ignis
{
    public class EntityComponentEventArgs : EventArgs
    {
        public int EntityID { get; }
        public Type ComponentType { get; }

        public EntityComponentEventArgs(int id, Type component) =>
            (this.EntityID, this.ComponentType) = (id, component);
    }
}