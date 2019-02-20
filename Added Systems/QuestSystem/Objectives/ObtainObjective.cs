using System;


namespace Server.Engines.Quests
{
	public class ObtainObjective : BaseObjective
	{
		private Type m_Obtain;
		private string m_Name;
		private int m_Image;
		private int m_Hue;

		public ObtainObjective(Type obtain, string name, int amount)
			: this(obtain, name, amount, 0, 0)
		{
		}

		public ObtainObjective(Type obtain, string name, int amount, int image)
			: this(obtain, name, amount, image, 0)
		{
		}

		public ObtainObjective(Type obtain, string name, int amount, int image, int seconds)
			: this(obtain, name, amount, image, seconds, 0)
		{
		}

		public ObtainObjective(Type obtain, string name, int amount, int image, int seconds, int hue)
			: base(amount, seconds)
		{
			m_Obtain = obtain;
			m_Name = name;
			m_Image = image;
			m_Hue = hue;
		}

		public Type Obtain { get { return m_Obtain; } set {	m_Obtain = value;}}
		public string Name { get { return m_Name; } set { m_Name = value; }}
		public int Image { get { return m_Image; } set { m_Image = value; }}
		public int Hue	{ get {	return m_Hue; } set { m_Hue = value; }}

		public override bool Update(object obj)
		{
			if (obj is Item)
			{
				Item obtained = (Item)obj;

				if (IsObjective(obtained))
				{
					if (!obtained.QuestItem)
					{
						CurProgress += obtained.Amount;

						obtained.QuestItem = true;
						Quest.Owner.SendLocalizedMessage(1072353); // You set the item to Quest Item status

						Quest.OnObjectiveUpdate(obtained);
					}
					else
					{
						CurProgress -= obtained.Amount;

						obtained.QuestItem = false;
						Quest.Owner.SendLocalizedMessage(1072354); // You remove Quest Item status from the item
					}

					return true;
				}
			}

			return false;
		}

		public virtual bool IsObjective(Item item)
		{
			if (m_Obtain == null)
				return false;

			if (m_Obtain.IsAssignableFrom(item.GetType()))
				return true;

			return false;
		}

		public override Type Type()
		{
			return m_Obtain;
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
