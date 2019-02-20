using System.Collections.Generic;


namespace Server.Engines.Quests
{
	public class QuestionAndAnswerObjective : BaseObjective
	{
		private int _CurrentIndex;

		private List<int> m_Done = new List<int>();
		private QuestionAndAnswerEntry[] m_EntryTable;

		public virtual QuestionAndAnswerEntry[] EntryTable { get { return m_EntryTable; } }

		public QuestionAndAnswerObjective(int count, QuestionAndAnswerEntry[] table)
			: base(count)
		{
			m_EntryTable = table;
			_CurrentIndex = -1;
		}

		public QuestionAndAnswerEntry GetRandomQandA()
		{
			if (m_EntryTable == null || m_EntryTable.Length == 0 || m_EntryTable.Length - m_Done.Count <= 0)
				return null;

			if (_CurrentIndex >= 0 && _CurrentIndex < m_EntryTable.Length)
			{
				return m_EntryTable[_CurrentIndex];
			}

			int ran;

			do
			{
				ran = Utility.Random(m_EntryTable.Length);
			}
			while (m_Done.Contains(ran));

			_CurrentIndex = ran;
			return m_EntryTable[ran];
		}

		public override bool Update(object obj)
		{
			m_Done.Add(_CurrentIndex);
			_CurrentIndex = -1;

			if (!Completed)
				CurProgress++;

			return true;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)1); // version

			writer.Write(_CurrentIndex);

			writer.Write(m_Done.Count);
			for (int i = 0; i < m_Done.Count; i++)
				writer.Write(m_Done[i]);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			if (version > 0)
			{
				_CurrentIndex = reader.ReadInt();
			}

			int c = reader.ReadInt();
			for (int i = 0; i < c; i++)
				m_Done.Add(reader.ReadInt());
		}
	}
}
