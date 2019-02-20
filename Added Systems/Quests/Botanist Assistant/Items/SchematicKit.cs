using System;
using Server;
using Server.Mobiles;
using Server.Targeting;
using Server.Engines.Quests;

namespace Server.Items
{
	public class SchematicKit : Item
	{
		[Constructable]
		public SchematicKit() : base(0xFC1)
		{
			LootType = LootType.Blessed;

			Weight = 1.0;
		}

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);
			AddQuestItemProperty(list);
		}

		public SchematicKit(Serial serial) : base(serial)
		{
		}

		
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
