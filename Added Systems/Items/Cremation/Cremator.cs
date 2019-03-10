using Server.Engines.Craft;
using Server.Targeting;


namespace Server.Items
{
	[Flipable(0x2DDB, 0x2DDC)]
	public class CrematorOven : Item
	{
		[Constructable]
		public CrematorOven() : base(0x2DDB)
		{
			Name = "Cremator";
			Weight = 150.0;
		}

		public CrematorOven(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}
	public class CrematorWood : Item
	{
		[Constructable]
		public CrematorWood() : base(0x1BDE)
		{
			Name = "Treated Wood";
			Weight = 1.0;
		}

		public CrematorWood(Serial serial) : base(serial)
		{

		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();

		}
	}
}
