using System;


namespace Server.Engines.Quests
{
	public class BaseObjective
	{
		private BaseQuest m_Quest;
		private int m_MaxProgress;
		private int m_CurProgress;
		private int m_Seconds;
		private bool m_Timed;
		public BaseObjective()
			: this(1, 0)
		{
		}

		public BaseObjective(int maxProgress)
			: this(maxProgress, 0)
		{
		}

		public BaseObjective(int maxProgress, int seconds)
		{
			m_MaxProgress = maxProgress;
			m_Seconds = seconds;

			if (seconds > 0)
				Timed = true;
			else
				Timed = false;
		}

		public BaseQuest Quest { get { return m_Quest; } set { m_Quest = value; } }
		public int MaxProgress { get { return m_MaxProgress; } set { m_MaxProgress = value; } }

		public int CurProgress
		{
			get
			{
				return m_CurProgress;
			}
			set
			{
				m_CurProgress = value;

				if (Completed)
					OnCompleted();

				if (m_CurProgress == -1)
					OnFailed();

				if (m_CurProgress < -1)
					m_CurProgress = -1;
			}
		}
		public int Seconds
		{
			get
			{
				return m_Seconds;
			}
			set
			{
				m_Seconds = value;

				if (m_Seconds < 0)
					m_Seconds = 0;
			}
		}
		public bool Timed
		{
			get
			{
				return m_Timed;
			}
			set
			{
				m_Timed = value;
			}
		}
		public bool Completed { get { return CurProgress >= MaxProgress; } }
		public bool Failed { get { return CurProgress == -1; } }

		public virtual object ObjectiveDescription { get { return null; } }

		public virtual void Complete() { CurProgress = MaxProgress; }
		public virtual void Fail() { CurProgress = -1; }

		public virtual void OnAccept() { }

		public virtual void OnCompleted() { }

		public virtual void OnFailed() { }

		public virtual Type Type() { return null; }

		public virtual bool Update(object obj) { return false; }

		public virtual void UpdateTime()
		{
			if (!Timed || Failed)
				return;

			if (Seconds > 0)
			{
				Seconds -= 1;
			}
			else if (!Completed)
			{
				m_Quest.Owner.SendLocalizedMessage(1072258); // You failed to complete an objective in time!

				Fail();
			}
		}

		public virtual void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt((int)0); // version

			writer.Write((int)m_CurProgress);
			writer.Write((int)m_Seconds);
		}

		public virtual void Deserialize(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			m_CurProgress = reader.ReadInt();
			m_Seconds = reader.ReadInt();
		}
	}
}
