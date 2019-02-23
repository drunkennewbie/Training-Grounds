namespace Server.Items
{
	public class BeggerCoins : Item
	{
		public override double DefaultWeight
		{
			get { return 0.02; }
		}

		[Constructable]
		public BeggerCoins() : this(1)
		{
		}

		[Constructable]
		public BeggerCoins(int amountFrom, int amountTo) : this(Utility.RandomMinMax(amountFrom, amountTo))
		{
		}

		[Constructable]
		public BeggerCoins(int amount) : base(0xEF0)
		{
			Hue = 0x096D;
			Name = "Dull Silver";
			Stackable = true;
			Amount = amount;
		}

		public BeggerCoins(Serial serial) : base(serial)
		{
		}

		public override int GetDropSound()
		{
			if (Amount <= 1)
				return 0x2E4;
			else if (Amount <= 5)
				return 0x2E5;
			else
				return 0x2E6;
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
