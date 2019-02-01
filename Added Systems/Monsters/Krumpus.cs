using System;
using System.Collections.Generic;
using Server.Items;
using Server.Spells;
using Server.Network;
using Server.Engines.CannedEvil;


namespace Server.Mobiles
{
	[CorpseName("an Krumpus corpse")]
	public class Krumpus : BaseCreature
	{
		private Timer m_Timer;
		private DateTime m_NextClearEffect;
		private DateTime m_NextTeleportTime;
		private DateTime m_NextSummon;
		private DateTime m_NextWither;

		[Constructable]
		public Krumpus() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
		{
			Name = "Krumpus";
			Body = 241;
			BaseSoundID = 655;

			SetStr(425);
			SetDex(150);
			SetInt(750);

			SetHits(26000);
			SetStam(300, 300);

			SetDamage(25, 35);

			SetDamageType(ResistanceType.Physical, 100);

			SetResistance(ResistanceType.Physical, 60, 70);
			SetResistance(ResistanceType.Fire, 50, 60);
			SetResistance(ResistanceType.Cold, 50, 60);
			SetResistance(ResistanceType.Poison, 40, 50);
			SetResistance(ResistanceType.Energy, 40, 50);

			SetSkill(SkillName.MagicResist, 100.0);
			SetSkill(SkillName.Tactics, 100.0);
			SetSkill(SkillName.Wrestling, 100.0);

			Fame = 22500;
			Karma = -22500;

			VirtualArmor = 70;

			m_Timer = new TeleportTimer(this);
			m_Timer.Start();
		}

		public override void GenerateLoot()
		{

		}

		public override bool BleedImmune { get { return true; } }
		public override bool AlwaysMurderer { get { return true; } }
		public override bool AutoDispel { get { return true; } }
		public override double AutoDispelChance { get { return 1.0; } }
		public override bool BardImmune { get { return true; } }
		public override bool Unprovokable { get { return true; } }
		public override bool Uncalmable { get { return true; } }
		public override Poison PoisonImmune { get { return Poison.Deadly; } }



		public Krumpus(Serial serial) : base(serial)
		{
		}

		public override void OnGotMeleeAttack(Mobile attacker)
		{
				DoSpecialAbility(attacker);
		}
		public override void OnDamagedBySpell(Mobile attacker)
		{
			DoSpecialAbility(attacker);
		}
		public void DoSpecialAbility(Mobile target)
		{

			if (target == null || target.Deleted) //sanity
				return;
			if (DateTime.UtcNow > m_NextSummon && 0.1 >= Utility.RandomDouble()) // 10% chance to Snowmen
			{
				this.Say("Come out and Play my little friends");
				Timer.DelayCall(TimeSpan.FromSeconds(1.0), SpawnSnowmen, target);
				m_NextSummon = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(10, 20));
			}
				if (DateTime.UtcNow > m_NextTeleportTime && 0.2 >= Utility.RandomDouble()) // 20% chance to teleport to player
			{
				this.Say("You've been naughty");
				KrumpusTeleport(this, target);
				m_NextTeleportTime = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(5, 10));
			}
			if (DateTime.UtcNow > m_NextWither && Hits < 16000 && 0.2 >= Utility.RandomDouble()) //AOE Wither
			{
				this.Say("YOU ALL WILL FREEZE!");
				Timer.DelayCall(TimeSpan.FromSeconds(2.0), Wither);
				Wither();
				m_NextWither = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(2, 5));
			}
			if (DateTime.UtcNow > m_NextClearEffect && 0.1 >= Utility.RandomDouble())
			{
				this.Say("Let it snow, let it snow");
				new ClearTimer(this);
			}

		}

		public override void OnDeath(Container c)
		{
			base.OnDeath(c);

			this.Say("I Will be back next year my children! Better be good!");

		}

		private static void TeleportTo(Mobile m_Owner, Mobile target)
		{
			Point3D from = m_Owner.Location;
			Point3D to = target.Location;

			m_Owner.Location = to;
			Effects.SendLocationParticles(
			EffectItem.Create(from, m_Owner.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 2023);
			Effects.SendLocationParticles(EffectItem.Create(to, m_Owner.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 5023);

			m_Owner.PlaySound(0x1FE);
		}
		public static void KrumpusTeleport(Mobile from, Mobile target)
		{
			TeleportTo(from, target);

		}

		public void SpawnSnowmen(Mobile target)
		{
			Map map = this.Map;

			if (map == null)
				return;

			int IceElm = 0;

			foreach (Mobile m in this.GetMobilesInRange(10))
			{
				if (m is IceElemental)
					++IceElm;
			}

			if (IceElm < 6)
			{
				int newIceElm = Utility.RandomMinMax(2, 3);

				for (int i = 0; i < newIceElm; ++i)
				{
					BaseCreature sn;

					switch (Utility.Random(3))
					{
						default:
						case 0:
						case 1: sn = new IceElemental(); break;
						case 2: sn = new SnowElemental(); break;

					}

					bool validLocation = false;
					Point3D loc = this.Location;

					for (int j = 0; !validLocation && j < 10; ++j)
					{
						int x = X + Utility.Random(3) - 1;
						int y = Y + Utility.Random(3) - 1;
						int z = map.GetAverageZ(x, y);

						if (validLocation = map.CanFit(x, y, this.Z, 16, false, false))
							loc = new Point3D(x, y, Z);
						else if (validLocation = map.CanFit(x, y, z, 16, false, false))
							loc = new Point3D(x, y, z);
					}

					sn.MoveToWorld(loc, map);
					sn.Combatant = target;
				}
			}
		}
		public void Wither()
		{
			Map map = this.Map;
			Mobile Caster = this;

			if (map != null)
			{

				List<Mobile> targets = new List<Mobile>();

				BaseCreature cbc = Caster as BaseCreature;
				bool isMonster = (cbc != null && !cbc.Controlled && !cbc.Summoned);

				foreach (Mobile m in Caster.GetMobilesInRange(8))
				{
					if (Caster != m && Caster.InLOS(m) && (isMonster || SpellHelper.ValidIndirectTarget(Caster, m)) && Caster.CanBeHarmful(m, false))
					{
						if (isMonster)
						{
							if (m is BaseCreature)
							{
								BaseCreature bc = (BaseCreature)m;

								if (!bc.Controlled && !bc.Summoned && bc.Team == cbc.Team)
									continue;
							}
							else if (!m.Player)
							{
								continue;
							}
						}

						targets.Add(m);
					}
				}

				Effects.PlaySound(Caster.Location, map, 0x1FB);
				Effects.PlaySound(Caster.Location, map, 0x10B);
				Effects.SendLocationParticles(EffectItem.Create(Caster.Location, map, EffectItem.DefaultDuration), 0x37CC, 1, 40, 97, 3, 9917, 0);

				for (int i = 0; i < targets.Count; ++i)
				{
					Mobile m = targets[i];

					Caster.DoHarmful(m);
					m.FixedParticles(0x374A, 1, 15, 9502, 97, 3, (EffectLayer)255);

					int damage = 50;
					Damage(damage, m);


				}
			}

		}

		private class TeleportTimer : Timer
		{
			private Mobile m_Owner;

			private static int[] m_Offsets = new int[]
			{
			-1, -1,
			-1,  0,
			-1,  1,
			0, -1,
			0,  1,
			1, -1,
			1,  0,
			1,  1
			};

			public TeleportTimer(Mobile owner) : base(TimeSpan.FromSeconds(5.0), TimeSpan.FromSeconds(5.0))
			{
				Priority = TimerPriority.TwoFiftyMS;

				m_Owner = owner;
			}

			protected override void OnTick()
			{
				if (m_Owner.Deleted)
				{
					Stop();
					return;
				}

				Map map = m_Owner.Map;

				if (map == null)
					return;

				if (0.25 < Utility.RandomDouble())
					return;

				Mobile toTeleport = null;

				foreach (Mobile m in m_Owner.GetMobilesInRange(16))
				{
					if (m != m_Owner && m.Player && m_Owner.CanBeHarmful(m) && m_Owner.CanSee(m))
					{
						toTeleport = m;
						break;
					}
				}

				if (toTeleport != null)
				{
					int offset = Utility.Random(8) * 2;

					Point3D to = m_Owner.Location;

					for (int i = 0; i < m_Offsets.Length; i += 2)
					{
						int x = m_Owner.X + m_Offsets[(offset + i) % m_Offsets.Length];
						int y = m_Owner.Y + m_Offsets[(offset + i + 1) % m_Offsets.Length];

						if (map.CanSpawnMobile(x, y, m_Owner.Z))
						{
							to = new Point3D(x, y, m_Owner.Z);
							break;
						}
						else
						{
							int z = map.GetAverageZ(x, y);

							if (map.CanSpawnMobile(x, y, z))
							{
								to = new Point3D(x, y, z);
								break;
							}
						}
					}

					Mobile m = toTeleport;

					Point3D from = m.Location;

					m.Location = to;

					Server.Spells.SpellHelper.Turn(m_Owner, toTeleport);
					Server.Spells.SpellHelper.Turn(toTeleport, m_Owner);

					m.ProcessDelta();

					Effects.SendLocationParticles(EffectItem.Create(from, m.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 2023);
					Effects.SendLocationParticles(EffectItem.Create(to, m.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 5023);

					m.PlaySound(0x1FE);

					m_Owner.Combatant = toTeleport;
				}
			}
		}
		private class ClearTimer : Timer
		{
			private Krumpus m_Mobile;
			private int m_Tick;

			public ClearTimer(Krumpus mob)
				: base(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2))
			{
				m_Tick = 1;
				m_Mobile = mob;
				Start();
			}

			protected override void OnTick()
			{
				if (m_Tick < 15)
				{
					Point3D p = FindLocation(m_Mobile.Map, m_Mobile.Location, 7);
					Effects.SendLocationEffect(p, m_Mobile.Map, 0x3789, 30, 1, 2062, 0x4);

					m_Tick++;
				}
				else
				{
					m_Mobile.ClearAround();
					Stop();
				}
			}

			private Point3D FindLocation(Map map, Point3D center, int range)
			{
				int cx = center.X;
				int cy = center.Y;

				for (int i = 0; i < 20; ++i)
				{
					int x = cx + Utility.Random(range * 2) - range;
					int y = cy + Utility.Random(range * 2) - range;

					if ((cx - x) * (cx - x) + (cy - y) * (cy - y) > range * range)
						continue;

					int z = map.GetAverageZ(x, y);

					if (!map.CanFit(x, y, z, 6, false, false))
						continue;

					int topZ = z;

					foreach (Item item in map.GetItemsInRange(new Point3D(x, y, z), 0))
					{
						topZ = Math.Max(topZ, item.Z + item.ItemData.CalcHeight);
					}

					return new Point3D(x, y, topZ);
				}

				return center;
			}
		}
		private void ClearAround()
		{
			Point3D loc = Location;
			Map pmmap = Map;

			List<Point3D> points = new List<Point3D>();

			Server.Misc.Geometry.Circle2D(loc, pmmap, 7, (pnt, map) =>
			{
				if (map.CanFit(pnt, 0) && InLOS(pnt))
					points.Add(pnt);
			});

			if (pmmap != Map.Internal && pmmap != null)
			{
				Server.Misc.Geometry.Circle2D(loc, pmmap, 6, (pnt, map) =>
				{
					if (map.CanFit(pnt, 0) && InLOS(pnt) && Utility.RandomBool())
					{
						Effects.SendPacket(pnt, map, new ParticleEffect(EffectType.FixedXYZ, Serial, Serial.Zero, 0x3789, pnt, pnt, 1, 30, false, false, 0, 3, 0, 9502, 1, Serial, 153, 0));
						Effects.SendPacket(pnt, map, new ParticleEffect(EffectType.FixedXYZ, Serial, Serial.Zero, 0x9DAC, pnt, pnt, 1, 30, false, false, 0, 0, 0, 9502, 1, Serial, 153, 0));
					}
				});

				Server.Misc.Geometry.Circle2D(loc, pmmap, 7, (pnt, map) =>
				{
					if (map.CanFit(pnt, 0) && InLOS(pnt) && Utility.RandomBool())
					{
						Effects.SendPacket(pnt, map, new ParticleEffect(EffectType.FixedXYZ, Serial, Serial.Zero, 0x3789, pnt, pnt, 1, 30, false, false, 0, 3, 0, 9502, 1, Serial, 153, 0));
						Effects.SendPacket(pnt, map, new ParticleEffect(EffectType.FixedXYZ, Serial, Serial.Zero, 0x9DAC, pnt, pnt, 1, 30, false, false, 0, 0, 0, 9502, 1, Serial, 153, 0));
					}
				});
			}

			Timer.DelayCall(TimeSpan.FromMilliseconds(500), () =>
			{
				IPooledEnumerable eable = GetMobilesInRange(6);

				foreach (Mobile from in eable)
				{
					if (!from.Alive || from == this || from.AccessLevel > AccessLevel.Player)
						continue;

					if (from is PlayerMobile || (from is BaseCreature && (((BaseCreature)from).Controlled) || ((BaseCreature)from).Summoned))
					{
						Point3D point = points[Utility.Random(points.Count)];
						from.MoveToWorld(point, pmmap);
						from.Frozen = true;

						Timer.DelayCall(TimeSpan.FromSeconds(3), () =>
						{
							from.Frozen = false;
							from.SendLocalizedMessage(1005603); // You can move again!
						});

						if (CanBeHarmful(from))
						{
							double damage = from.Hits * 0.6;

							if (damage < 10.0)
								damage = 10.0;
							else if (damage > 75.0)
								damage = 75.0;

							DoHarmful(from);

							AOS.Damage(from, this, (int)damage, 100, 0, 0, 0, 0);
						}
					}
				}

				eable.Free();
			});

			Hue = 0;

			m_NextClearEffect = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(30, 60));
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

			if (BaseSoundID == 263)
				BaseSoundID = 655;
		}
	}
}
