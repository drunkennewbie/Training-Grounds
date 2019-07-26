using Server.Engines.Craft;
using Server.Targeting;
using Server.Gumps;
using Server.Multis;
using Server.Network;
using Server.ContextMenus;
using System.Collections.Generic;


namespace Server.Items
{
	public class BeverageMachine : BaseTool, ISecurable
	{
		public override CraftSystem CraftSystem { get { return DefBeverage.CraftSystem; } }
		public override bool BreakOnDepletion { get { return false; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public SecureLevel Level { get; set; }

		[Constructable]
		public BeverageMachine() : base(0x9A96)
		{
			Name = "Steam Powered Beverage Machine";
			Weight = 20.0;
		}

		public BeverageMachine(Serial serial) : base(serial)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (!from.InRange(this, 4))
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3E9, 1019045); // I can't reach that.
				return;
			}

			if (CheckAccessible(from, this))
			{
				CraftSystem system = this.CraftSystem;
				int num = system.CanCraft(from, this, null);
				CraftContext context = system.GetContext(from);
				from.SendGump(new CraftGump(from, system, this, null));
			}
		}
		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);

			SetSecureLevelEntry.AddTo(from, this, list);
		}
		public bool CheckAccessible(Mobile from, Item item)
		{
			if (from.AccessLevel >= AccessLevel.GameMaster)
				return true; // Staff can access anything

			BaseHouse house = BaseHouse.FindHouseAt(item);

			if (house == null)
				return false;

			switch (Level)
			{
				case SecureLevel.Owner: return house.IsOwner(from);
				case SecureLevel.CoOwners: return house.IsCoOwner(from);
				case SecureLevel.Friends: return house.IsFriend(from);
				case SecureLevel.Anyone: return true;
				case SecureLevel.Guild: return house.IsGuildMember(from);
			}

			return false;
		}


		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)Level);
			writer.Write((int)0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			Level = (SecureLevel)reader.ReadInt();
			int version = reader.ReadInt();
		}
	}
	public class SteamStone : Item
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public SecureLevel Level { get; set; }

		[Constructable]
		public SteamStone() : base(0x44C1)
		{
			Name = "Steam Stone";
			Weight = 1.0;
		}

		public SteamStone(Serial serial) : base(serial)
		{

		}
		public override void OnDoubleClick(Mobile from)
		{
			if (!this.IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1042001);
			}
			else
			{
				from.BeginTarget(2, true, TargetFlags.None, new TargetCallback(OnTarget));
				from.SendMessage("What do you want to use the Stone on?");
			}
		}

		public void OnTarget(Mobile from, object obj)
		{
			if (CheckAccessible(from, this))
			{
				if (obj is BaseTool)
				{
					BaseTool tools = (BaseTool)obj;
					if (tools is BeverageMachine)
					{
						if (tools.UsesRemaining >= 76)
						{
							from.SendMessage("There isn't enough room in the machine for this.");
						}
						else
						{
							tools.UsesRemaining += 25;
							from.SendMessage("You recharge the machine with Steam.");
							this.Delete();
						}
					}
				}
				else
					from.SendMessage("You can't use this item on that.");
			}
		}

		public bool CheckAccessible(Mobile from, Item item)
		{
			if (from.AccessLevel >= AccessLevel.GameMaster)
				return true; // Staff can access anything

			BaseHouse house = BaseHouse.FindHouseAt(item);

			if (house == null)
				return false;

			switch (Level)
			{
				case SecureLevel.Owner: return house.IsOwner(from);
				case SecureLevel.CoOwners: return house.IsCoOwner(from);
				case SecureLevel.Friends: return house.IsFriend(from);
				case SecureLevel.Anyone: return true;
				case SecureLevel.Guild: return house.IsGuildMember(from);
			}

			return false;
		}


		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}

	}
}
