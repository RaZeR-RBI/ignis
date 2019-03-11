using System;

namespace Ignis
{
    public class EntityIdEventArgs : EventArgs
    {
        public int EntityID { get; }
        public EntityIdEventArgs(int id) => EntityID = id;
    }
}