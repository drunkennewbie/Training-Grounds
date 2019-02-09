using System;
using Server.Engines.Quests.Objectives;
using Server.Engines.Quests.Rewards;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests.Definitions
{
	#region Quests
	public class Test1 : Quests
	{
		public Test1()
		{
			
			Activated = true;
			HasRestartDelay = true;
			Title = "Test Quest (Title)";
			Description = "This is a Test of the Collecting Questing System, ONLY a Test. (Description)"; 
			RefusalMessage = 1075508; // Oh. Well. I'll just keep trying alone, I suppose...
			InProgressMessage = "In Progress Message";
			CompletionMessage = "Completion Message";

			Objectives.Add(new CollectObjective(20, typeof(Arrow), 1023902)); // arrow

			Rewards.Add(new ItemReward("Apple Pie", typeof(ApplePie))); //
		}

		public override void Generate()
		{
			base.Generate();

			PutSpawner(new Spawner(1, 5, 10, 0, 0, "Doug"), new Point3D(3445, 2634, 28), Map.Trammel);
		}
	}
	public class Test2 : Quests
	{
		public Test2()
		{


			Activated = true;
			HasRestartDelay = true;
			Title = "Test Quest (Title)";
			Description = "This is a Test of the Delivery Questing System, ONLY a Test. (Description)";
			RefusalMessage = 1075508; // Oh. Well. I'll just keep trying alone, I suppose...
			InProgressMessage = "In Progress Message";
			CompletionMessage = "Completion Message";

			Objectives.Add(new DeliverObjective(typeof(Apple), 5, "Apple", typeof(Doug)));

			Rewards.Add(new ItemReward("Apple Pie", typeof(ApplePie))); //
		}

		public override void Generate()
		{
			base.Generate();

			PutSpawner(new Spawner(1, 5, 10, 0, 0, "Funny"), new Point3D(3445, 2634, 28), Map.Trammel);
		}
	}
	public class Test3 : Quests
	{
		public Test3()
		{


			Activated = true;
			HasRestartDelay = true;
			Title = "Test Quest (Title)";
			Description = "This is a Test of the Gain Skill Questing System, ONLY a Test. (Description)";
			RefusalMessage = 1075508; // Oh. Well. I'll just keep trying alone, I suppose...
			InProgressMessage = "In Progress Message";
			CompletionMessage = "Completion Message";

			Objectives.Add(new GainSkillObjective(SkillName.Begging, 500, true, true));

			Rewards.Add(new ItemReward("Apple Pie", typeof(ApplePie))); //
		}

		public override void Generate()
		{
			base.Generate();

			PutSpawner(new Spawner(1, 5, 10, 0, 0, "Patty"), new Point3D(3445, 2634, 28), Map.Trammel);
		}
	}
	public class Test4 : Quests
	{
		public Test4()
		{


			Activated = true;
			HasRestartDelay = true;
			Title = "Test Quest (Title)";
			Description = "This is a Test of the Kill Questing System, ONLY a Test. (Description)";
			RefusalMessage = 1075508; // Oh. Well. I'll just keep trying alone, I suppose...
			InProgressMessage = "In Progress Message";
			CompletionMessage = "Completion Message";

			Objectives.Add(new KillObjective(10, new Type[] { typeof(Rabbit) },"Rabbits"));

			Rewards.Add(new DummyReward("OH MY GOD! WHY DID YOU KILL THE BUNNIES!")); //
		}

		public override void Generate()
		{
			base.Generate();

			PutSpawner(new Spawner(1, 5, 10, 0, 0, "Graham"), new Point3D(3445, 2634, 28), Map.Trammel);
		}
	}
	#endregion

	#region Mobiles

	public class Doug : BaseCreature
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
		public Doug()
			: base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
		{
			Name = "Doug";
			Title = "the Collect Tester";
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

		public Doug(Serial serial)
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

	public class Funny : BaseCreature
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
		public Funny()
			: base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
		{
			Name = "Funny";
			Title = "the Delivery Tester";
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

		public Funny(Serial serial)
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
	public class Patty : BaseCreature
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
		public Patty()
			: base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
		{
			Name = "Patty";
			Title = "the Skill Gain Tester";
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

		public Patty(Serial serial)
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
	public class Graham : BaseCreature
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
		public Graham()
			: base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
		{
			Name = "Graham Chapman";
			Title = "the Kill Tester";
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

		public Graham(Serial serial)
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

