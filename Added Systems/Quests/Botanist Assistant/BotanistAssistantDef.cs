using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	#region Quests
	public class Bot1 : BaseQuest
	{
		public override QuestChain ChainID { get { return QuestChain.Sprinklers; } }
		public override Type NextQuest { get { return typeof(Bot2); } }		
		public override bool DoneOnce { get { return true; } }

		public Bot1() : base()
		{
			this.AddObjective(new DeliverObjective(typeof(GionvanniRequest), "Giovanni Help Request", 1, typeof(Giovanni), "Giovanni"));
			this.AddReward(new BaseReward("Another Step closer to creation of Sprinkler System"));
		}
		public override object Title { get { return "Requesting assistance"; }}
		public override object Description {
			get
			{
				return "Penelope hired me to help her tend to her garden. The problem is there are to many flowers for me to handle alone. " +
						  "<BR>I remember in my travels someone mentioning that there was a system designed that could help water flowers and crops." +
						  "<BR>I think it was called a Sprinkler System. " +
						  "<BR>There was a tinker I met in Britain but I am sure they moved now though, but maybe they can help with this thing." +
						  "<BR>I remember their name it was; Giovanni.";
			}
		}

		public override object Refuse { get { return 1075508; }}
		public override object Uncomplete {  get { return "How goes your search for Giovanni?"; }}
		public override object Complete { get { return "What is this we have here?";  } }

		public override void GiveRewards()
		{
			base.GiveRewards();

			this.Owner.SendMessage("Another Step Closer to a sprinkler System... We hope");
		}

	
	}

	
	public class Bot2 : BaseQuest
	{
		public Bot2()
		{
			this.AddObjective(new DeliverObjective(typeof(GionvanniRequest), "Giovanni Help Request", 1, typeof(Giovanni), "Giovanni"));
			this.AddReward(new BaseReward("Another Step closer to creation of Sprinkler System"));
		}

		public override object Title { get { return "Meeting Giovanni"; } }
		public override object Description
		{
			get
			{
				return "Ah, the Sprinkler system? I have heard of one of those too. I saw one many a year ago. I think it was in Skara Brae" +
					   "<BR>I would love to have the schmatics, the problem is the creator died with the only schematic in existant." +
					   "<BR>If you could draw and write down all the information on the sprinkler with the schematic kit maybe I can recreate it.";
			}
		}

		public override object Refuse { get { return 1075508; } }
		public override object Uncomplete { get { return "Any luck with drawing the schematics?"; } }
		public override object Complete { get { return "What is this we have here?"; } }

		public override void GiveRewards()
		{
			base.GiveRewards();

			this.Owner.SendMessage("Another Step Closer to a sprinkler System... We hope");
		}

	}
	
	#endregion

	#region Mobiles

	public class Harvey : NewQuester
	{
		public override bool IsInvulnerable { get { return true; } }
		public override bool CanTeach { get { return false; } }

		public override Type[] Quests
		{
			get
			{
				return new Type[]
				{
					typeof(Bot1)
				};
			}
		}
		
		[Constructable]
		public Harvey(): base("Harvey"," The Botanist Assistant")
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

	public class Giovanni : NewQuester
	{
		public override bool IsInvulnerable { get { return true; } }
		public override bool CanTeach { get { return false; } }

		public override Type[] Quests
		{
			get
			{
				return new Type[]
				{
					typeof(Bot2)
				};
			}
		}

		[Constructable]
		public Giovanni() : base("Giovanni"," The Master Tinker")
		{
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

