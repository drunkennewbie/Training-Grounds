using System;
using Server.Items;

namespace Server.Engines.Craft
{

	#region Recipes
	public enum BeverageRecipes
	{

	}
	#endregion

	public class DefBeverage : CraftSystem
	{
		public override SkillName MainSkill { get { return SkillName.Cooking; } }

		public override int GumpTitleNumber
		{
			get { return 1044003; } // <CENTER>COOKING MENU</CENTER>
		}

		private static CraftSystem m_CraftSystem;

		public static CraftSystem CraftSystem
		{
			get
			{
				if (m_CraftSystem == null)
					m_CraftSystem = new DefBeverage();

				return m_CraftSystem;
			}
		}

		public override CraftECA ECA { get { return CraftECA.ChanceMinusSixtyToFourtyFive; } }

		public override double GetChanceAtMin(CraftItem item)
		{
			return 0.0; // 0%
		}

		private DefBeverage() : base(1, 1, 1.25)// base( 1, 1, 1.5 )
		{
		}

		public override int CanCraft(Mobile from, BaseTool tool, Type itemType)
		{
			if (tool == null || tool.Deleted || tool.UsesRemaining < 0)
				return 1044038; // You have worn out your tool!
			return 0;
		}

		public override void PlayCraftEffect(Mobile from)
		{
		}

		public override int PlayEndingEffect(Mobile from, bool failed, bool lostMaterial, bool toolBroken, int quality, bool makersMark, CraftItem item)
		{
			if (failed)
			{
				if (lostMaterial)
				{
					return 1044043; // You failed to create the item, and some of your materials are lost.
				}
				return 1044157; // You failed to create the item, but no materials were lost.
			}
			else
			{
				if (quality == 0)
				{
					return 502785; // You were barely able to make this item.  It's quality is below average.
				}
				else if (quality == 2)
				{
					return 1044155; // You create an exceptional quality item.
				}

				return 1044154; // You create the item.
			}
		}

		public override void InitCraftList()
		{
			int index = -1;
			MarkOption = false; //Enable the Ability to turn on Makers Mark.

			//AddCraft ( Item Type, Definition, Definition Name, Min Skill, Max Skill, ResType, Name, Amount
			//AddRes ( type, name, min skill, max skill )

			/*		#region Beverages - CliLoc - 1155736

					index = AddCraft(typeof(CoffeeMug), 1155736, 1155737, 0.0, 28.58, typeof(CoffeeGrounds), 1155735, 1, 1155734);
					AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
					AddRes(index, typeof(CeramicMug), 1022453, 1, 1044253);
					SetBeverageType(index, BeverageType.Water);
					ForceNonExceptional(index);

					index = AddCraft(typeof(BasketOfGreenTeaMug), 1155736, 1030315, 0.0, 28.58, typeof(GreenTeaBasket), 1155735, 1, 1044253);
					AddRes(index, typeof(BaseBeverage), 1046458, 1, 1044253);
					AddRes(index, typeof(CeramicMug), 1022453, 1, 1044253);
					SetBeverageType(index, BeverageType.Water);
					ForceNonExceptional(index);

					index = AddCraft(typeof(HotCocoaMug), 1155736, 1155738, 0.0, 28.58, typeof(CocoaLiquor), 1080007, 1, 1080006);
					AddRes(index, typeof(SackOfSugar), 1080003, 1, 1080002);
					AddRes(index, typeof(BaseBeverage), 1080011, 1, 1080010);
					AddRes(index, typeof(CeramicMug), 1022453, 1, 1044253);
					SetBeverageType(index, BeverageType.Milk);
					ForceNonExceptional(index);
					#endregion
			*/

		}
	}
}
