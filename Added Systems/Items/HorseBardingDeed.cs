using System;
using Server;
using Server.Mobiles;
using Server.Targeting;
using Server.Engines.Craft;
namespace Server.Items
{
	[TypeAlias("Server.Items.HorseBarding")]
	public class HorseBardingDeed : Item, ICraftable
	{
		private bool m_Exceptional;
		private Mobile m_Crafter;
		private CraftResource m_Resource;

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Crafter { get { return m_Crafter; } set { m_Crafter = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Exceptional { get { return m_Exceptional; } set { m_Exceptional = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public CraftResource Resource { get { return m_Resource; } set { m_Resource = value; Hue = CraftResources.GetHue(value); InvalidateProperties(); } }

		[Constructable]
		public HorseBardingDeed() : base(0x14F0)
		{
			Name = m_Exceptional ? "Exceptional Horse Barding" : "Horse Barding";
			Weight = 1.0;
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (m_Exceptional && m_Crafter != null)
				list.Add(1050043, m_Crafter.Name); // crafted by ~1_NAME~
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (IsChildOf(from.Backpack))
			{
				from.BeginTarget(6, false, TargetFlags.None, new TargetCallback(OnTarget));
				from.SendMessage("Select the strong dragon you wish to place the barding on.");
			}
			else
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
			}
		}

		public virtual void OnTarget(Mobile from, object obj)
		{
			if (Deleted)
				return;

			BardableHorse pet = obj as BardableHorse;

			if (pet == null || pet.HasBarding)
			{
				from.SendMessage("That is not an unarmored horse.");
			}
			else if (!pet.Controlled || pet.ControlMaster != from)
			{
				from.SendMessage("You can only put barding on a tamed strong horse that you own.");
			}
			else if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1060640); // The item must be in your backpack to use it.
			}
			else
			{
				pet.BardingExceptional = this.Exceptional;
				pet.BardingCrafter = this.Crafter;
				pet.BardingHP = pet.BardingMaxHP;
				pet.BardingResource = this.Resource;
				pet.HasBarding = true;
				pet.Hue = this.Hue;

				this.Delete();

				from.SendMessage("You place the barding on your strong horse.  Use a bladed item on your dragon to remove the armor.");
			}
		}

		public HorseBardingDeed(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)1); // version

			writer.Write((bool)m_Exceptional);
			writer.Write((Mobile)m_Crafter);
			writer.Write((int)m_Resource);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 1:
				case 0:
					{
						m_Exceptional = reader.ReadBool();
						m_Crafter = reader.ReadMobile();

						if (version < 1)
							reader.ReadInt();

						m_Resource = (CraftResource)reader.ReadInt();
						break;
					}
			}
		}
		#region ICraftable Members

		public int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue)
		{
			Exceptional = (quality >= 2);

			if (makersMark)
				Crafter = from;

			Type resourceType = typeRes;

			if (resourceType == null)
				resourceType = craftItem.Resources.GetAt(0).ItemType;

			Resource = CraftResources.GetFromType(resourceType);

			CraftContext context = craftSystem.GetContext(from);

			if (context != null && context.DoNotColor)
				Hue = 0;

			return quality;
		}

		#endregion
	}
}
