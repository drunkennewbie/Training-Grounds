using System;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class DeliverObjective : BaseObjective
	{
		private Type m_Delivery;
		private string m_DeliveryName;
		private Type m_Destination;
		private string m_DestName;

		public DeliverObjective(Type delivery, string deliveryName, int amount, Type destination, string destName)
			: this(delivery, deliveryName, amount, destination, destName, 0)
		{
		}

		public DeliverObjective(Type delivery, string deliveryName, int amount, Type destination, string destName, int seconds)
			: base(amount, seconds)
		{
			m_Delivery = delivery;
			m_DeliveryName = deliveryName;

			m_Destination = destination;
			m_DestName = destName;
		}

		public Type Delivery { get { return m_Delivery;	} set { m_Delivery = value;	}}
		public string DeliveryName { get { return m_DeliveryName; } set { m_DeliveryName = value; }}
		public Type Destination { get { return m_Destination; }	set { m_Destination = value; }}
		public string DestName { get { return m_DestName; } set { m_DestName = value;	}}

		public override void OnAccept()
		{
			if (Quest.StartingItem != null)
			{
				Quest.StartingItem.QuestItem = true;
				return;
			}

			int amount = MaxProgress;

			while (amount > 0 && !Failed)
			{
				Item item = QuestHelper.Construct(m_Delivery) as Item;

				if (item == null)
				{
					Fail();
					break;
				}

				if (item.Stackable)
				{
					item.Amount = amount;
					amount = 1;
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

				amount -= 1;
			}

			if (Failed)
			{
				QuestHelper.DeleteItems(Quest.Owner, m_Delivery, MaxProgress - amount, false);

				Quest.RemoveQuest();
			}
		}

		public override bool Update(object obj)
		{
			if (m_Delivery == null || m_Destination == null)
				return false;

			if (Failed)
			{
				Quest.Owner.SendLocalizedMessage(1074813);  // You have failed to complete your delivery.
				return false;
			}

			if (obj is BaseVendor)
			{
				if (Quest.StartingItem != null)
				{
					Complete();
					return true;
				}
				else if (m_Destination.IsAssignableFrom(obj.GetType()))
				{
					if (MaxProgress < QuestHelper.CountQuestItems(Quest.Owner, Delivery))
					{
						Quest.Owner.SendLocalizedMessage(1074813);  // You have failed to complete your delivery.						
						Fail();
					}
					else
						Complete();

					return true;
				}
			}

			return false;
		}

		public override Type Type()
		{
			return m_Delivery;
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
