#region References
using System;
using Server.Items;
#endregion

namespace Server.Engines.Quests
{
	public class BaseReward
	{
		public BaseReward(object name)
			: this(null, 1, name)
		{ }

		public BaseReward(Type type, object name)
			: this(type, 1, name)
		{ }

		public BaseReward(Type type, int amount, object name)
		{
			Type = type;
			Amount = amount;
			Name = name;
		}

		public BaseQuest Quest { get; set; }

		public Type Type { get; set; }

		public int Amount { get; set; }

		public object Name { get; set; }

		public virtual void GiveReward()
		{ }
	}
}
