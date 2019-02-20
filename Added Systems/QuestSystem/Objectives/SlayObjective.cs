using System;


namespace Server.Engines.Quests
{
	public class SlayObjective : BaseObjective
	{
		private Type[] m_Creatures;
		private string m_Name;
		private Region m_Region;

		public SlayObjective(Type creature, string name, int amount)
			: this(new Type[] { creature }, name, amount, null, 0)
		{
		}

		public SlayObjective(Type creature, string name, int amount, string region)
			: this(new Type[] { creature }, name, amount, region, 0)
		{
		}

		public SlayObjective(Type creature, string name, int amount, int seconds)
			: this(new Type[] { creature }, name, amount, null, seconds)
		{
		}

		public SlayObjective(string name, int amount, params Type[] creatures)
			: this(creatures, name, amount, null, 0)
		{
		}

		public SlayObjective(string name, int amount, string region, params Type[] creatures)
			: this(creatures, name, amount, region, 0)
		{
		}

		public SlayObjective(string name, int amount, int seconds, params Type[] creatures)
			: this(creatures, name, amount, null, seconds)
		{
		}

		public SlayObjective(Type[] creatures, string name, int amount, string region, int seconds)
			: base(amount, seconds)
		{
			m_Creatures = creatures;
			m_Name = name;

			if (region != null)
			{
				m_Region = QuestHelper.FindRegion(region);

				if (m_Region == null)
					Console.WriteLine(String.Format("Invalid region name ('{0}') in '{1}' objective!", region, GetType()));
			}
		}

		public Type[] Creatures { get { return m_Creatures;	} set {	m_Creatures = value; }}
		public string Name { get { return m_Name; } set	{ m_Name = value; }}
		public Region Region {	get	{ return m_Region; } set {	m_Region = value; }}

		public virtual void OnKill(Mobile killed)
		{
			if (Completed)
				Quest.Owner.SendMessage("You killed all the {0} required for this Quest.", killed.Name); // You have killed all the required quest creatures of this type.
			else
				Quest.Owner.SendMessage("You killed {0}. You have {1}/{2} Left.", killed.Name, (MaxProgress - CurProgress).ToString(), MaxProgress.ToString()); // You have killed a quest creature. ~1_val~ more left.
		}

		public virtual bool IsObjective(Mobile mob)
		{
			if (m_Creatures == null)
				return false;

			foreach (var type in m_Creatures)
			{
				if (type.IsAssignableFrom(mob.GetType()))
				{
					if (m_Region != null && !m_Region.Contains(mob.Location))
						return false;

					return true;
				}
			}

			return false;
		}

		public override bool Update(object obj)
		{
			if (obj is Mobile)
			{
				Mobile mob = (Mobile)obj;

				if (IsObjective(mob))
				{
					if (!Completed)
						CurProgress += 1;
					OnKill(mob);
					return true;
				}
			}

			return false;
		}

		public override Type Type()
		{
			return m_Creatures != null && m_Creatures.Length > 0 ? m_Creatures[0] : null;
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
