using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Items;

namespace Server
{
	public enum SetItem
	{
		None,
		Hoboware
	}

	public interface ISetItem
	{
		SetItem SetID { get; }
		int Pieces { get; }
		int SetHue { get; set; }
		bool IsSetItem { get; }
		bool SetEquipped { get; set; }
		bool LastEquipped { get; set; }
		AosSkillBonuses SetSkillBonuses { get; }

	}

	public interface IBeggingAttire
	{
		int SetBonus { get; set; }
	}

	public static class SetHelper
	{
		public static void GetSetProperties(ObjectPropertyList list, ISetItem setItem)
		{
			AosAttributes attrs;

			if (setItem is BaseWeapon)
				attrs = ((BaseWeapon)setItem).Attributes;
			else if (setItem is BaseArmor)
				attrs = ((BaseArmor)setItem).Attributes;
			else if (setItem is BaseClothing)
				attrs = ((BaseClothing)setItem).Attributes;
			else if (setItem is BaseQuiver)
				attrs = ((BaseQuiver)setItem).Attributes;
			else if (setItem is BaseJewel)
				attrs = ((BaseJewel)setItem).Attributes;
			else
				attrs = new AosAttributes(setItem as Item);

			bool full = setItem.SetEquipped;
			int pieces = setItem.Pieces;
			int prop;

			if (setItem is IBeggingAttire)
			{
				IBeggingAttire attire = (IBeggingAttire)setItem;

				if (setItem.SetEquipped && attire.SetBonus > 0)
					list.Add("Begging Bonus: {0}%", attire.SetBonus.ToString());
			}

			if (setItem.SetSkillBonuses.Skill_1_Value != 0)
				list.Add(1072502, "{0}\t{1}", "#" + (1044060 + (int)setItem.SetSkillBonuses.Skill_1_Name), setItem.SetSkillBonuses.Skill_1_Value); // ~1_skill~ ~2_val~ (total)

		}

		public static void RemoveSetBonus(Mobile from, SetItem setID, Item item)
		{
			bool self = false;

			for (int i = 0; i < from.Items.Count; i++)
			{
				if (from.Items[i] == item)
					self = true;
				Remove(from, setID, from.Items[i]);
			}

			if (!self)
				Remove(from, setID, item);

		}

		public static void Remove (Mobile from, SetItem setID, Item item)
		{
			if (item is ISetItem)
			{
				ISetItem setItem = (ISetItem)item;

				if (setItem.IsSetItem && setItem.SetID == setID)
				{
					if (setItem.LastEquipped)
					{
						if (from != null)
							RemoveStatBonuses(from, item);

						setItem.SetSkillBonuses.Remove();
					}
					int temp = item.Hue;
					item.Hue = setItem.SetHue;
					setItem.SetHue = temp;

					setItem.SetEquipped = false;
					setItem.LastEquipped = false;
					item.InvalidateProperties();
				}
			}
		}

		public static void AddSetBonus (Mobile to, SetItem setID)
		{
			int temp;

			for (int i = 0; i < to.Items.Count; i++)
			{
				if (to.Items[i] is ISetItem)
				{
					ISetItem setItem = (ISetItem)to.Items[i];

					if (setItem.IsSetItem && setItem.SetID == setID)
					{
						if (setItem.LastEquipped)
						{
							setItem.SetSkillBonuses.AddTo(to);
						}
						temp = to.Items[i].Hue;
						to.Items[i].Hue = setItem.SetHue;
						setItem.SetHue = temp;
						setItem.SetEquipped = true;
						Timer.DelayCall<Item>(item => item.InvalidateProperties(), to.Items[i]);
					}
				}
					
			}

			Effects.PlaySound(to.Location, to.Map, 0x1F7);
			to.FixedParticles(0x376A, 9, 32, 5030, EffectLayer.Waist);
			to.SendLocalizedMessage(1072391); // The magic of your armor combines to assist you!

		}

		public static bool FullSetEquipped(Mobile from, SetItem setID, int pieces)
		{
			int equipped = 0;

			for (int i = 0; i < from.Items.Count && equipped < pieces; i++)
			{
				if (from.Items[i] is ISetItem)
				{
					ISetItem setItem = (ISetItem)from.Items[i];

					if (setItem.IsSetItem && setItem.SetID == setID)
						equipped += 1;
				}
			}

			if (equipped == pieces)
				return true;

			return false;
		}

		public static void RemoveStatBonuses(Mobile from, Item item)
		{
			string modName = item.Serial.ToString();

			from.RemoveStatMod(modName + "SetStr");
			from.RemoveStatMod(modName + "SetDex");
			from.RemoveStatMod(modName + "SetInt");

			from.Delta(MobileDelta.Armor);
			from.CheckStatTimers();
		}
		public static void AddStatBonuses(Mobile to, Item item, int str, int dex, int intel)
		{
			if ((str != 0 || dex != 0 || intel != 0))
			{
				string modName = item.Serial.ToString();

				if (str != 0)
					to.AddStatMod(new StatMod(StatType.Str, modName + "SetStr", str, TimeSpan.Zero));

				if (dex != 0)
					to.AddStatMod(new StatMod(StatType.Dex, modName + "SetDex", dex, TimeSpan.Zero));

				if (intel != 0)
					to.AddStatMod(new StatMod(StatType.Int, modName + "SetInt", intel, TimeSpan.Zero));
			}

			to.CheckStatTimers();
		}

	}

}
