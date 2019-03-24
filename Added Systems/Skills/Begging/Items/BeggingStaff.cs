using System;
using Server.Mobiles;
using Server.Spells;
using Server.Spells.Fifth;
using Server.Spells.Seventh;



namespace Server.Items
{
	//Based Off Gnarled Staff
	public class BeggerStaff : GnarledStaff
	{
	
		[Constructable]
		public BeggerStaff()
		{
			this.Weight = 12.0;
			SkillBonuses.SetValues(0, SkillName.Begging, 10.0);
						
		}

		public BeggerStaff(Serial serial)
			: base(serial)
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
}
