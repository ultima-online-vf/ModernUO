using System;

namespace Server.Items
{
    [Serializable(1)]
    public partial class TestItem1 : Item
    {
        [SerializableField(1)]
        private int _someProperty;

        // Generated (overrides)
        public TestItem1(Serial serial) : base(serial)
        {
        }

        public override void Deserialize(IGenericReader reader)
        {
        }

        public override void Serialize(IGenericWriter writer)
        {
        }
    }

    [Serializable(1)]
    public partial class TestItem2 : ISerializable
    {
        [SerializableField(1)]
        private int _someProperty;

        public int TypeRef { get; }

        public void SetTypeRef(Type type)
        {
        }

        public void Delete()
        {
            if (Deleted)
            {
                return;
            }
        }

        public bool Deleted { get; }

        // Generated
        public TestItem2(Serial serial)
        {
            Serial = serial;
            SetTypeRef(GetType());
        }

        BufferWriter ISerializable.SaveBuffer { get; set; }

        public Serial Serial { get; }

        public void MarkDirty()
        {
        }

        public void Deserialize(IGenericReader reader)
        {
        }

        public void Serialize(IGenericWriter writer)
        {
        }
    }
}
