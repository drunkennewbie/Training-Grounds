using System;
using Server.Targeting;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Network;



namespace Server.Items
{
	public class DrowCircletPieces : Item
	{
		private const int m_Partial = 3;
		private const int m_completed = 5;
		private int m_Quantity;
		public int Quantity
		{
			get { return m_Quantity; }
			set
			{
				if (value <= 1)
					m_Quantity = 1;
				else if (value >= m_completed)
					m_Quantity = m_completed;
				else
					m_Quantity = value;

				if (m_Quantity < m_Partial)
					ItemID = 0x0F29;
				else if (m_Quantity < m_completed)
					ItemID = 0x2B6E;
				else
					ItemID = 0x0F29;

				InvalidateProperties();
			}
		}

		[Constructable]
		public DrowCircletPieces() : base(0x0F29)
		{
			m_Quantity = 1;
			Name = "Pieces of Drow Circlet";
			Weight = 1.0;
			Hue = 1;
		}
		public DrowCircletPieces(Serial serial) : base(serial)
		{
		}

		public override void OnSingleClick(Mobile from)
		{
			if (m_Quantity < m_Partial)
				LabelTo(from, "a part of a Drow Circlet");
			else
				LabelTo(from, "a partially made Drow Circlet");
		}


		public override void OnDoubleClick(Mobile m)
		{
			if (m_Quantity < m_completed)
			{
				if (!IsChildOf(m.Backpack))
					m.SendMessage("You can't use that, put it in your backpack");
				else
					m.Target = new InternalTarget(this);
			}

		}

		private class InternalTarget : Target
		{
			private DrowCircletPieces m_pieces;

			public InternalTarget(DrowCircletPieces pieces) : base(-1, false, TargetFlags.None)
			{
				m_pieces = pieces;
			}
			protected override void OnTarget(Mobile from, object targeted)
			{
				Item targ = targeted as Item;
				if (m_pieces.Deleted || m_pieces.Quantity >= DrowCircletPieces.m_completed || targ == null)
				{
					return;
				}

				if (m_pieces.IsChildOf(from.Backpack) && targ.IsChildOf(from.Backpack) && targ is DrowCircletPieces & targ != m_pieces)
				{
					DrowCircletPieces targPieces = (DrowCircletPieces)targ;
					if (targPieces.Quantity < DrowCircletPieces.m_completed)
					{
						if (targPieces.Quantity + m_pieces.Quantity <= DrowCircletPieces.m_completed)
						{
							targPieces.Quantity += m_pieces.Quantity;
							m_pieces.Delete();
						}
						else
						{
							int delta = DrowCircletPieces.m_completed - targPieces.Quantity;
							targPieces.Quantity += delta;
							m_pieces.Quantity -= delta;

						}

						if (targPieces.Quantity >= DrowCircletPieces.m_completed)
						{
							targPieces.Delete();
							from.AddToBackpack(new DrowCirclet());
						}
						else
							from.SendMessage("You attached pieces to the Circlet ");

						return;

					}
					from.SendMessage("Nothing Happened");
				}
			}
		}
			   			

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
			writer.WriteEncodedInt(m_Quantity);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
			m_Quantity = reader.ReadEncodedInt();
		}
	}
	
	[FlipableAttribute(0x2B6E, 0x3165)]
	public class DrowCirclet : BaseHat
	{
		public override int InitMinHits { get { return 50; } }
		public override int InitMaxHits { get { return 65; } }

		public override int AosStrReq { get { return 10; } }
		public override int OldStrReq { get { return 10; } }

		public override bool AllowMaleWearer { get { return false; }}


		public override bool Dye(Mobile from, DyeTub sender)
		{
			from.SendLocalizedMessage(sender.FailMessage);
			return false;
		}

		[Constructable]
		public DrowCirclet() : base(0x2b6f, 1)
		{
			Name = "Drow Circlet";
			Weight = 2.0;
		}

		public DrowCirclet(Serial serial) : base(serial)
		{
		}
		public override void OnAdded(IEntity parent)
		{
			base.OnAdded(parent);

			if (parent is Mobile)
				Misc.Titles.AwardKarma((Mobile)parent, -20, true);
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

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadEncodedInt();
		}
	}
}
