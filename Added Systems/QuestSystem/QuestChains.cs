#region References
using System;
#endregion

namespace Server.Engines.Quests
{
	public enum QuestChain
	{
		None = 0,
		Sprinklers = 1,
	}

	public class BaseChain
	{
		public Type CurrentQuest { get; set; }
		public Type Quester { get; set; }

		public BaseChain(Type currentQuest, Type quester)
		{
			CurrentQuest = currentQuest;
			Quester = quester;
		}
	}
}
