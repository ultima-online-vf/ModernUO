using System;

namespace Server
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SerializableEntityAttribute : Attribute
    {
        public int Version { get; set; }

        public SerializableEntityAttribute(int version)
        {
            Version = version;
        }
    }
}
