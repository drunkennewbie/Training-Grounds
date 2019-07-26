using System;
using Server.Mobiles;
using Server.Spells;
using Server.Spells.Fifth;
using Server.Spells.Seventh;



namespace Server.Items
{
	//Based Off Gnarled Staff
	public class BeggerStaff : BlackStaff
	{
	
		[Constructable]
		public BeggerStaff()
		{
			Name = "Staff of a Begger";
			this.Weight = 12.0;
			Hue = 1008;
						
		}

		public BeggerStaff(Serial serial)
			: base(serial)
		{
		}

		public override bool CanEquip(Mobile from)
		{
			if (!base.CanEquip(from))
				return false;
			if (from.Skills[SkillName.Begging].Base >= 75)
			{
				from.SendMessage("You feel strangly empowered by the staff");
				return true;
			}
			from.SendMessage("The staff magical enchantment rejects you, you lack the skill to wield it.");
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
