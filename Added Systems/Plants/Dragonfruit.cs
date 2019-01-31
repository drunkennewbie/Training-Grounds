using System;
using Server;

namespace Server.Items
{
	public class DragonFruit : Item
	{
		public override string DefaultName
		{
			get
			{
				return "dragon fruit";
			}
		}

		[Constructable]
		public DragonFruit() : base(3173)
		{
			Weight = 1.0;
			Hue = 1172;
		}

		public DragonFruit(Serial serial)
			: base(serial)
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.ReadInt();
		}
	}
}
