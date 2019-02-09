using System;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.Quests.Items
{
	public abstract class QuestGiversItem : Item, IQuestGivers
	{
		private List<Quests> m_Quests;

		public List<Quests> Quests
		{
			get
			{
				if (m_Quests == null)
				{
					m_Quests = QuestSystem.FindQuestList(GetType());

					if (m_Quests == null)
						m_Quests = QuestSystem.EmptyList;
				}

				return m_Quests;
			}
		}

		public bool CanGiveQuest { get { return (Quests.Count != 0); } }

		public QuestGiversItem(int itemId)
			: base(itemId)
		{
		}

		public override bool Nontransferable { get { return true; } }

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);

			AddQuestItemProperty(list);

			if (CanGiveQuest)
				list.Add(1072269); // Quest Giver
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (!from.InRange(GetWorldLocation(), 2))
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
			else if (!IsChildOf(from.Backpack))
				from.SendLocalizedMessage(1042593); // That is not in your backpack.
			else if (CanGiveQuest && from is PlayerMobile)
				QuestSystem.OnDoubleClick(this, (PlayerMobile)from);
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			QuestSystem.HandleDeletion(this);
		}

		public QuestGiversItem(Serial serial)
			: base(serial)
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

	public abstract class TransientQuestGiverItem : TransientItem, IQuestGivers
	{
		private List<Quests> m_Quests;

		public List<Quests> Quests
		{
			get
			{
				if (m_Quests == null)
				{
					m_Quests = QuestSystem.FindQuestList(GetType());

					if (m_Quests == null)
						m_Quests = QuestSystem.EmptyList;
				}

				return m_Quests;
			}
		}

		public bool CanGiveQuest { get { return (Quests.Count != 0); } }

		public TransientQuestGiverItem(int itemId, TimeSpan lifeSpan)
			: base(itemId, lifeSpan)
		{
		}

		public override bool Nontransferable { get { return true; } }

		public override void HandleInvalidTransfer(Mobile from)
		{
		}

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);

			AddQuestItemProperty(list);

			if (CanGiveQuest)
				list.Add(1072269); // Quest Giver
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (!from.InRange(GetWorldLocation(), 2))
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
			else if (!IsChildOf(from.Backpack))
				from.SendLocalizedMessage(1042593); // That is not in your backpack.
			else if (CanGiveQuest && from is PlayerMobile)
				QuestSystem.OnDoubleClick(this, (PlayerMobile)from);
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			QuestSystem.HandleDeletion(this);
		}

		public TransientQuestGiverItem(Serial serial)
			: base(serial)
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
