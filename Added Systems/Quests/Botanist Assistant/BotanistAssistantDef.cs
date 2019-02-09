using System;
using System.Collections.Generic;
using Server;
using Server.ContextMenus;
using Server.Engines.Quests.Objectives;
using Server.Engines.Quests.Rewards;
using Server.Items;
using Server.Misc;
using Server.Mobiles;

namespace Server.Engines.Quests.Definitions
{
	#region Quests
	public class Bot1 : Quests
	{
		public override Type NextQuest { get { return typeof(Bot2); } }
		public override bool IsChainTriggered {  get { return true;  } }

		public Bot1()
		{
			

			Activated = true;
			OneTimeOnly = true;
			Title = "Requesting Assistance";
			Description = "Penelope hired me to help her tend to her garden. The problem is there are to many flowers for me to handle alone. " +
						  "<BR>I remember in my travels someone mentioning that there was a system designed that could help water flowers and crops." +
						  "<BR>I think it was called a Sprinkler System. " +
						  "<BR>There was a tinker I met in Britain but I am sure they moved now though, but maybe they can help with this thing." +
						  "<BR>I remember their name it was; Giovanni."; 
			RefusalMessage = 1075508; // Oh. Well. I'll just keep trying alone, I suppose...
			InProgressMessage = "How goes your search for Giovanni?";
			CompletionMessage = "What is this we have here?";

			Objectives.Add(new DeliverObjective(typeof(GionvanniRequest), 1, "Gionvanni Help Request", typeof(Giovanni)));

			Rewards.Add(new DummyReward("Onwards to create a Sprinkler!"));
		}

		public override void Generate()
		{
			base.Generate();

			PutSpawner(new Spawner(1, 5, 10, 0, 0, "Harvey"), new Point3D(3445, 2634, 28), Map.Trammel);
		}
	}
	public class Bot2 : Quests
	{
		public Bot2()
		{
			
		Activated = true;
			OneTimeOnly = true;
			Title = "Meeting Giovanni";
			Description = "Ah, the Sprinkler system? I have heard of one of those too. I saw one many a year ago. I think it was in Skara Brae" +
						  "<BR>I would love to have the schmatics, the problem is the creator died with the only schematic in existant." +
						  "<BR>If you could draw and write down all the information on the sprinkler with the schematic kit maybe I can recreate it.";
			RefusalMessage = "Alright, if you change your mind, you know where to find me.";
			InProgressMessage = "Any luck with drawing the schematics?";
			CompletionMessage = "Alright lets see, what do we have here";

			Objectives.Add(new CollectObjective(20, typeof(Arrow), 1023902)); // arrow

			Rewards.Add(new DummyReward("Onwards to create a Sprinkler!"));
		}

		public override void Generate()
		{
			base.Generate();

			PutSpawner(new Spawner(1, 5, 10, 0, 0, "Giovanni"), new Point3D(3445, 2635, 28), Map.Trammel);
		}
	}
	#endregion

	#region Mobiles

	public class Harvey : BaseCreature
	{
		public override bool IsInvulnerable { get { return true; } }
		public override bool CanTeach { get { return false; } }

		public override bool CanShout { get { return true; } }
		public override void Shout(PlayerMobile pm)
		{
			QuestSystem.Tell(this, pm, Utility.RandomList(
				1074205, // Oh great adventurer, would you please assist a weak soul in need of aid?
				1074213 // Hey buddy.  Looking for work?
			));
		}

		[Constructable]
		public Harvey()
			: base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
		{
			Name = "Harvey";
			Title = "the Botanist Assistant";
			Race = Race.Human;
			BodyValue = 0x190;
			Female = false;
			Hue = Race.RandomSkinHue();
			InitStats(100, 100, 25);

			Utility.AssignRandomHair(this, true);

			SetSkill(SkillName.Archery, 60.0, 80.0);

			AddItem(new Backpack());

			Item item;

			AddItem(new Doublet(0x598));
			AddItem(new LongPants(0x59B));
			AddItem(new Boots());
		}

		public Harvey(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}

	public class Giovanni : BaseCreature
	{
		public override bool IsInvulnerable { get { return true; } }
		public override bool CanTeach { get { return false; } }
						
		[Constructable]
		public Giovanni()
			: base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
		{
			Name = "Giovanni";
			Title = "the Master Tinker";
			Race = Race.Human;
			BodyValue = 0x190;
			Female = false;
			Hue = Race.RandomSkinHue();
			InitStats(100, 100, 25);

			Utility.AssignRandomHair(this, true);

			SetSkill(SkillName.Tinkering, 60.0, 80.0);

			AddItem(new Backpack());
				
			AddItem(new Backpack());
			AddItem(new Sandals());
			AddItem(new Doublet());
			AddItem(new ShortPants());
			AddItem(new HalfApron(0x8AB));

	}

		public Giovanni(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}

	#endregion
}

