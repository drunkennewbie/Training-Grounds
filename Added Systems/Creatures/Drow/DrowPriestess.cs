using System;
using System.Collections;
using Server;
using Server.Misc;
using Server.Items;
using Server.Spells;

namespace Server.Mobiles
{
	[CorpseName( "a drow corpse" )]
	public class DrowPriestess : BaseCreature
	{
		[Constructable]
		public DrowPriestess() : base( AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = NameList.RandomName("drow female");
			Body = 184;
			Hue = 1000;
			Item hair = new Item(Utility.RandomList(8252, 8253));
			hair.Hue = 2498;
			hair.Layer = Layer.Hair;
			hair.Movable = false;
			AddItem(hair);

			SetStr( 126, 145 );
			SetDex( 91, 110 );
			SetInt( 161, 185 );

			SetDamage( 4, 10 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 30, 40 );
			SetResistance( ResistanceType.Fire, 20, 30 );
			SetResistance( ResistanceType.Cold, 20, 30 );
			SetResistance( ResistanceType.Poison, 20, 30 );
			SetResistance( ResistanceType.Energy, 40, 50 );

			SetSkill( SkillName.EvalInt, 77.5, 100.0 );
			SetSkill( SkillName.Fencing, 62.5, 85.0 );
			SetSkill( SkillName.Macing, 62.5, 85.0 );
			SetSkill( SkillName.Magery, 72.5, 95.0 );
			SetSkill( SkillName.Meditation, 77.5, 100.0 );
			SetSkill( SkillName.MagicResist, 77.5, 100.0 );
			SetSkill( SkillName.Swords, 62.5, 85.0 );
			SetSkill( SkillName.Tactics, 62.5, 85.0 );
			SetSkill( SkillName.Wrestling, 62.5, 85.0 );

			Fame = 1000;
			Karma = -1000;


			#region PackItems
			PackReg(10, 15);
			this.PackItem(new Bandage(Utility.RandomMinMax(1, 15)));
			if (Female && 0.05 > Utility.RandomDouble())
				PackItem(new DrowCircletPieces()); //Drow Crown part
			#endregion


			#region Equipment
			AddItem(new Server.Items.Shoes(1));
			AddItem(new Server.Items.Robe(1));
			#endregion
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Average );

		}

		public override int Meat{ get{ return 1; } }
		public override bool AlwaysMurderer{ get{ return true; } }
		public override bool ShowFameTitle{ get{ return false; } }

		
		public override bool IsEnemy( Mobile m )
		{
			if (m.Player && m.FindItemOnLayer(Layer.Helm) is DrowCirclet)
				return false;


			return base.IsEnemy(m);
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
			if (to is Dragon || to is WhiteWyrm)
				damage *= 2;
			if (to is DreadSpider || to is FrostSpider || to is GiantSpider)
				damage /= 2;
		}

		public override void OnGotMeleeAttack( Mobile attacker )
		{
			base.OnGotMeleeAttack( attacker );

			if ( 0.1 > Utility.RandomDouble() )
				BeginDance();
		}

		public void BeginDance()
		{
			if( this.Map == null )
				return;

			ArrayList list = new ArrayList();

			foreach ( Mobile m in this.GetMobilesInRange( 8 ) )
			{
				if ( m != this && m is DrowPriestess)
					list.Add( m );
			}

			Animate( 111, 5, 1, true, false, 0 ); // Do a little dance...

			if ( AIObject != null )
				AIObject.NextMove = Core.TickCount + 1000;

			if ( list.Count >= 3 )
			{
				for ( int i = 0; i < list.Count; ++i )
				{
					DrowPriestess dancer = (DrowPriestess)list[i];

					dancer.Animate( 111, 5, 1, true, false, 0 ); // Get down tonight...

					if ( dancer.AIObject != null )
						dancer.AIObject.NextMove = Core.TickCount + 1000;
				}

				Timer.DelayCall( TimeSpan.FromSeconds( 1.0 ), new TimerCallback( EndDance ) );
			}
		}

		public void EndDance()
		{
			if ( Deleted )
				return;

			ArrayList list = new ArrayList();

			foreach ( Mobile m in this.GetMobilesInRange( 8 ) )
				list.Add( m );

			if ( list.Count > 0 )
			{
				switch ( Utility.Random( 3 ) )
				{
					case 0: /* greater heal */
					{
						foreach ( Mobile m in list )
						{
							bool isFriendly = ( m is Drow || m is DrowArcher || m is DrowPriestess );

							if ( !isFriendly )
								continue;

							if ( m.Poisoned || MortalStrike.IsWounded( m ) || !CanBeBeneficial( m ) )
								continue;

							DoBeneficial( m );

							// Algorithm: (40% of magery) + (1-10)

							int toHeal = (int)(Skills[SkillName.Magery].Value * 0.4);
							toHeal += Utility.Random( 1, 10 );

							m.Heal( toHeal, this );

							m.FixedParticles( 0x376A, 9, 32, 5030, EffectLayer.Waist );
							m.PlaySound( 0x202 );
						}

						break;
					}
					case 1: /* lightning */
					{
						foreach ( Mobile m in list )
						{
								bool isFriendly = (m is Drow || m is DrowArcher || m is DrowPriestess);

								if ( isFriendly )
								continue;

							if ( !CanBeHarmful( m ) )
								continue;

							DoHarmful( m );

							double damage;

							if ( Core.AOS )
							{
								int baseDamage = 6 + (int)(Skills[SkillName.EvalInt].Value / 5.0);

								damage = Utility.RandomMinMax( baseDamage, baseDamage + 3 );
							}
							else
							{
								damage = Utility.Random( 12, 9 );
							}

							m.BoltEffect( 0 );

							SpellHelper.Damage( TimeSpan.FromSeconds( 0.25 ), m, this, damage, 0, 0, 0, 0, 100 );
						}

						break;
					}
					case 2: /* poison */
					{
						foreach ( Mobile m in list )
						{
								bool isFriendly = (m is Drow || m is DrowArcher || m is DrowPriestess);

								if ( isFriendly )
								continue;

							if ( !CanBeHarmful( m ) )
								continue;

							DoHarmful( m );

							if ( m.Spell != null )
								m.Spell.OnCasterHurt();

							m.Paralyzed = false;

							double total = Skills[SkillName.Magery].Value + Skills[SkillName.Poisoning].Value;

							double dist = GetDistanceToSqrt( m );

							if ( dist >= 3.0 )
								total -= (dist - 3.0) * 10.0;

							int level;

							if ( total >= 200.0 && Utility.Random( 1, 100 ) <= 10 )
								level = 3;
							else if ( total > 170.0 )
								level = 2;
							else if ( total > 130.0 )
								level = 1;
							else
								level = 0;

							m.ApplyPoison( this, Poison.GetPoison( level ) );

							m.FixedParticles( 0x374A, 10, 15, 5021, EffectLayer.Waist );
							m.PlaySound( 0x474 );
						}

						break;
					}
				}
			}
		}

		public DrowPriestess( Serial serial ) : base( serial )
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
