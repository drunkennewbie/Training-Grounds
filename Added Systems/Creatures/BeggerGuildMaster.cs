using System;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.ContextMenus;
using Server.Items;

namespace Server.Mobiles
{
	public class BeggerGuildMaster : BaseGuildmaster
	{
		public override NpcGuild NpcGuild{ get{ return NpcGuild.BeggerGuild; } }

		public override TimeSpan JoinAge{ get{ return TimeSpan.FromDays( 7.0 ); } }
		public override bool CanTeach { get { return false; } }

		[Constructable]
		public BeggerGuildMaster() : base( "Begger" )
		{
		}

		public override void InitOutfit()
		{
			base.InitOutfit();

				AddItem( new Server.Items.GnarledStaff() );
			
		}

		public override bool CheckCustomReqs( PlayerMobile pm )
		{
			if ( pm.Young )
			{
				SayTo( pm, "You are to young to join the Begger's guild." ); 
				return false;
			}
			else if ( pm.Kills > 0 )
			{
				SayTo( pm, "This guild is for the poor not for murderers!" ); 
				return false;
			}
			else if ( pm.Skills[SkillName.Begging].Base < 60.0 )
			{
				SayTo( pm, "You must be at least a journeyman begger to join us" ); 
				return false;
			}

			return true;
		}

		public override void SayWelcomeTo( Mobile m )
		{
			SayTo( m, "Welcome to the Society of Beggers, stay safe on the streets, friend." ); 
		}

		public override bool HandlesOnSpeech( Mobile from )
		{
			if ( from.InRange( this.Location, 2 ) )
				return true;

			return base.HandlesOnSpeech( from );
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);

			if (from is PlayerMobile)
			{
				PlayerMobile pm = (PlayerMobile)from;

				if (pm.NpcGuild == NpcGuild.BeggerGuild)
					list.Add(new ObtainStaffEntry(from, this ));
			}
		}

		public class ObtainStaffEntry: ContextMenuEntry
		{
			private Mobile m_mobile;
			private BeggerGuildMaster m_npc;

			public ObtainStaffEntry(Mobile mobile, BeggerGuildMaster npc)
				: base(1017104)
			{
				m_mobile = mobile;
				m_npc = npc;
			}

			public override void OnClick()
			{
				m_npc.SayTo(m_mobile, "It's a enchanted staff that assists on to look poor and potentially have your target offer better goods. It cost 1000 Dull Silver.");
			}

		}

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			if (from is PlayerMobile)
			{
				PlayerMobile pm = (PlayerMobile)from;
				if (pm.NpcGuild == NpcGuild.BeggerGuild && dropped is BeggerCoins && dropped.Amount == 1000)
				{
					from.AddToBackpack(new BeggerStaff());
					dropped.Delete();
					return true;
				}
			}

			return base.OnDragDrop(from, dropped);
		}

		public BeggerGuildMaster( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}
}
