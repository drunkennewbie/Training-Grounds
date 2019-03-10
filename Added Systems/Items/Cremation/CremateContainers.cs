using Server.Targeting;

namespace Server.Items
{
	public class CremationUrn : Item
	{
		[Constructable]
		public CremationUrn() : base(0x241D)
		{
			Name = "a funeral urn";
			Weight = 1.0;
		}

		public CremationUrn(Serial serial) : base(serial)
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

	public class FilledUrn : Item
	{
		public Mobile Owner;

		[Constructable]
		public FilledUrn(Mobile owner, int hue) : base(0x241D)
		{
			if (owner != null)
			{
				Name = "A Funeral Urn with " + owner.Name + "'s ashes";
			}
			else
			{
				Name = "A Funeral Urn with Unknown Ashes";
			}
			if (hue != 0)
			{
				Hue = hue;
			}
			else
			{
				Hue = 0;
			}
			Owner = owner;
			Weight = 3.0;
		}

		public FilledUrn(Serial serial) : base(serial)
		{
		}
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
			writer.WriteMobile(Owner);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
			Owner = reader.ReadMobile();
		}
	}

	[FlipableAttribute(0x09a8, 0x0e80)]
	public class CrematationBox : Item
	{

		[Constructable]
		public CrematationBox() : base(0x09a8)
		{
			Name = "Cremation Kit";
			Weight = 5.0;
			Hue = 916;
		}

		public CrematationBox(Serial serial) : base(serial)
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

		public override void OnDoubleClick(Mobile from)
		{
			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
			}
			else if (from.Skills[SkillName.Forensics].Base < 90.0)
			{
				from.SendLocalizedMessage(1042594); // You do not understand how to use this.
			}
			else
			{
				from.SendMessage("Target the Corpse you would like to pack up");
				from.Target = new CorpseTarget(this);
			}
		}
		private class CorpseTarget : Target
		{
			private CrematationBox m_Kit;
			private Mobile Owner;

			public CorpseTarget(CrematationBox kit) : base(3, false, TargetFlags.None)
			{
				m_Kit = kit;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (m_Kit.Deleted)
					return;

				if (!(targeted is Corpse))
				{
					from.SendLocalizedMessage(1042600); // That is not a corpse!
				}
				else if (targeted is Corpse && ((Corpse)targeted).Carved)
				{
					from.SendMessage("That corpse is to damaged to use"); // That corpse seems to have been visited by a taxidermist already.
				}
				else if (!m_Kit.IsChildOf(from.Backpack))
				{
					from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
				}
				else if (from.Skills[SkillName.Forensics].Base < 90.0)
				{
					from.SendLocalizedMessage(1042603); // You would not understand how to use the kit.
				}
				else
				{
					from.RevealingAction();
					Owner = ((Corpse)targeted).Owner;
					((Corpse)targeted).Cremation(from, m_Kit);
					if (((Corpse)targeted).Carved)
					{
						m_Kit.Delete();
						from.AddToBackpack(new FilledCrematationBox(Owner));
					}

				}
			}
		}



	}

	[FlipableAttribute(0x09a8, 0x0e80)]
	public class FilledCrematationBox : Item
	{
		public Mobile Owner;

		[Constructable]
		public FilledCrematationBox(Mobile owner) : base(0x09a8)
		{
			if (owner != null)
			{
				Name = "Cremation Kit filled with " + owner.Name + "'s remains";
			}
			else
			{
				Name = "Full Cremation Kit";
			}


			Owner = owner;
			Weight = 30.0;
			Hue = 2425;
		}

		public FilledCrematationBox(Serial serial) : base(serial)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
			}
			else if (from.Skills[SkillName.Cooking].Base < 90.0)
			{
				from.SendMessage("You don't feel very comfortable doing this");
			}
			else
			{
				from.SendMessage("Target the Corpse you would like to pack up");
				from.Target = new CremationOven(this);
			}
		}

		private class CremationOven : Target
		{
			public Mobile Owner;
			private FilledCrematationBox m_cremationbox;
			private int fuhue = 0;

			public CremationOven(FilledCrematationBox Cremationbox) : base(3, false, TargetFlags.None)
			{
				m_cremationbox = Cremationbox;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (m_cremationbox.Deleted)
					return;

				if (!(targeted is CrematorOven))
				{
					from.SendMessage("This is not a cremation oven");
				}
				else if (!m_cremationbox.IsChildOf(from.Backpack))
				{
					from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
				}
				else
				{
					Owner = m_cremationbox.Owner;
					Item cu = from.Backpack.FindItemByType(typeof(CremationUrn));
					if (cu != null)
					{
						fuhue = cu.Hue;
						from.AddToBackpack(new FilledUrn(Owner, fuhue));
						m_cremationbox.Delete();
						cu.Delete();
						

					}
					else
						from.SendMessage("You do not have a funeral urn to put the ashes into");

				}
			}

		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
			writer.WriteMobile(Owner);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
			Owner = reader.ReadMobile();
		}
	}

	
}
