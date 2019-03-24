using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Items
{
	public class BeggerKilt : Kilt, IBeggingAttire
	{
		#region Begging Attire
		public int SetBonus { get { return 5; } set{ } }
		#endregion

		[Constructable]
		public BeggerKilt() : this(0)
		{
		}

		[Constructable]
		public BeggerKilt(int hue)
		{
			Name = "Begging Kilt";
			hue = 1169;
			Weight = 2.0;
			this.SetHue = 1008;

		}

		public override SetItem SetID { get { return SetItem.Hoboware; } }
		public override int Pieces { get { return 4; } }

		public BeggerKilt(Serial serial) : base(serial)
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

	public class BeggerCap : SkullCap, IBeggingAttire
	{
		#region Begging Attire
		public int SetBonus { get { return 5; } set { } }
		#endregion

		[Constructable]
		public BeggerCap() : this(0)
		{
		}

		[Constructable]
		public BeggerCap(int hue)
		{
			Name = "Begger Cap";
			hue = 1169;
			Weight = 1.0;
			this.SetHue = 1008;

		}

		public override SetItem SetID { get { return SetItem.Hoboware; } }
		public override int Pieces { get { return 4; } }

		public BeggerCap(Serial serial) : base(serial)
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

	[FlipableAttribute(0x1517, 0x1518)]
	public class BeggerShirt : Shirt, IBeggingAttire
	{
		#region Begging Attire
		public int SetBonus { get { return 5; } set { } }
		#endregion

		[Constructable]
		public BeggerShirt() : base ()
		{
			Name = "Begger Shirt";
			Weight = 1.0;
			this.SetHue = 1008;
		}

		public override SetItem SetID {	get { return SetItem.Hoboware; }}
		public override int Pieces { get { return 4; }}

		public BeggerShirt(Serial serial) : base(serial)
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

	[FlipableAttribute(0x170d, 0x170e)]
	public class BegSandals : Sandals
	{
		#region Begging Attire
		public int SetBonus { get { return 5; } set { } }
		#endregion

		[Constructable]
		public BegSandals() : this(0)
		{
		}

		[Constructable]
		public BegSandals(int hue) : base()
		{
			Name = "Beggar Sandals";
			Weight = 1.0;
			this.SetHue = 1008;
		}

		public override SetItem SetID { get { return SetItem.Hoboware; } }
		public override int Pieces { get { return 4; } }

		public BegSandals(Serial serial) : base(serial)
		{
		}

		public override bool Dye(Mobile from, DyeTub sender)
		{
			return false;
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
