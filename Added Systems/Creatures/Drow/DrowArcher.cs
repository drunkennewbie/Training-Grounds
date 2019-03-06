using Server.Items;
using Server.Targeting;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "a drow corpse" )]
	public class DrowArcher : BaseCreature
	{
		[Constructable]
		public DrowArcher() : base( AIType.AI_Stealth, FightMode.Weakest, 10, 1, 0.15, 0.4 )
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


			SetStr( 151, 170 );
			SetDex( 92, 130 );
			SetInt( 51, 65 );

			SetDamage( 29, 34 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetSkill( SkillName.Archery, 72.5, 95.0 );

			SetSkill( SkillName.Healing, 60.3, 90.0 );
			SetSkill(SkillName.Anatomy, 50.1, 90.0);

			SetSkill( SkillName.Poisoning, 60.0, 82.5 );
			SetSkill( SkillName.MagicResist, 72.5, 95.0 );
						
			SetSkill( SkillName.Tactics, 72.5, 95.0 );

			SetSkill( SkillName.Hiding, 100.0, 100.0);
			SetSkill( SkillName.Stealth, 80.0, 100.0);
			SetSkill( SkillName.DetectHidden, 100.0, 100.0);

			Fame = 1000;
			Karma = -1000;

			PackItem( new Bandage( Utility.RandomMinMax( 1, 15 ) ) );

			#region Equipment
			AddItem(new Shoes(1));
			AddItem(new Robe(1));

			if (0.1>Utility.RandomDouble())
				AddItem(new DrowBow());
			else
				AddItem(new CompositeBow());

			#endregion


			#region PackItems
			this.PackItem(new Bandage(Utility.RandomMinMax(1, 15)));
			if (Female && 0.05 > Utility.RandomDouble())
				PackItem(new DrowCircletPieces()); //Drow Crown part
			#endregion
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Average );
		}

		public override bool CanStealth { get { return true; } }
		public override bool CanRummageCorpses { get { return true; } }
		public override int Meat{ get{ return 1; } }
		public override bool AlwaysMurderer{ get{ return true; } }
		public override bool ShowFameTitle{ get{ return false; } }

		public override OppositionGroup OppositionGroup
		{
			get{ return OppositionGroup.SavagesAndOrcs; }
		}

		public override bool OnBeforeDeath()
		{
			IMount mount = this.Mount;

			if ( mount != null )
				mount.Rider = null;

			if ( mount is Mobile )
				((Mobile)mount).Delete();

			return base.OnBeforeDeath();
		}

		public override bool IsEnemy( Mobile m )
		{
			if (m.Player && m.FindItemOnLayer(Layer.Helm) is DrowCirclet)
				return false;

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


		private Mobile FindTarget()
		{
			IPooledEnumerable eable = GetMobilesInRange(10);
			foreach (Mobile m in eable)
			{
				if (m.Player && m.Hidden)
				{
					eable.Free();
					return m;
				}
			}

			eable.Free();
			return null;
		}

		public override void OnThink()
		{
			if (Utility.RandomDouble() < 0.2)
			{
				TryToDetectHidden();
			}
		}

		private void TryToDetectHidden()
		{
			Mobile m = FindTarget();

			if (m != null)
			{
				if (Core.TickCount >= NextSkillTime && UseSkill(SkillName.DetectHidden))
				{
					Target targ = Target;

					if (targ != null)
					{
						targ.Invoke(this, this);
					}

					Effects.PlaySound(Location, Map, 0x340);
				}
			}
		}


		public override void AlterMeleeDamageTo( Mobile to, ref int damage )
		{
			if (to is Dragon || to is WhiteWyrm)
				damage *= 2;
			if (to is DreadSpider || to is FrostSpider || to is GiantSpider)
				damage /= 2;
		}

		public DrowArcher( Serial serial ) : base( serial )
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
