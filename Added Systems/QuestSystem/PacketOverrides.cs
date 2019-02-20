using System;
using Server.Mobiles;
using Server.Network;
using Server.Items;

namespace Server.Engines.Quests
{
	public class PacketOverrides
	{
		public static void Initialize()
		{
			Timer.DelayCall(TimeSpan.Zero, new TimerCallback(Override));
		}

		public static void Override()
		{
			PacketHandlers.RegisterEncoded(0x32, true, new OnEncodedPacketReceive(QuestButton));

		}

		public static void QuestButton(NetState state, IEntity e, EncodedReader reader)
		{
			if (state.Mobile is PlayerMobile)
			{
				PlayerMobile from = (PlayerMobile)state.Mobile;

				from.CloseGump(typeof(NewQuestGump));
				from.SendGump(new NewQuestGump(from));
			}
		}

	
	}
}
