using System;
using Server;

namespace Server.Items
{
	public class GionvanniRequest : Item
	{
		public override bool Nontransferable { get { return true; } }

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);
			AddQuestItemProperty(list);
		}

		[Constructable]
		public GionvanniRequest() : base(0x14ED)
		{
			Name = "Gionvanni Help Request";
			LootType = LootType.Blessed;
		}

		public GionvanniRequest(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // Version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
