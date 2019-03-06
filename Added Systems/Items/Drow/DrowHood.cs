using System;
using Server.Items;

namespace Server.Items
{
	public class DrowHood : BaseHat
	{
		public override int InitMinHits { get { return 25; } }
		public override int InitMaxHits { get { return 45; } }

		public override int AosStrReq { get { return 10; } }
		public override int OldStrReq { get { return 10; } }

		public override bool Dye( Mobile from, DyeTub sender )
		{
			from.SendLocalizedMessage( sender.FailMessage );
			return false;
		}

		public override string DefaultName
		{
			get { return "Drow Hood";}
		}

		[Constructable]
		public DrowHood() : base(0x278F)
		{
			Weight = 2.0;
			Hue = 1;
		}

		public DrowHood(Serial serial) : base(serial)
		{
		}

		public override bool CanEquip(Mobile m)
		{
			if (!base.CanEquip(m))
				return false;
			if (m.BodyMod == 183 || m.BodyMod == 184)
			{
				m.SendLocalizedMessage(1061629);
				return false;
			}
			else if (m.FindItemOnLayer(Layer.Helm) is OrcishKinMask)
			{
				m.SendLocalizedMessage(501605); // You are already disguised.
				return false;
			}
			return true;
		}

	
		public override void OnAdded(IEntity parent)
		{
			base.OnAdded( parent );

			if ( parent is Mobile )
				Misc.Titles.AwardKarma( (Mobile)parent, -20, true );
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
	}
}
