using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Mobiles
{
	[CorpseName("a horse corpse")]
	public class BardableHorse : BaseMount
	{
		private bool m_BardingExceptional;
		private Mobile m_BardingCrafter;
		private int m_BardingHP;
		private bool m_HasBarding;
		private CraftResource m_BardingResource;

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile BardingCrafter
		{
			get { return m_BardingCrafter; }
			set { m_BardingCrafter = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool BardingExceptional
		{
			get { return m_BardingExceptional; }
			set { m_BardingExceptional = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int BardingHP
		{
			get { return m_BardingHP; }
			set { m_BardingHP = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int BardingMaxHP
		{
			get
			{
				switch (m_BardingResource)
				{
					default:
						return BardingExceptional ? 12000 : 10000;
					case CraftResource.DullCopper:
					case CraftResource.Valorite:
						return BardingExceptional ? 14500 : 12500;
					case CraftResource.ShadowIron:
						return BardingExceptional ? 17000 : 15000;
				}
			}
		}


		[CommandProperty(AccessLevel.GameMaster)]
		public bool HasBarding
		{
			get { return m_HasBarding; }
			set
			{
				m_HasBarding = value;

				if (m_HasBarding)
				{
					Hue = CraftResources.GetHue(m_BardingResource);
					BodyValue = 0x11C;
					ItemID = 0x3E92;
				}
				else
				{
					Hue = 0x3A4;
					BodyValue = 0xC8;
					ItemID = 0x3EA0;
				}

				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public CraftResource BardingResource
		{
			get { return m_BardingResource; }
			set
			{
				m_BardingResource = value;

				if (m_HasBarding)
					Hue = CraftResources.GetHue(value);

				InvalidateProperties();
			}
		}

		[Constructable]
		public BardableHorse() : this("a strong horse")
		{
		}

		[Constructable]
		public BardableHorse(string name) : base(name, 0xC8, 0x3EA0, AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.4)
		{
			BaseSoundID = 0x16A;

			SetStr(201, 300);
			SetDex(66, 85);
			SetInt(61, 100);

			SetHits(121, 180);

			SetDamage(3, 4);

			SetDamageType(ResistanceType.Physical, 75);
			SetDamageType(ResistanceType.Poison, 25);

			SetResistance(ResistanceType.Physical, 35, 40);
			SetResistance(ResistanceType.Fire, 20, 30);
			SetResistance(ResistanceType.Cold, 20, 40);
			SetResistance(ResistanceType.Poison, 20, 30);
			SetResistance(ResistanceType.Energy, 30, 40);

			SetSkill(SkillName.Anatomy, 45.1, 55.0);
			SetSkill(SkillName.MagicResist, 45.1, 55.0);
			SetSkill(SkillName.Tactics, 45.1, 55.0);
			SetSkill(SkillName.Wrestling, 45.1, 55.0);

			Fame = 2000;
			Karma = -2000;

			Hue = 0x3A4;

			Tamable = true;
			ControlSlots = 1;
			MinTameSkill = 93.9;
		}

		public override int GetIdleSound() { return 0x0AA; }
		public override int GetDeathSound() { return 0x0AD; }
		public override int GetHurtSound() { return 0x0AC; }
		public override int GetAttackSound() { return 0x0AB; }

		public override double GetControlChance(Mobile m, bool useBaseSkill) { return 1.0; }
		public override bool ReacquireOnMovement { get { return true; } }
		public override bool AutoDispel { get { return !Controlled; } }
		public override FoodType FavoriteFood { get { return FoodType.FruitsAndVegies | FoodType.GrainsAndHay; } }
		public override int Meat { get { return 3; } }
		public override int Hides { get { return 10; } }
		public override bool CanAngerOnTame { get { return true; } }

		public BardableHorse(Serial serial) : base(serial) { }

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (m_HasBarding && m_BardingExceptional && m_BardingCrafter != null)
				list.Add(1060853, m_BardingCrafter.Name); // armor exceptionally crafted by ~1_val~

		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)1); // version

			writer.Write((bool)m_BardingExceptional);
			writer.Write((Mobile)m_BardingCrafter);
			writer.Write((bool)m_HasBarding);
			writer.Write((int)m_BardingHP);
			writer.Write((int)m_BardingResource);
		}


		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 1:
					{
						m_BardingExceptional = reader.ReadBool();
						m_BardingCrafter = reader.ReadMobile();
						m_HasBarding = reader.ReadBool();
						m_BardingHP = reader.ReadInt();
						m_BardingResource = (CraftResource)reader.ReadInt();
						break;
					}
			}

			if (Hue == 0 && !m_HasBarding)
				Hue = 0x3A4;

			if (BaseSoundID == -1)
				BaseSoundID = 0x16A;
		}
	}
}