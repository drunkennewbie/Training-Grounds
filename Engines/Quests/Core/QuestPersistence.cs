
//Updated Quest
namespace Server.Engines.Quests
{
	public class QuestPersistence : Item
	{
		private static QuestPersistence m_Instance;

		public static void EnsureExistence()
		{
			if (m_Instance == null)
				m_Instance = new QuestPersistence();
		}

		public override string DefaultName
		{
			get { return "ML quests persistence - Internal"; }
		}

		private QuestPersistence()
			: base(1)
		{
			Movable = false;
		}

		public QuestPersistence(Serial serial) : base(serial)
		{
			m_Instance = this;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)2); // version
			writer.Write(QuestSystem.Contexts.Count);

			foreach (QuestContext context in QuestSystem.Contexts.Values)
				context.Serialize(writer);

			writer.Write(QuestSystem.Quests.Count);

			foreach (Quests quest in QuestSystem.Quests.Values)
				Quests.Serialize(writer, quest);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
			int contexts = reader.ReadInt();

			for (int i = 0; i < contexts; ++i)
			{
				QuestContext context = new QuestContext(reader, version);

				if (context.Owner != null)
					QuestSystem.Contexts[context.Owner] = context;
			}

			int quests = reader.ReadInt();

			for (int i = 0; i < quests; ++i)
				Quests.Deserialize(reader, version);
		}
	}
}
