using System;
using Server;
using Server.Mobiles;
using Server.Gumps;
using System.Collections.Generic;

namespace Server.Engines.Quests.Rewards
{
	public abstract class BaseReward
	{
		private TextDefinition m_Name;

		public TextDefinition Name
		{
			get { return m_Name; }
			set { m_Name = value; }
		}

		public BaseReward(TextDefinition name)
		{
			m_Name = name;
		}

		protected virtual int LabelHeight { get { return 16; } }

		public void WriteToGump(Gump g, int x, ref int y)
		{
			TextDefinition.AddHtmlText(g, x, y, 280, LabelHeight, m_Name, false, false, 0x15F90, 0xBDE784);
		}

		public abstract void AddRewardItems(PlayerMobile pm, List<Item> rewards);
	}
	public class DummyReward : BaseReward
	{
		public DummyReward(TextDefinition name)
			: base(name)
		{
		}

		protected override int LabelHeight { get { return 180; } }

		public override void AddRewardItems(PlayerMobile pm, List<Item> rewards)
		{
		}
	}

	public class ItemReward : BaseReward
	{
		private Type m_Type;
		private int m_Amount;

		public ItemReward()
			: this(null, null)
		{
		}

		public ItemReward(TextDefinition name, Type type)
			: this(name, type, 1)
		{
		}

		public ItemReward(TextDefinition name, Type type, int amount)
			: base(name)
		{
			m_Type = type;
			m_Amount = amount;
		}

		public virtual Item CreateItem()
		{
			Item spawnedItem = null;

			try
			{
				spawnedItem = Activator.CreateInstance(m_Type) as Item;
			}
			catch (Exception e)
			{
				if (QuestSystem.Debug)
					Console.WriteLine("WARNING: ItemReward.CreateItem failed for {0}: {1}", m_Type, e);
			}

			return spawnedItem;
		}

		public override void AddRewardItems(PlayerMobile pm, List<Item> rewards)
		{
			Item reward = CreateItem();

			if (reward == null)
				return;

			if (reward.Stackable)
			{
				if (m_Amount > 1)
					reward.Amount = m_Amount;

				rewards.Add(reward);
			}
			else
			{
				for (int i = 0; i < m_Amount; ++i)
				{
					rewards.Add(reward);

					if (i < m_Amount - 1)
					{
						reward = CreateItem();

						if (reward == null)
							return;
					}
				}
			}
		}
	}

}
