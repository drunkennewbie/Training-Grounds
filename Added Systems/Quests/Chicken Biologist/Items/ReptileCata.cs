using System;
using Server;
using Server.Items;

	public class CataMortar : Item
	{
		public static Type[] ScaleTypes = new Type[] //List of Scales
		{
			typeof(RedScales),
			typeof(BlueScales),
			typeof(BlackScales),
			typeof(GreenScales),
			typeof(WhiteScales),
			typeof(YellowScales)
		
		};


		private bool _Full;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Full { get	{ return _Full;	} set { if (value) { FillVial(); } } }

		private int _FungusCount;

		[CommandProperty(AccessLevel.GameMaster)]
		public int FungusCount { get { return _FungusCount;	} set { _FungusCount = value; CheckFilled(null); }}

		private int __ReptileScalesCount;

		[CommandProperty(AccessLevel.GameMaster)]
		public int ReptileScalesCount { get { return __ReptileScalesCount; } set { __ReptileScalesCount = value; CheckFilled(null); }}

		private int _BottleCount;

		[CommandProperty(AccessLevel.GameMaster)]
		public int BottleCount { get { return _BottleCount; } set { _BottleCount = value; CheckFilled(null); } }


		[Constructable]
		public CataMortar() : base(0x0E9B)
		{
			Name = "";
			Hue = 57;
			Weight = 1.0;
		}

		public CataMortar(Serial serial) : base(serial)
		{
		}

		public override void OnSingleClick(Mobile from)
		{

			if (FungusCount <= 0 && ReptileScalesCount <= 0)
			{
				LabelTo(from, "a Speciality Mortar and Pestle");
				return;
			}

			if (FungusCount >= 10 && ReptileScalesCount >= 2)
			{
				LabelTo(from, "a bottle of unknown liquid");
				return;
			}

			LabelTo(from, "a mortar and pestle with {0}/10 Zoogi Fungus and {1}/2 Reptile Scales", FungusCount, ReptileScalesCount);
			
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (from == null || from.Backpack == null)
			{
				return;
			}

			if (Full)
			{
				from.SendMessage("The bottle is full of unknown liquid.");
				return;
			}

			if (FungusCount < 10)
			{
				var needs = 10 - FungusCount;

				var consumed = from.Backpack.ConsumeUpTo(typeof(ZoogiFungus), needs);

				FungusCount += consumed;
				CheckFilled(from);
			}

			if (ReptileScalesCount < 2)
			{
				for (int i = 1; i < ScaleTypes.Length && ReptileScalesCount < 2; i++)
				{

					var needs = 2 - ReptileScalesCount;
					var consumed = from.Backpack.ConsumeUpTo(ScaleTypes[i], needs);

					ReptileScalesCount += consumed;
				}
			CheckFilled(from);
			}

			if (BottleCount < 1)
			{
				var needs = 1 - BottleCount;

				var consumed = from.Backpack.ConsumeUpTo(typeof(Bottle), needs);

				BottleCount += consumed;
				CheckFilled(from);
			}
		}

		void CheckFilled(Mobile m)
		{
			if (FungusCount < 10)
			{
				return;
			}

			if (ReptileScalesCount < 2)
			{
				return;
			}

			if (BottleCount < 1)
			{
				return;
			}

			if (m != null)
			{
				m.SendMessage("You follow the instructions and mix the items together and create a bottle of unknown liquid. Return it to the Biologist!");
			}

			FillVial();
		}

		void FillVial()
		{
			Name = "a bottle of unknown liquid";
			Hue = 43;
			Weight = 1.0;
			ItemID = 0x1844;

			// do not set this with the property or it will stack overflow
			_Full = true;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.Write(_Full);
			writer.Write(_FungusCount);
			writer.Write(__ReptileScalesCount);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						_Full = reader.ReadBool();
						_FungusCount = reader.ReadInt();
						__ReptileScalesCount = reader.ReadInt();
						break;
					}
			}
		}
	}
