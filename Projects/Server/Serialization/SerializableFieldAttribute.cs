using System;

namespace Server
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SerializableFieldAttribute : Attribute
    {
        public int Order { get; set; }
        public Action<ISerializable, string, object> AfterSet { get; set; }

        public SerializableFieldAttribute(int order, Action<ISerializable, string, object> afterSet = null)
        {
            Order = order;
            AfterSet = afterSet;
        }
    }
}
