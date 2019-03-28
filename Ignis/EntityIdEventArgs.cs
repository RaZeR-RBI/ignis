using System;

namespace Ignis
{
    public struct EntityIdEventArgs
    {
        public int EntityID { get; }
        public EntityIdEventArgs(int id) => EntityID = id;
    }
}