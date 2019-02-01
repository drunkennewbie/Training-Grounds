namespace Server.Items
{
	[FlipableAttribute(0xF52, 0xF51)]
	public class RustyDagger : BaseKnife
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.InfectiousStrike; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.ShadowStrike; } }

		public override int AosStrengthReq { get { return 10; } }
		public override int AosMinDamage { get { return 10; } }
		public override int AosMaxDamage { get { return 11; } }
		public override int AosSpeed { get { return 56; } }
		public override float MlSpeed { get { return 2.00f; } }

		public override int OldStrengthReq { get { return 1; } }
		public override int OldMinDamage { get { return 1; } }
		public override int OldMaxDamage { get { return 2; } }
		public override int OldSpeed { get { return 58; } }

		public override int InitMinHits { get { return 10; } }
		public override int InitMaxHits { get { return 25; } }

		public override SkillName DefSkill { get { return SkillName.Fencing; } }
		public override WeaponType DefType { get { return WeaponType.Piercing; } }
		public override WeaponAnimation DefAnimation { get { return WeaponAnimation.Pierce1H; } }

		[Constructable]
		public RustyDagger() : base(0xF52)
		{
			Name = "Rusty Dagger";
			Weight = 1.0;
			Hue = 2401;
		}

		public RustyDagger(Serial serial) : base(serial)
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
	[FlipableAttribute(0xF43, 0xF44)]
	public class RustyHatchet : BaseAxe
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.ArmorIgnore; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.Disarm; } }

		public override int AosStrengthReq { get { return 20; } }
		public override int AosMinDamage { get { return 13; } }
		public override int AosMaxDamage { get { return 15; } }
		public override int AosSpeed { get { return 41; } }
		public override float MlSpeed { get { return 2.75f; } }

		public override int OldStrengthReq { get { return 15; } }
		public override int OldMinDamage { get { return 1; } }
		public override int OldMaxDamage { get { return 2; } }
		public override int OldSpeed { get { return 40; } }

		public override int InitMinHits { get { return 10; } }
		public override int InitMaxHits { get { return 40; } }

		[Constructable]
		public RustyHatchet() : base(0xF43)
		{
			Name = "Rusty Hatchet";
			Weight = 4.0;
			Hue = 2401;
		}

		public RustyHatchet(Serial serial) : base(serial)
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
	[FlipableAttribute(0x13FF, 0x13FE)]
	public class RustyKatana : BaseSword
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.DoubleStrike; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.ArmorIgnore; } }

		public override int AosStrengthReq { get { return 25; } }
		public override int AosMinDamage { get { return 11; } }
		public override int AosMaxDamage { get { return 13; } }
		public override int AosSpeed { get { return 46; } }
		public override float MlSpeed { get { return 2.50f; } }

		public override int OldStrengthReq { get { return 10; } }
		public override int OldMinDamage { get { return 1; } }
		public override int OldMaxDamage { get { return 5; } }
		public override int OldSpeed { get { return 60; } }

		public override int DefHitSound { get { return 0x23B; } }
		public override int DefMissSound { get { return 0x23A; } }

		public override int InitMinHits { get { return 10; } }
		public override int InitMaxHits { get { return 45; } }

		[Constructable]
		public RustyKatana() : base(0x13FF)
		{
			Name = "Rusty Katana";
			Weight = 6.0;
			Hue = 2401;
		}

		public RustyKatana(Serial serial) : base(serial)
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
