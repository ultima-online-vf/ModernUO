using System;

namespace Server
{
    /// <summary>
    /// Marks a property as serializable. Requires a call to ISerializable.MarkDirty()
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SerializablePropertyAttribute : Attribute
    {
        public int Order { get; }

        public SerializablePropertyAttribute(int order) => Order = order;
    }
}
