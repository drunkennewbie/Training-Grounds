using System;
using Server.Mobiles;

namespace Server.Mobiles
{
	[CorpseName("a hell cat corpse")]
	[TypeAlias("Server.Mobiles.Preditorhellcat")]
	public class CatHerder : BaseCreature
	{
		[Constructable]
		public CatHerder() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
		{
			Name = "a herding cat";
			Body = 127;
			BaseSoundID = 0xBA;

			SetStr(161, 185);
			SetDex(96, 115);
			SetInt(10, 10);

			SetHits(97, 131);

			SetDamage(5, 17);

			SetDamageType(ResistanceType.Physical, 75);
			SetDamageType(ResistanceType.Fire, 25);

			SetResistance(ResistanceType.Physical, 25, 35);
			SetResistance(ResistanceType.Fire, 30, 40);
			SetResistance(ResistanceType.Energy, 5, 15);

			SetSkill(SkillName.MagicResist, 75.1, 90.0);
			SetSkill(SkillName.Tactics, 50.1, 65.0);
			SetSkill(SkillName.Wrestling, 50.1, 65.0);

			Fame = 2500;
			Karma = -2500;

			VirtualArmor = 30;

			Tamable = true;
			ControlSlots = 1;
			MinTameSkill = 89.1;

		}
				
		public override bool IsBondable { get { return false; } }
		public override int Hides { get { return 10; } }
		public override HideType HideType { get { return HideType.Spined; } }
		public override FoodType FavoriteFood { get { return FoodType.Meat; } }
		public override PackInstinct PackInstinct { get { return PackInstinct.Feline; } }

		public override double GetControlChance(Mobile m, bool useBaseSkill)
		{
			Console.Write(GetHerdConntrolChance(m));
			return GetHerdConntrolChance(m);
		}

		public CatHerder(Serial serial) : base(serial)
		{
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
