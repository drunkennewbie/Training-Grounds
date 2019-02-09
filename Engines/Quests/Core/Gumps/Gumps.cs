using System.Collections.Generic;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;


namespace Server.Engines.Quests.Gumps
{
	public class InfoNPCGump : BaseQuestGump
	{
		public InfoNPCGump(TextDefinition title, TextDefinition message)
			: base(1060668) // INFORMATION
		{
			RegisterButton(ButtonPosition.Left, ButtonGraphic.Close, 3);

			SetPageCount(1);

			BuildPage();
			TextDefinition.AddHtmlText(this, 160, 108, 250, 16, title, false, false, 0x2710, 0x4AC684);
			TextDefinition.AddHtmlText(this, 98, 156, 312, 180, message, false, true, 0x15F90, 0xBDE784);
		}
	}
	public class NQuestOfferGump : BaseQuestGump
	{
		private Quests m_Quest;
		private IQuestGivers m_Quester;

		public NQuestOfferGump(Quests quest, IQuestGivers quester, PlayerMobile pm)
			: base(1049010) // Quest Offer
		{
			m_Quest = quest;
			m_Quester = quester;

			CloseOtherGumps(pm);
			pm.CloseGump(typeof(QuestOfferGump));

			SetTitle(quest.Title);
			RegisterButton(ButtonPosition.Left, ButtonGraphic.Accept, 1);
			RegisterButton(ButtonPosition.Right, ButtonGraphic.Refuse, 2);

			SetPageCount(3);

			BuildPage();
			AddDescription(quest);

			BuildPage();
			AddObjectives(quest);

			BuildPage();
			AddRewardsPage(quest);
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			PlayerMobile pm = sender.Mobile as PlayerMobile;

			if (pm == null)
				return;

			switch (info.ButtonID)
			{
				case 1: // Accept
					{
						m_Quest.OnAccept(m_Quester, pm);
						break;
					}
				case 2: // Refuse
					{
						m_Quest.OnRefuse(m_Quester, pm);
						break;
					}
			}
		}
	}
	public class QuestLogGump : BaseQuestGump
	{
		private PlayerMobile m_Owner;
		private bool m_CloseGumps;

		public QuestLogGump(PlayerMobile pm)
			: this(pm, true)
		{
		}

		public QuestLogGump(PlayerMobile pm, bool closeGumps)
			: base(1046026) // Quest Log
		{
			m_Owner = pm;
			m_CloseGumps = closeGumps;

			if (closeGumps)
			{
				pm.CloseGump(typeof(QuestLogGump));
				pm.CloseGump(typeof(QuestLogDetailedGump));
			}

			RegisterButton(ButtonPosition.Right, ButtonGraphic.Okay, 3);

			SetPageCount(1);

			BuildPage();

			int numberColor, stringColor;

			QuestContext context = QuestSystem.GetContext(pm);

			if (context != null)
			{
				List<QuestInstance> instances = context.QuestInstances;

				for (int i = 0; i < instances.Count; ++i)
				{
					if (instances[i].Failed)
					{
						numberColor = 0x3C00;
						stringColor = 0x7B0000;
					}
					else
					{
						numberColor = stringColor = 0xFFFFFF;
					}

					TextDefinition.AddHtmlText(this, 98, 140 + 21 * i, 270, 21, instances[i].Quest.Title, false, false, numberColor, stringColor);
					AddButton(368, 140 + 21 * i, 0x26B0, 0x26B1, 6 + i, GumpButtonType.Reply, 1);
				}
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (info.ButtonID < 6)
				return;

			QuestContext context = QuestSystem.GetContext(m_Owner);

			if (context == null)
				return;

			List<QuestInstance> instances = context.QuestInstances;
			int index = info.ButtonID - 6;

			if (index >= instances.Count)
				return;

			sender.Mobile.SendGump(new QuestLogDetailedGump(instances[index], m_CloseGumps));
		}
	}
	public class QuestLogDetailedGump : BaseQuestGump
	{
		private QuestInstance m_Instance;
		private bool m_CloseGumps;

		public QuestLogDetailedGump(QuestInstance instance)
			: this(instance, true)
		{
		}

		public QuestLogDetailedGump(QuestInstance instance, bool closeGumps)
			: base(1046026) // Quest Log
		{
			m_Instance = instance;
			m_CloseGumps = closeGumps;

			PlayerMobile pm = instance.Player;
			Quests quest = instance.Quest;

			if (closeGumps)
			{
				CloseOtherGumps(pm);
				pm.CloseGump(typeof(QuestLogDetailedGump));
			}

			SetTitle(quest.Title);
			RegisterButton(ButtonPosition.Left, ButtonGraphic.Resign, 1);
			RegisterButton(ButtonPosition.Right, ButtonGraphic.Okay, 2);

			SetPageCount(3);

			BuildPage();
			AddDescription(quest);

			if (instance.Failed) // only displayed on the first page
				AddHtmlLocalized(160, 80, 250, 16, 500039, 0x3C00, false, false); // Failed!

			BuildPage();
			AddObjectivesProgress(instance);

			BuildPage();
			AddRewardsPage(quest);
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (m_Instance.Removed)
				return;

			switch (info.ButtonID)
			{
				case 1: // Resign
					{
						// TODO: Custom reward loss protection? OSI doesn't have this
						//if ( m_Instance.ClaimReward )
						//	pm.SendMessage( "You cannot cancel a quest with rewards pending." );
						//else

						sender.Mobile.SendGump(new QuestCancelConfirmGump(m_Instance, m_CloseGumps));

						break;
					}
				case 2: // Okay
					{
						sender.Mobile.SendGump(new QuestLogGump(m_Instance.Player, m_CloseGumps));

						break;
					}
			}
		}
	}
	public class QuestConversationGump : BaseQuestGump
	{
		public QuestConversationGump(Quests quest, PlayerMobile pm, TextDefinition text)
			: base(3006156) // Quest Conversation
		{
			CloseOtherGumps(pm);

			SetTitle(quest.Title);
			RegisterButton(ButtonPosition.Right, ButtonGraphic.Close, 3);

			SetPageCount(1);

			BuildPage();
			AddConversation(text);
		}
	}
	public class QuestCancelConfirmGump : Gump
	{
		private QuestInstance m_Instance;
		private bool m_CloseGumps;

		public QuestCancelConfirmGump(QuestInstance instance)
			: this(instance, true)
		{
		}

		public QuestCancelConfirmGump(
		QuestInstance instance, bool closeGumps)
			: base(120, 50)
		{
			m_Instance = instance;
			m_CloseGumps = closeGumps;

			if (closeGumps)
				BaseQuestGump.CloseOtherGumps(instance.Player);

			AddPage(0);

			Closable = false;

			AddImageTiled(0, 0, 348, 262, 0xA8E);
			AddAlphaRegion(0, 0, 348, 262);

			AddImage(0, 15, 0x27A8);
			AddImageTiled(0, 30, 17, 200, 0x27A7);
			AddImage(0, 230, 0x27AA);

			AddImage(15, 0, 0x280C);
			AddImageTiled(30, 0, 300, 17, 0x280A);
			AddImage(315, 0, 0x280E);

			AddImage(15, 244, 0x280C);
			AddImageTiled(30, 244, 300, 17, 0x280A);
			AddImage(315, 244, 0x280E);

			AddImage(330, 15, 0x27A8);
			AddImageTiled(330, 30, 17, 200, 0x27A7);
			AddImage(330, 230, 0x27AA);

			AddImage(333, 2, 0x2716);
			AddImage(333, 248, 0x2716);
			AddImage(2, 248, 0x2716);
			AddImage(2, 2, 0x2716);

			AddHtmlLocalized(25, 22, 200, 20, 1049000, 0x7D00, false, false); // Confirm Quest Cancellation
			AddImage(25, 40, 0xBBF);

			/*
			 * This quest will give you valuable information, skills
			 * and equipment that will help you advance in the
			 * game at a quicker pace.<BR>
			 * <BR>
			 * Are you certain you wish to cancel at this time?
			 */
			AddHtmlLocalized(25, 55, 300, 120, 1060836, 0xFFFFFF, false, false);

			Quests quest = instance.Quest;

			if (quest.IsChainTriggered || quest.NextQuest != null)
			{
				AddRadio(25, 145, 0x25F8, 0x25FB, false, 2);
				AddHtmlLocalized(60, 150, 280, 20, 1075023, 0xFFFFFF, false, false); // Yes, I want to quit this entire chain!
			}

			AddRadio(25, 180, 0x25F8, 0x25FB, true, 1);
			AddHtmlLocalized(60, 185, 280, 20, 1049005, 0xFFFFFF, false, false); // Yes, I really want to quit this quest!

			AddRadio(25, 215, 0x25F8, 0x25FB, false, 0);
			AddHtmlLocalized(60, 220, 280, 20, 1049006, 0xFFFFFF, false, false); // No, I don't want to quit.

			AddButton(265, 220, 0xF7, 0xF8, 7, GumpButtonType.Reply, 0);
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (m_Instance.Removed)
				return;

			switch (info.ButtonID)
			{
				case 7: // Okay
					{
						if (info.IsSwitched(2))
							m_Instance.Cancel(true);
						else if (info.IsSwitched(1))
							m_Instance.Cancel(false);

						sender.Mobile.SendGump(new QuestLogGump(m_Instance.Player, m_CloseGumps));
						break;
					}
			}
		}
	}
	public class QuestRewardGump : BaseQuestGump
	{
		private QuestInstance m_Instance;

		public QuestRewardGump(QuestInstance instance)
			: base(1072201) // Reward
		{
			m_Instance = instance;

			Quests quest = instance.Quest;
			PlayerMobile pm = instance.Player;

			CloseOtherGumps(pm);

			SetTitle(quest.Title);
			RegisterButton(ButtonPosition.Left, ButtonGraphic.Accept, 1);

			SetPageCount(1);

			BuildPage();
			AddRewards(quest);
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (info.ButtonID == 1)
				m_Instance.ClaimRewards();
		}
	}
	public class QuestReportBackGump : BaseQuestGump
	{
		private QuestInstance m_Instance;

		public QuestReportBackGump(QuestInstance instance)
			: base(3006156) // Quest Conversation
		{
			m_Instance = instance;

			Quests quest = instance.Quest;
			PlayerMobile pm = instance.Player;

			// TODO: Check close sequence
			CloseOtherGumps(pm);

			SetTitle(quest.Title);
			RegisterButton(ButtonPosition.Left, ButtonGraphic.Continue, 4);
			RegisterButton(ButtonPosition.Right, ButtonGraphic.Close, 3);

			SetPageCount(1);

			BuildPage();
			AddConversation(quest.CompletionMessage);
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (info.ButtonID == 4)
				m_Instance.ContinueReportBack(true);
		}
	}



}
