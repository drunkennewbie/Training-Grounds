using System;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName("a clowder corpse")]

	public class CatHerder : BaseCreature
	{
		private DateTime m_NextHide;

		[Constructable]
		public CatHerder() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
		{
			Name = "a clowder";
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
			SetSkill(SkillName.Hiding, 80.0, 90.0);
			SetSkill(SkillName.Stealth, 60.0, 80.0);

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
		public override bool CanStealth { get { return true; } }

		public override double GetControlChance(Mobile m, bool useBaseSkill)
		{
			Console.Write(GetHerdConntrolChance(m));
			return GetHerdConntrolChance(m);
		}

		public CatHerder(Serial serial) : base(serial)
		{
		}

		public override void OnThink()
		{
			if (!this.Alive || this.Deleted)
			{
				return;
			}
			if (!this.Hidden)
			{
				double chance = 0.05; //5%
				if (this.Hits < 40)
				{
					chance = 0.1;
				}

				if (this.Poisoned)
				{
					chance = 0.01;
				}
				
				if (!this.Controlled)
				{
					chance = 0.80; //10% chance to hide if not tamed
				}

				if (DateTime.UtcNow > m_NextHide && Utility.RandomDouble() < chance)
				{
					HideSelf();
				}
			}


			base.OnThink();
		}

		public override void OnDamage(int amount, Mobile from, bool willKill)
		{
			RevealingAction();
			base.OnDamage(amount, from, willKill);
		}

		public override void OnDamagedBySpell(Mobile from)
		{
			RevealingAction();
			base.OnDamagedBySpell(from);
		}

		private void HideSelf()
		{
			if (Core.TickCount >= this.NextSkillTime)
			{
				
				this.UseSkill(SkillName.Hiding);
				m_NextHide = DateTime.UtcNow + TimeSpan.FromSeconds(30);
			}
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
