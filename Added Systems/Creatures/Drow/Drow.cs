using System;
using Server;
using Server.Misc;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "a drow corpse" )]
	public class Drow : BaseCreature
	{
		[Constructable]
		public Drow() : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{


			if (Female = Utility.RandomBool())
			{
				Name = NameList.RandomName("drow female");
				Body = 184;
				Hue = 1000;
				Item hair = new Item(Utility.RandomList(8252, 8253));
				hair.Hue = 2498;
				hair.Layer = Layer.Hair;
				hair.Movable = false;
				AddItem(hair);
			}
			else
			{
				Body = 183;
				Hue = 1000;
				Name = NameList.RandomName("drow male");
				Item hair = new Item(Utility.RandomList(8252, 8253, 8251));
				hair.Hue = 2498;
				hair.Layer = Layer.Hair;
				hair.Movable = false;
				AddItem(hair);
			}
			

			SetStr( 96, 115 );
			SetDex( 86, 105 );
			SetInt( 51, 65 );

			SetDamage( 23, 27 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetSkill( SkillName.Fencing, 60.0, 82.5 );
			SetSkill( SkillName.Archery, 60.0, 82.5 );
			SetSkill( SkillName.Poisoning, 60.0, 82.5 );
			SetSkill( SkillName.MagicResist, 57.5, 80.0 );
			SetSkill( SkillName.Swords, 60.0, 82.5 );
			SetSkill( SkillName.Tactics, 60.0, 82.5 );
			SetSkill( SkillName.Healing, 80.3, 90.0);
			SetSkill( SkillName.Stealth, 60.3, 80.0);

			Fame = 1000;
			Karma = -1000;

			#region Equipment
			AddItem(new Server.Items.Shoes(1));
			AddItem(new Server.Items.Robe(1));
			AddItem(new Server.Items.Kryss());
			#endregion


			#region PackItems
			this.PackItem(new Bandage(Utility.RandomMinMax(1, 15)));
			if (Female && 0.05 > Utility.RandomDouble())
				PackItem(new DrowCircletPieces()); //Drow Crown part
			#endregion

		}

		public override bool CanStealth { get { return true; } }
		public override bool CanRummageCorpses { get { return true; } }
	
		public override int Meat{ get{ return 1; } }
		public override bool AlwaysMurderer{ get{ return true; } }
		public override bool ShowFameTitle{ get{ return false; } }

		//public override OppositionGroup OppositionGroup
		//{
		//	get{ return OppositionGroup.DrowAndPaladins; }
		//}

		public override bool IsEnemy( Mobile m )
		{
		if (m.Player && m.FindItemOnLayer(Layer.Helm) is DrowCirclet)
		return false;
		//	if (m.Player && m.FindItemOnLayer(Layer.Helm) is DrowHood)
		//		return false;

			return base.IsEnemy( m );
		}

		public override void AggressiveAction( Mobile aggressor, bool criminal )
		{
			base.AggressiveAction(aggressor, criminal);

			Item item = aggressor.FindItemOnLayer(Layer.Helm);

			if (item is DrowCirclet)
			{
				AOS.Damage(aggressor, 50, 0, 100, 0, 0, 0);
				item.Delete();
				aggressor.FixedParticles(0x36BD, 20, 10, 5044, EffectLayer.Head);
				aggressor.PlaySound(0x307);
			}
		}

		public override void AlterMeleeDamageTo( Mobile to, ref int damage )
		{
			if ( to is Dragon || to is WhiteWyrm)
				damage *= 2;
			if (to is DreadSpider || to is FrostSpider || to is GiantSpider)
				damage /= 2;
		}

		public Drow( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}
