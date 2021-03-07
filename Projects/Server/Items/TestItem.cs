using System;

namespace Server.Items
{
    [SerializableEntity(2)]
    public partial class TestItem : Item
    {
        [SerializableField(1)]
        private int _someProperty;

        public static void Initialize()
        {
            var obj = new TestItem(0x1000u);
            Console.WriteLine("Worked! {0} {1}", _version, obj.SomeProperty);
        }
    }
}
