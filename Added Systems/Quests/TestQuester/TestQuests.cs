using System;
using Server.Items;
using Server.Gumps;
using System.Linq;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	#region Quests
	public class TestDeliverQuester : BaseQuest
	{
		//public override Type NextQuest { get { return typeof(Bot2); } }

		public TestDeliverQuester() : base()
		{
			this.AddObjective(new DeliverObjective(typeof(Peach), "Peaches", 5, typeof(QuestionObjectiveNPC), "Randy"));
			this.AddReward(new BaseReward("Delivery Test Completed"));
		}
		public override object Title { get { return "Delivery Test"; } }
		public override object Description
		{
			get
			{
				return "This is a test, Only a Test of the delivery System. Please give the item to Randy";
			}
		}

		public override object Refuse { get { return 1075508; } }
		public override object Uncomplete { get { return "Uncompleted Quest Message"; } }
		public override object Complete { get { return "Completed Qest Message"; } }

		public override void GiveRewards()
		{
			base.GiveRewards();

			this.Owner.SendMessage("Completed with the Delivery Quest!");
		}

	}

	public class QuestioningQuester : BaseQuest
	{
		public QuestioningQuester() : base()
		{
			AddObjective(new QuestionAndAnswerObjective(4, m_EntryTable));
			this.AddReward(new BaseReward("Good Job!, You're not a idiot!"));
		}

		public override bool ShowDescription { get { return false; } }
		public override object Title { get { return "Q+A Test";} }
		public override object Description
		{
			get
			{
				return "Im here to test your knowledge of the world, answer a question for me please";
			}
		}
		public override object Refuse {  get { return 1075508;  } }
		public override object Uncomplete { get { return "Once you are ready speak to me again."; }}
		public override object Complete { get { return "Great Job!"; } }
		public override object FailedMsg { get { return "That isnt right"; } }

		public override void GiveRewards()
		{
			base.GiveRewards();

			this.Owner.SendMessage("Question Quest Completed!");
		}

		public override bool RenderObjective(NewQuestGump g, bool offer)
		{
			if (offer)
				g.AddHtmlLocalized(130, 45, 270, 16, 1049010, 0xFFFFFF, false, false); // Quest Offer
			else
				g.AddHtmlLocalized(130, 45, 270, 16, 1046026, 0xFFFFFF, false, false); // Quest Log

			g.AddHtmlObject(160, 70, 200, 40, Title, BaseQuestGump.DarkGreen, false, false);
			g.AddHtmlLocalized(98, 140, 312, 16, 1049073, 0x2710, false, false); // Objective:

			g.AddHtmlLocalized(98, 156, 312, 16, 1072208, 0x2710, false, false); // All of the following	

			int offset = 172;
			string str;

			foreach (QuestionAndAnswerObjective obj in Objectives.OfType<QuestionAndAnswerObjective>())
			{
				if (offer)
					str = String.Format("Answer {0} questions correctly.", obj.MaxProgress);
				else
					str = String.Format("Answer {0}/{1} questions answered correctly.", obj.CurProgress, obj.MaxProgress);

				g.AddHtmlObject(98, offset, 312, 16, str, BaseQuestGump.LightGreen, false, false);

				offset += 16;
			}

			return true;
		}

		public override void OnAccept()
		{
			base.OnAccept();
			Owner.SendGump(new QAndAGump(Owner, this));
		}

		public static void Configure()
		{
			m_EntryTable[0] = new QuestionAndAnswerEntry("What is yellow and yummy?", new object[] { "Banana" }, new object[] { "Apple", "Orange", "Peach" });  //<center>Finish this truism: Humility shows us...</center>
			m_EntryTable[1] = new QuestionAndAnswerEntry("What is the new fruit of UO?", new object[] { "Dragonfruit" }, new object[] { "Squirrels", "Banana", "Apples" });  //<center>Finish this truism: Humility shows us...</center>
			m_EntryTable[2] = new QuestionAndAnswerEntry("What year was UO Made?", new object[] { "1997" }, new object[] { "1998","1999","2000","1989" });  //<center>Finish this truism: Humility shows us...</center>
			m_EntryTable[3] = new QuestionAndAnswerEntry("The answer of this question is?", new object[] { "This" }, new object[] { "Not this" });  //<center>Finish this truism: Humility shows us...</center>
		}

		private static QuestionAndAnswerEntry[] m_EntryTable = new QuestionAndAnswerEntry[4];
		public static QuestionAndAnswerEntry[] EntryTable { get { return m_EntryTable; } }

	}

	public class ObtainObjectiveQuester : BaseQuest
	{
		//public override Type NextQuest { get { return typeof(Bot2); } }

		public ObtainObjectiveQuester() : base()
		{ //Obtain Objective (Obtain Item, Name of Item, How Many, Image (If able), Time?, Hue)
			this.AddObjective(new ObtainObjective(typeof(Apple), "Apples", 5));
			this.AddReward(new BaseReward("Obtain an Item Objective Completed"));
		}
		public override object Title { get { return "Obtain Completed"; } }
		public override object Description
		{
			get
			{
				return "Fetch me Apples Boy! For this Test!.";
			}
		}

		public override object Refuse { get { return 1075508; } }
		public override object Uncomplete { get { return "Uncompleted: Find me my apples yet Boy!?"; } }
		public override object Complete { get { return "Completed: Thanks! Chump"; } }

		public override void GiveRewards()
		{
			base.GiveRewards();

			this.Owner.SendMessage("Obtain Quest Completed!");
		}

	}

	public class SlayObjectiveQuester : BaseQuest
	{
		//public override Type NextQuest { get { return typeof(Bot2); } }

		public SlayObjectiveQuester() : base()
		{ //Slay Objective (Creature, Name, How Many, Where at?, Time?)
			this.AddObjective(new SlayObjective(typeof(Rat), "Rats", 5));
			this.AddReward(new BaseReward("Slay a Creature Completed"));
		}
		public override object Title { get { return "Slay Completed"; } }
		public override object Description
		{
			get
			{
				return "Slay A few of those killer rabbits!";
			}
		}

		public override object Refuse { get { return 1075508; } }
		public override object Uncomplete { get { return "Uncompleted: Can't even Kill a Damn Rabbit?"; } }
		public override object Complete { get { return "Completed: Thanks! Chump"; } }

		public override void GiveRewards()
		{
			base.GiveRewards();

			this.Owner.SendMessage("Slay Quest Completed!");
		}

	}


	#endregion
	#region Mobiles

	public class DeliveryObjectNPC : NewQuester
	{
		public override bool IsInvulnerable { get { return true; } }
		public override bool CanTeach { get { return false; } }

		public override Type[] Quests
		{
			get
			{
				return new Type[]
				{
					typeof(TestDeliverQuester)
				};
			}
		}

		[Constructable]
		public DeliveryObjectNPC() : base("David", " The Delivery Tester")
		{

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

		public DeliveryObjectNPC(Serial serial)
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
	public class QuestionObjectiveNPC : NewQuester
	{
		public override bool IsInvulnerable { get { return true; } }
		public override bool CanTeach { get { return false; } }


		public override Type[] Quests
		{
			get
			{
				return new Type[]
				{
					typeof(QuestioningQuester)
				};
			}
		}

		[Constructable]
		public QuestionObjectiveNPC() : base("Randy", " The Delivery Tester")
		{

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

		public QuestionObjectiveNPC(Serial serial)
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
	public class ObtainQuesterNPC : NewQuester
	{
		public override bool IsInvulnerable { get { return true; } }
		public override bool CanTeach { get { return false; } }

		public override Type[] Quests
		{
			get
			{
				return new Type[]
				{
					typeof(ObtainObjectiveQuester)
				};
			}
		}

		[Constructable]
		public ObtainQuesterNPC() : base("Oscar", " The Obtain Tester")
		{

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

		public ObtainQuesterNPC(Serial serial)
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
	public class SlayObjectNPC : NewQuester
	{
		public override bool IsInvulnerable { get { return true; } }
		public override bool CanTeach { get { return false; } }
		public override bool DisallowAllMoves { get { return true; } }

		public override Type[] Quests
		{
			get
			{
				return new Type[]
				{
					typeof(SlayObjectiveQuester)
				};
			}
		}

		[Constructable]
		public SlayObjectNPC() : base("Sean", " The Slayer Tester")
		{

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

		public SlayObjectNPC(Serial serial)
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
