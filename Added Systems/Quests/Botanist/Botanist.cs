using Server.Items;
using Server.ContextMenus;
using Server.Gumps;
using Server.Network;
using System.Collections.Generic;
using Server.Engines.Plants;

namespace Server.Mobiles
{
	[CorpseName("Botanist's Corpse")]
	public class Botanist : Mobile //Lets Make the NPC!
	{
		[Constructable]
		public Botanist()
		{
			Name = "Jemma";
			Title = "the Botanist";
			Body = 0x191;
			CantWalk = true;
			Hue = 0x83F8;

			AddItem(new Doublet(0x598));
			AddItem(new Skirt(0x59B));
			AddItem(new Boots());

			if (Utility.RandomBool())
			{
				AddItem(new LongHair(1150));
			}

			Blessed = true;
		}

		public Botanist(Serial serial) : base(serial)
		{
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);
			list.Add(new BotanistEntry(from, this));
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

		public class BotanistEntry : ContextMenuEntry
		{
			private Mobile _Mobile;
			private Mobile _Giver;

			public BotanistEntry(Mobile from, Mobile giver) : base(6146, 3)
			{
				_Mobile = from;
				_Giver = giver;

			}

			public override void OnClick()
			{
				var pm = _Mobile as PlayerMobile;
				if (pm == null)
				{
					return;
				}

				if (pm.HasGump(typeof(BotanistGump)))
				{
					return;
				}

				if (pm.Backpack == null)
				{
					return;
				}

				var ej = pm.Backpack.FindItemByType(typeof(DirtJar)) as DirtJar; //Checks to see if a Jar is in their inventory
				if (ej != null && !ej.Full)
				{
					pm.SendMessage(
						"{0} seems distracted right now. You already have a jar, fill it up before you ask for another", _Giver.Name);
				}
				else
				{
					pm.SendGump(new BotanistGump(pm)); //Quest Gump
					pm.AddToBackpack(new DirtJar()); //Deliver Jar
				}
			}

		}


		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			var pm = from as PlayerMobile;
			if (pm == null)
			{
				return false;
			}

			var jar = dropped as DirtJar;
			if (jar == null || !jar.Full)
			{
				PrivateOverheadMessage(MessageType.Regular, 1162, false, "I can't use that for my plants.", pm.NetState);
				return false;
			}

			PrivateOverheadMessage(MessageType.Regular, 1162, false, "Thank you so much! Here you go have a seed. If you want to get me more worms and dirt I'll give you more seeds.", pm.NetState);
			dropped.Consume();

			PlantType type;
			switch (Utility.Random(17))
			{
				case 0: type = PlantType.CampionFlowers; break;
				case 1: type = PlantType.Poppies; break;
				case 2: type = PlantType.Snowdrops; break;
				case 3: type = PlantType.Bulrushes; break;
				case 4: type = PlantType.Lilies; break;
				case 5: type = PlantType.PampasGrass; break;
				case 6: type = PlantType.Rushes; break;
				case 7: type = PlantType.ElephantEarPlant; break;
				case 8: type = PlantType.Fern; break;
				case 9: type = PlantType.PonytailPalm; break;
				case 10: type = PlantType.SmallPalm; break;
				case 11: type = PlantType.CenturyPlant; break;
				case 12: type = PlantType.WaterPlant; break;
				case 13: type = PlantType.SnakePlant; break;
				case 14: type = PlantType.PricklyPearCactus; break;
				case 15: type = PlantType.BarrelCactus; break;
				default: type = PlantType.TribarrelCactus; break;
			}

			Seed reward;

			if (Utility.RandomDouble() < 0.10) //10% chance for a Fire Red Seed
			{
				reward = new Seed(type, PlantHue.FireRed, false);
				from.SendMessage("{0} gives you a {0} seed!!!", Name, PlantHue.FireRed);
			}
			else //30% chance per Seed
			{
				PlantHue hue;
				switch (Utility.Random(3))
				{
					case 0: hue = PlantHue.Pink; break;
					case 1: hue = PlantHue.Magenta; break;
					default: hue = PlantHue.Aqua; break;

				}
				reward = new Seed(type, hue, false);
				from.SendMessage("{0} gives you a {1} seed", Name, hue);
			}

			pm.AddToBackpack(reward);
			return true;
		}
	}
}
