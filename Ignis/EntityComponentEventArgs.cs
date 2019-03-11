using System;

namespace Ignis
{
    public class EntityComponentEventArgs : EventArgs
    {
        public long EntityID { get; }
        public object ComponentValue { get; }

        public EntityComponentEventArgs(long id, object component) =>
            (this.EntityID, this.ComponentValue) = (id, component);
    }
}