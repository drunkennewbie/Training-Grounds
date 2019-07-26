using Server.Items;
using Server.ContextMenus;
using Server.Gumps;
using Server.Network;
using System.Collections.Generic;


namespace Server.Mobiles
{
	[CorpseName("Biologist's Corpse")]
	public class ChickenBiologist : Mobile
	{
		[Constructable]
		public ChickenBiologist()
		{
			Name = "Clyde";
			Title = "the Chicken Biologist";
			Body = 0x190;
			CantWalk = true;
			Hue = 0x83F8;

			AddItem(new Doublet(0x598));
			AddItem(new LongPants(0x59B));
			AddItem(new Boots());

			
			Blessed = true;
		}

		public ChickenBiologist(Serial serial) : base(serial)
		{
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);
			list.Add(new BiologistEntry(from, this));
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

		public class BiologistEntry : ContextMenuEntry
		{
			private Mobile _Mobile;
			private Mobile _Giver;

			public BiologistEntry(Mobile from, Mobile giver) : base(6146, 3)
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

				if (pm.HasGump(typeof(BiologistGump)))
				{
					return;
				}

				if (pm.Backpack == null)
				{
					return;
				}

				var cm = pm.Backpack.FindItemByType(typeof(CataMortar)) as CataMortar;
				if (cm != null && !cm.Full)
				{
					pm.SendMessage(
						"{0} looks distracted with a chicken", _Giver.Name);
				}
				else
				{
					pm.SendGump(new BiologistGump(pm)); //Quest Gump
					pm.AddToBackpack(new CataMortar()); //Deliver Jar
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

			var cm = dropped as CataMortar;
			if (cm == null || !cm.Full)
			{
				PrivateOverheadMessage(MessageType.Regular, 1162, false, "Whats this? This isn't what I asked for!", pm.NetState);
				return false;
			}

			PrivateOverheadMessage(MessageType.Regular, 1162, false, "Alright, lets see here feed this to the chicken and... It produced some eggs! this one looks normal compared to the others here you can have it.", pm.NetState);
			dropped.Consume();

			
			pm.AddToBackpack(new ChickenLizardEgg());
			return true;
		}
	}
}
