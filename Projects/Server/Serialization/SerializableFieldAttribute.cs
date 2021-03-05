using System;

namespace Server
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SerializableFieldAttribute : Attribute
    {
        public int Order { get; set; }

        public SerializableFieldAttribute(int order)
        {
            Order = order;
        }
    }
}
