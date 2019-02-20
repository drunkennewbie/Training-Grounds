using System;
using System.Linq;

namespace Server.Engines.Quests
{

	public class EscortObjective : BaseObjective
	{
		private Region m_Region;
		private int m_Fame;
		private int m_Compassion;
		public EscortObjective(string region)
			: this(region, 10, 200, 0)
		{
		}

		public EscortObjective(string region, int fame)
			: this(region, fame, 200)
		{
		}

		public EscortObjective(string region, int fame, int compassion)
			: this(region, fame, compassion, 0)
		{
		}

		public EscortObjective(string region, int fame, int compassion, int seconds)
			: base(1, seconds)
		{
			m_Region = QuestHelper.FindRegion(region);
			m_Fame = fame;
			m_Compassion = compassion;

			if (m_Region == null)
				Console.WriteLine(String.Format("Invalid region name ('{0}') in '{1}' objective!", region, GetType()));
		}

		public Region Region { get { return m_Region; }	set { m_Region = value;	}}
		public int Fame { get { return m_Fame; } set { m_Fame = value; }}
		public int Compassion { get { return m_Compassion; } set { 	m_Compassion = value; }}

		public override void OnCompleted()
		{
			base.OnCompleted();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadEncodedInt();
		}
	}

}
