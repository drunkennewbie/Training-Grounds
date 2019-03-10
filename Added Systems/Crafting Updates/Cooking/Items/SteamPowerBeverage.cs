using Server.Engines.Craft;
using Server.Targeting;


namespace Server.Items
{
	public class BeverageMachine : BaseTool
	{
		public override CraftSystem CraftSystem { get { return DefBeverage.CraftSystem; } }
	//	public override bool ReqBPToUse { get { return false; } }
		public override bool BreakOnDepletion { get { return false; } }


		[Constructable]
		public BeverageMachine() : base(0x9A96)
		{
			Name = "Steam Powered Beverage Machine";
			Weight = 20.0;
		}

		public BeverageMachine(Serial serial) : base(serial)
		{
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
	public class SteamStone : Item
	{
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
			if (!this.IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1042001); //That must be in your backpack for you to use it.
				return;
			}
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
