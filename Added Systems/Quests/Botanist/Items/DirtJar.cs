
namespace Server.Items
{

	public class DirtJar : Item
	{
		private bool _Full;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Full
		{
			get
			{
				return _Full;
			}
			set
			{
				if (value)
				{
					FillJar();
				}
			}
		}

		private int _WormCount;

		[CommandProperty(AccessLevel.GameMaster)]
		public int WormCount
		{
			get
			{
				return _WormCount;
			}
			set
			{
				_WormCount = value;
				CheckFilled(null);
			}
		}

		private int _FertileDirtCount;

		[CommandProperty(AccessLevel.GameMaster)]
		public int FertileDirtCount
		{
			get
			{
				return _FertileDirtCount;
			}
			set
			{
				_FertileDirtCount = value;
				CheckFilled(null);
			}
		}

		[Constructable]
		public DirtJar() : /*base(0x1005)*/ base(0x22C5)
		{
			Name = "";
			Hue = 1113;
			Weight = 9.0;
		}

		public DirtJar(Serial serial) : base(serial)
		{
		}

		public override void OnSingleClick(Mobile from)
		{
			//base.OnSingleClick(from);

			if (WormCount <= 0 && FertileDirtCount <= 0)
			{
				LabelTo(from, "an empty jar");
				return;
			}

			if (WormCount >= 5 && FertileDirtCount >= 10)
			{
				LabelTo(from, "a full jar");
				return;
			}

			LabelTo(from, "a jar with {0}/5 worms and {1}/10 fertile dirt", WormCount, FertileDirtCount);
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (from == null || from.Backpack == null)
			{
				return;
			}

			if (Full)
			{
				from.SendMessage("This jar is completely full.");
				return;
			}

			if (WormCount < 5)
			{
				var needs = 5 - WormCount;

				var consumed = from.Backpack.ConsumeUpTo(typeof(Worm), needs);

				WormCount += consumed;
				CheckFilled(from);
			}

			if (FertileDirtCount < 10)
			{
				var needs = 10 - FertileDirtCount;

				var consumed = from.Backpack.ConsumeUpTo(typeof(FertileDirt), needs);

				FertileDirtCount += consumed;
				CheckFilled(from);
			}
		}

		void CheckFilled(Mobile m)
		{
			if (WormCount < 5)
			{
				return;
			}

			if (FertileDirtCount < 10)
			{
				return;
			}

			if (m != null)
			{
				m.SendMessage("The jar is full, return it to the botanist for your reward!");
			}

			FillJar();
		}

		void FillJar()
		{
			Name = "a jar full of dirt and worms";
			Hue = 1190;
			Weight = 9.0;

			// do not set this with the property or it will stack overflow
			_Full = true;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.Write(_Full);
			writer.Write(_WormCount);
			writer.Write(_FertileDirtCount);
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
						_WormCount = reader.ReadInt();
						_FertileDirtCount = reader.ReadInt();
						break;
					}
			}
		}
	}
}
