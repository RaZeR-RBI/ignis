using System;

namespace Ignis
{
    public struct EntityComponentEventArgs
    {
        public int EntityID { get; }
        public Type ComponentType { get; }

        public EntityComponentEventArgs(int id, Type component) =>
            (this.EntityID, this.ComponentType) = (id, component);
    }
}