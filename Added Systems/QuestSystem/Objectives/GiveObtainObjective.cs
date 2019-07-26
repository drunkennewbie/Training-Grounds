using System;


namespace Server.Engines.Quests
{
	public class GiveObtainObjective : BaseObjective
	{
		private Type m_Delob;
		private string m_DelName;
		private int m_DelAmount;
		private Type m_Obtain;
		private string m_Name;
		private int m_Image;
		private int m_Hue;

		public GiveObtainObjective(Type delob, string delname, int delamount, Type obtain, string name, int amount)
			: this(delob, delname, delamount, obtain, name, amount, 0, 0)
		{
		}

		public GiveObtainObjective(Type delob, string delname, int delamount, Type obtain, string name, int amount, int image)
			: this(delob, delname, delamount, obtain, name, amount, image, 0)
		{
		}

		public GiveObtainObjective(Type delob, string delname, int delamount, Type obtain, string name, int amount, int image, int seconds)
			: this(delob, delname, delamount, obtain, name, amount, image, seconds, 0)
		{
		}

		public GiveObtainObjective(Type delob, string delname, int delamount, Type obtain, string name, int amount, int image, int seconds, int hue)
			: base(amount, seconds)
		{
			m_Delob = delob;
			m_DelName = delname;
			m_DelAmount = delamount;
			m_Obtain = obtain;
			m_Name = name;
			m_Image = image;
			m_Hue = hue;
		}

		public Type Obtain { get { return m_Obtain; } set {	m_Obtain = value;}}
		public string Name { get { return m_Name; } set { m_Name = value; }}
		public int Image { get { return m_Image; } set { m_Image = value; }}
		public int Hue	{ get {	return m_Hue; } set { m_Hue = value; }}

		public override void OnAccept()
		{
			if (Quest.StartingItem != null)
			{
				Quest.StartingItem.QuestItem = true;
				return;
			}

			int delamount = MaxProgress;

			while (delamount > 0 && !Failed)
			{
				Item item = QuestHelper.Construct(m_Delob) as Item;

				if (item == null)
				{
					Fail();
					break;
				}

				if (item.Stackable)
				{
					item.Amount = delamount;
					delamount = 1;
				}

				if (!Quest.Owner.PlaceInBackpack(item))
				{
					Quest.Owner.SendLocalizedMessage(503200); // You do not have room in your backpack for 
					Quest.Owner.SendLocalizedMessage(1075574); // Could not create all the necessary items. Your quest has not advanced.

					Fail();

					break;
				}
				else
					item.QuestItem = true;

				delamount -= 1;
			}

			if (Failed)
			{
				QuestHelper.DeleteItems(Quest.Owner, m_Delob, MaxProgress - delamount, false);

				Quest.RemoveQuest();
			}
		}

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
