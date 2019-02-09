using System;
using System.Collections.Generic;
using System.Text;
using Server.Mobiles;
using Server.Engines.Quests.Objectives;

namespace Server.Engines.Quests
{
	[Flags]
	public enum QuestFlag
	{
		None = 0x00,
		
	}

	[PropertyObject]
	public class QuestContext
	{
		private class DoneQuestInfo
		{
			public Quests m_Quest;
			public DateTime m_NextAvailable;

			public DoneQuestInfo(Quests quest, DateTime nextAvailable)
			{
				m_Quest = quest;
				m_NextAvailable = nextAvailable;
			}

			public void Serialize(GenericWriter writer)
			{
				QuestSystem.WriteQuestRef(writer, m_Quest);
				writer.Write(m_NextAvailable);
			}

			public static DoneQuestInfo Deserialize(GenericReader reader, int version)
			{
				Quests quest = QuestSystem.ReadQuestRef(reader);
				DateTime nextAvailable = reader.ReadDateTime();

				if (quest == null || !quest.RecordCompletion)
					return null; // forget about this record

				return new DoneQuestInfo(quest, nextAvailable);
			}
		}

		private PlayerMobile m_Owner;
		private List<QuestInstance> m_QuestInstances;
		private List<DoneQuestInfo> m_DoneQuests;
		private List<Quests> m_ChainOffers;
		private QuestFlag m_Flags;

		public PlayerMobile Owner
		{
			get { return m_Owner; }
		}

		public List<QuestInstance> QuestInstances
		{
			get { return m_QuestInstances; }
		}

		public List<Quests> ChainOffers
		{
			get { return m_ChainOffers; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsFull
		{
			get { return m_QuestInstances.Count >= QuestSystem.MaxConcurrentQuestsAllowed; }
		}

		public QuestContext(PlayerMobile owner)
		{
			m_Owner = owner;
			m_QuestInstances = new List<QuestInstance>();
			m_DoneQuests = new List<DoneQuestInfo>();
			m_ChainOffers = new List<Quests>();
			m_Flags = QuestFlag.None;
		}

		public bool HasDoneQuest(Type questType)
		{
			Quests quest = QuestSystem.FindQuest(questType);

			return (quest != null && HasDoneQuest(quest));
		}

		public bool HasDoneQuest(Quests quest)
		{
			foreach (DoneQuestInfo info in m_DoneQuests)
			{
				if (info.m_Quest == quest)
					return true;
			}

			return false;
		}

		public bool HasDoneQuest(Quests quest, out DateTime nextAvailable)
		{
			nextAvailable = DateTime.MinValue;

			foreach (DoneQuestInfo info in m_DoneQuests)
			{
				if (info.m_Quest == quest)
				{
					nextAvailable = info.m_NextAvailable;
					return true;
				}
			}

			return false;
		}

		public void SetDoneQuest(Quests quest)
		{
			SetDoneQuest(quest, DateTime.MinValue);
		}

		public void SetDoneQuest(Quests quest, DateTime nextAvailable)
		{
			foreach (DoneQuestInfo info in m_DoneQuests)
			{
				if (info.m_Quest == quest)
				{
					info.m_NextAvailable = nextAvailable;
					return;
				}
			}

			m_DoneQuests.Add(new DoneQuestInfo(quest, nextAvailable));
		}

		public void RemoveDoneQuest(Quests quest)
		{
			for (int i = m_DoneQuests.Count - 1; i >= 0; --i)
			{
				DoneQuestInfo info = m_DoneQuests[i];

				if (info.m_Quest == quest)
					m_DoneQuests.RemoveAt(i);
			}
		}

		public void HandleDeath()
		{
			for (int i = m_QuestInstances.Count - 1; i >= 0; --i)
				m_QuestInstances[i].OnPlayerDeath();
		}

		public void HandleDeletion()
		{
			for (int i = m_QuestInstances.Count - 1; i >= 0; --i)
				m_QuestInstances[i].Remove();
		}

		public QuestInstance FindInstance(Type questType)
		{
			Quests quest = QuestSystem.FindQuest(questType);

			if (quest == null)
				return null;

			return FindInstance(quest);
		}

		public QuestInstance FindInstance(Quests quest)
		{
			foreach (QuestInstance instance in m_QuestInstances)
			{
				if (instance.Quest == quest)
					return instance;
			}

			return null;
		}

		public bool IsDoingQuest(Type questType)
		{
			Quests quest = QuestSystem.FindQuest(questType);

			return (quest != null && IsDoingQuest(quest));
		}

		public bool IsDoingQuest(Quests quest)
		{
			return (FindInstance(quest) != null);
		}

		public void Serialize(GenericWriter writer)
		{
			// Version info is written in MLQuestPersistence.Serialize

			writer.WriteMobile<PlayerMobile>(m_Owner);
			writer.Write(m_QuestInstances.Count);

			foreach (QuestInstance instance in m_QuestInstances)
				instance.Serialize(writer);

			writer.Write(m_DoneQuests.Count);

			foreach (DoneQuestInfo info in m_DoneQuests)
				info.Serialize(writer);

			writer.Write(m_ChainOffers.Count);

			foreach (Quests quest in m_ChainOffers)
				QuestSystem.WriteQuestRef(writer, quest);

			writer.WriteEncodedInt((int)m_Flags);
		}

		public QuestContext(GenericReader reader, int version)
		{
			m_Owner = reader.ReadMobile<PlayerMobile>();
			m_QuestInstances = new List<QuestInstance>();
			m_DoneQuests = new List<DoneQuestInfo>();
			m_ChainOffers = new List<Quests>();

			int instances = reader.ReadInt();

			for (int i = 0; i < instances; ++i)
			{
				QuestInstance instance = QuestInstance.Deserialize(reader, version, m_Owner);

				if (instance != null)
					m_QuestInstances.Add(instance);
			}

			int doneQuests = reader.ReadInt();

			for (int i = 0; i < doneQuests; ++i)
			{
				DoneQuestInfo info = DoneQuestInfo.Deserialize(reader, version);

				if (info != null)
					m_DoneQuests.Add(info);
			}

			int chainOffers = reader.ReadInt();

			for (int i = 0; i < chainOffers; ++i)
			{
				Quests quest = QuestSystem.ReadQuestRef(reader);

				if (quest != null && quest.IsChainTriggered)
					m_ChainOffers.Add(quest);
			}

			m_Flags = (QuestFlag)reader.ReadEncodedInt();
		}

		public bool GetFlag(QuestFlag flag)
		{
			return ((m_Flags & flag) != 0);
		}

		public void SetFlag(QuestFlag flag, bool value)
		{
			if (value)
				m_Flags |= flag;
			else
				m_Flags &= ~flag;
		}
	}
}
