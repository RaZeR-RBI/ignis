using System;

namespace Ignis
{
    public class EntityIdEventArgs : EventArgs
    {
        public long EntityID { get; }
        public EntityIdEventArgs(long id) => EntityID = id;
    }
}