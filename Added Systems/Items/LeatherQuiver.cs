using System;
using Server;

namespace Server.Items
{
	[FlipableAttribute( 0x2FB7, 0x3171 )]
	public class LeatherQuiver : BaseQuiver
	{
		[Constructable]
		public LeatherQuiver() : base()
		{
			Name = "Leather Quiver";
			WeightReduction = 50;
			Capacity = 1000;
			DamageIncrease = 0;
			Attributes = null;
		}

		public LeatherQuiver( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadEncodedInt();
		}
	}
}
