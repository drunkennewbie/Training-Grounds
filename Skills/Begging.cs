using System;
using Server;
using Server.Misc;
using Server.Targeting;
using Server.Items;
using Server.Network;
using Server.Mobiles;
using System.Linq;

//Upgraded for multiple places:
//Can now Beg from Vendors, SERVER WIDE, Meaning if Player 1 Begs from Vendor, Player 2 has to wait for that vendor on cool down. BUT other vendors are avaiable. Timer is random 60-90~
//Can now beg from Orcs + Savages! 30% chance to be attacked or mask/paint blows up. Close to .05% chance to get Rare Item
//Begging Coins are for Quest


namespace Server.SkillHandlers
{
	public class Begging
	{
		public static Type[] OrcTypes = new Type[] //make Orc List add any you may have.
		{
			typeof(Orc),
			typeof(OrcBomber),
			typeof(OrcBrute),
			typeof(OrcCaptain),
			typeof(OrcishLord),
			typeof(OrcishMage)
			//typeof(OrcishMineOverseer), //These 4 are Commented out because they arent on my server.
			//typeof(OrcMiner),
			//typeof(OrcLeader),
			//typeof(OrcMineBomber)
		};

		public static bool IsOrc(Mobile m) //"IsOrc" for search options.
		{
			Type t = m.GetType();

			return OrcTypes.Any(t1 => t1.IsAssignableFrom(t));
		}

		public static Type[] SavageTypes = new Type[] //Create Savage List add any you may have
		{
			typeof( Savage ),
			typeof( SavageRider ),
			typeof( SavageRidgeback ),
			typeof( SavageShaman )

		};

		public static bool IsSavage(Mobile m) //"IsSavage" for seach options
		{
			Type t = m.GetType();

			return SavageTypes.Any(t1 => t1.IsAssignableFrom(t));
		}

		public static void Initialize()
		{
			SkillInfo.Table[(int)SkillName.Begging].Callback = OnUse;
		}

		public static TimeSpan OnUse(Mobile m) //Commence the Skill
		{
			m.RevealingAction();

			m.SendLocalizedMessage(500397); // To whom do you wish to grovel?

			Timer.DelayCall(() => m.Target = new InternalTarget());

			return TimeSpan.FromHours(1.0);
		}

		private class InternalTarget : Target
		{
			private bool m_SetSkillTime = true;

			public InternalTarget()
				: base(12, false, TargetFlags.None)
			{ }

			protected override void OnTargetFinish(Mobile from)
			{
				if (m_SetSkillTime)
				{
					from.NextSkillTime = Core.TickCount;
				}
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				from.RevealingAction(); //Reveals Beggers

				int number = -1;

				if (targeted is Mobile) //Determine the Creature
				{

					Mobile targ = (Mobile)targeted; //Rename to Targ

					bool orcs = IsOrc(targ); //Sets True/False for Orc

					if (targ.Player) // We can't beg from players
					{

						number = 500398; // Perhaps just asking would work better.
					}
					else if (!targ.Body.IsHuman && !orcs) // Make sure the NPC is human | or Orc
					{
						number = 500399; // There is little chance of getting money from that!
					}
					else if (!from.InRange(targ, 2)) //Gender Differences
					{
						if (!targ.Female)
						{
							number = 500401; // You are too far away to beg from him.
						}
						else
						{
							number = 500402; // You are too far away to beg from her.
						}
					}
					else if (from.Mounted) // If we're on a mount, who would give us money? Possible used on server?
					{
						number = 500404; // They seem unwilling to give you any money.
					}

					else
					{
						if (targ is BaseVendor) //Vendors Themselves are on a 60-90 minute cool down to help prevent macroing 
						{
							DateTime now = DateTime.UtcNow;
							BaseVendor targvend = targ as BaseVendor; //Is Target Vendor?
						//	Console.WriteLine(targvend.GeneralNextBegging);
							if (targvend.NextBegging > now) //Vendor be used
							{
								from.SendLocalizedMessage(500404); // They seem unwilling to give you any money.
								return;
							}
							else
							{
						//		Console.WriteLine(targvend.GeneralNextBegging);
								targvend.NextBegging = now + TimeSpan.FromMinutes(Utility.RandomMinMax(60, 90)); //Set Vendor Timer 60-90 minutes (Change for Balance)
							}
						}
						else
						{
							DateTime now = DateTime.UtcNow;
							BaseCreature targcreat = targ as BaseCreature;
						//	Console.WriteLine(targcreat.GeneralNextBegging);
							if (targcreat.GeneralNextBegging > now) //Vendor be used
							{
								from.SendLocalizedMessage(500404); // They seem unwilling to give you any money.
								return;
							}
							else
							{
						//		Console.WriteLine(targcreat.GeneralNextBegging);
								targcreat.GeneralNextBegging = now + TimeSpan.FromMinutes(Utility.RandomMinMax(5, 10)); //Set Vendor Timer 60-90 minutes (Change for Balance)
							}
						}
							// Face eachother
						from.Direction = from.GetDirectionTo(targ);
						targ.Direction = targ.GetDirectionTo(from);

						from.Animate(32, 5, 1, true, false, 0); // Bow
						switch (Utility.Random(2))
						{
							case 0: from.PublicOverheadMessage(MessageType.Regular, 0x3B2, false, "Please could you spare some change"); break;
							case 1: from.PublicOverheadMessage(MessageType.Regular, 0x3B2, false, "Anything you give will help. Please"); break;
							case 2: from.PublicOverheadMessage(MessageType.Regular, 0x3B2, false, "Do you really need that item?"); break;
						}
						new InternalTimer(from, targ).Start(); //Commence Timer

						m_SetSkillTime = false;

					}
				}
				else // Not a Mobile
				{
					number = 500399; // There is little chance of getting money from that!
				}

				if (number != -1)
				{
					from.SendLocalizedMessage(number);
				}
			}

			private class InternalTimer : Timer
			{
				private readonly Mobile m_From;
				private readonly Mobile m_Target;

				public InternalTimer(Mobile from, Mobile target)
					: base(TimeSpan.FromSeconds(2.0))
				{
					m_From = from;
					m_Target = target;
					Priority = TimerPriority.TwoFiftyMS;
				}

				protected override void OnTick()
				{

					bool orcs = IsOrc(m_Target);
					bool savage = IsSavage(m_Target);

					var badKarmaChance = 0.5 - ((double)m_From.Karma / 8570); //Lower your Karma Less chance you get
					var goodKarmaChance = 0.5 + ((double)m_From.Karma / 8570); //Higher Karma less chance for Monsters

					m_From.NextSkillTime = Core.TickCount + 10000; //Set next skill use 10 seconds
					if (!orcs && !savage && m_From.Karma < 0 && badKarmaChance > Utility.RandomDouble())
					{
						if (!orcs && m_From.CheckTargetSkill(SkillName.Begging, m_Target, 0, 100))
							m_Target.PublicOverheadMessage(MessageType.Regular, m_Target.SpeechHue, 500406);
						else
							m_Target.PublicOverheadMessage(MessageType.Regular, m_Target.SpeechHue, 500406);
						// Thou dost not look trustworthy... no gold for thee today!
					}
					else if (orcs)
					{
						if (m_From.Karma > 0 && goodKarmaChance > Utility.RandomDouble())
							m_From.SendMessage("You seem to notable to beg");
						else
						{
							//Console.WriteLine("Orc Begged");
							if (m_From.CheckTargetSkill(SkillName.Begging, m_Target, 80, 100)) //Need 80+ Skill to Attempt to Beg Orc
							{
								int begchance = Utility.Random(100);//Lets see if you have bad accident
								if (begchance <= 10) //10% chance to blow up mask 
								{
									Item item = m_From.FindItemOnLayer(Layer.Helm);

									if (item is OrcishKinMask)
									{
										m_From.SendMessage("{0} alerts the other orcs that you are a fake", m_Target); //Orcs dont beg
										AOS.Damage(m_From, 50, 0, 100, 0, 0, 0);
										item.Delete();
										m_From.FixedParticles(0x36BD, 20, 10, 5044, EffectLayer.Head);
										m_From.PlaySound(0x307);
									}
								}
								else if (begchance <= 30) //30% to just attack player
								{

									m_Target.Attack(m_From);
									m_From.SendMessage("{0} seems upset", m_Target); //Well they dont like beggers
								}
								else
									BegChance(m_From, m_Target, true, false); //What is your chance to beg?
							}
							else
							{
								m_From.SendMessage("They don't seem to have noticed you");
							}
						}
					}
					else if (savage)
					{
						//Console.WriteLine("Savage Beg");
						if (m_From.CheckTargetSkill(SkillName.Begging, m_Target, 80, 100)) //Need 80+ Skill to Attempt to beg from Savages
						{
							int begchance = Utility.Random(100);
							if (begchance <= 10) //10% chance to blow up paint, Pride Tribesman/Women!
							{
								if (m_From.BodyMod == 183 || m_From.BodyMod == 184)
								{
									AOS.Damage(m_From, 50, 0, 100, 0, 0, 0);
									m_From.BodyMod = 0;
									m_From.HueMod = -1;
									m_From.FixedParticles(0x36BD, 20, 10, 5044, EffectLayer.Head);
									m_From.PlaySound(0x307);
									m_From.SendLocalizedMessage(1040008); // Your skin is scorched as the tribal paint burns away!
									((PlayerMobile)m_From).SavagePaintExpiration = TimeSpan.Zero;
								}
							}
							else if (begchance <= 30) //30% to just attack player
							{
								m_Target.Attack(m_From);
								m_From.SendMessage("{0} seems upset", m_Target);
							}
							else
								BegChance(m_From, m_Target, false, true); //Whats your Beg Chance?
						}
					}
					else if (m_From.CheckTargetSkill(SkillName.Begging, m_Target, 0, 100))
					{
						//Console.WriteLine("Human Beg");
						BegChance(m_From, m_Target, false, false); //Not a Savage or Orc? Human than!
					}
					else
					{
						//Console.WriteLine("Fail Beg");
						m_Target.SendLocalizedMessage(500404); // They seem unwilling to give you any money.
					}
				}
			}

			public static void BegChance(Mobile m, object targeted, bool orcs, bool savage) //What is loot roll!
			{
				Mobile t = (Mobile)targeted;
				double chance = 1.0; //Test
				//double chance = Utility.RandomDouble(); //Roll .00 -> 1.0

				#region Begging Outfit
				double bonus = 0;
				double pbonus = 0;
				Console.WriteLine("<0>Chance is {0}", chance);

				for (int i = 0; i < m.Items.Count; i++)
				{
					Item item = m.Items[i];

					if (item is IBeggingAttire)
					{
						if (item is ISetItem)
						{
							if (((ISetItem)item).SetEquipped)
							{
								pbonus = ((double)((IBeggingAttire)item).SetBonus / 100) / (double)((ISetItem)item).Pieces;
								//Console.WriteLine("<{0}>Piece Bonus is {1}", i, pbonus);
								bonus = bonus + pbonus;
								//Console.WriteLine("<{0}>Bonus is {1}", i, bonus);
							}
						}
					}
					if (item is BeggerStaff)
					{
						//Staff is 5%
						bonus = bonus + .05;
					//	Console.WriteLine("<{0}>Staff Bonus is {1}", i, bonus);
					}

				}
				
				chance = chance + bonus;
				#endregion

				Console.WriteLine("<1>Chance is {0}", chance);

				if (m is PlayerMobile)
				{
					PlayerMobile pm = (PlayerMobile)m;
					if (chance >= .95 && t is BaseVendor ) //Greater than .95 and its a Vendor
					{
						VendorBeg(m, t, chance);
						//m.Say("Vendor File {0}", chance);
					}
					else if (chance >= .95 && orcs == true && pm.NpcGuild == NpcGuild.BeggerGuild) //Greater than .95 and a Orc
					{
						OrcBeg(m, t, chance);
						//m.Say("Orc File {0}", chance);
					}
					else if (chance >= .95 && savage == true && pm.NpcGuild == NpcGuild.BeggerGuild) //Greater than .95 and Savage
					{
						SavageBeg(m, t, chance);
						//m.Say("Savage File {0}", chance);
					}
					else //Less than .75 or not a vendor-orc-savage? Have some crap
					{
						JunkBeg(m, t, chance);
						//m.Say("Junk File {0}", chance);
					}
				}
			}

			public static void VendorBeg(Mobile m, object targeted, double chance)
			{
				//Console.WriteLine("Chance: {0}", chance);
				Mobile t = (Mobile)targeted;
				Item reward = null;
				string rewardName = "";


				if (chance > .99 && m.Skills.Begging.Base >= 100)//Vendor Only (1 hour cooldown)
				{
					int rand = Utility.Random(9); //0-9 (10 Total) (40% chance for a item)
					if (rand == 0)
					{
						reward = new RockArtifact(); //Rock Artifact
						rewardName = "A rock";
					}
					else if (rand == 2)
					{
						reward = new BeggerCoins(24); //Special Coins
						rewardName = "24 dull slver coins.";
					}
					else if (rand == 4) //Fur Boots
					{
						reward = new FurBoots();
						rewardName = "a pair of Fur Boots";
					}
					else if (rand == 6)
					{
						reward = new FlowerGarland();
						rewardName = "a flower Garland";
					}
				}
				Console.WriteLine("RewardName (1) {0}", reward);
				if (chance > .95 && m.Skills.Begging.Base >= 90 && reward == null)
				{
					int rand = Utility.Random(9);
					Console.WriteLine("Random 2 is {0}", rand);
					if (rand == 0)
					{
						reward = new BegBedRoll();
						rewardName = "Bedroll";
					}
					else if (rand == 1)
					{
						reward = new Cookies();
						rewardName = "Cookies";
					}
					else if (rand == 3)
					{
						reward = new FishSteak();
						rewardName = "Fish Steaks";
					}
					else if (rand == 4)
					{
						reward = new FishingPole();
						rewardName = "Fishing Pole";
					}
					else if (rand == 6)
					{
						reward = new BeggerCoins(12);
						rewardName = "12 Dull Silver Coins.";
					}
					else if (rand == 7)
					{
						reward = new Turnip();
						rewardName = "Turnip";
					}
					else if (rand == 9)
					{
						reward = new CeramicMug();
						rewardName = "Ceramic Mug";
					}

				}
				Console.WriteLine("RewardName (2) {0}", reward);
				if (reward == null)
				{
					JunkBeg(m, t, chance);
				}
				else
					Reward(m, t, reward, rewardName);
			}

			public static void SavageBeg(Mobile m, object targeted, double chance)
			{
				Mobile t = (Mobile)targeted;
				Item reward = null;
				string rewardName = "";


				if (chance > .99 && m.Skills.Begging.Base >= 100)
				{
					int rand = Utility.Random(9);
					if (rand == 0)
					{
						reward = new TribalBedroll();
						rewardName = "a Tribal Bedroll";
					}
					else if (rand == 2)
					{
						reward = new BeggerCoins(50); //Special Coins!
						rewardName = "50 dull silver coins.";
					}
					else if (rand == 4)
					{
						reward = new FurCape(); //fur Cape!
						rewardName = "a Fur Cape";

					}
					else if (rand == 6)
					{
						reward = new HornedTribalMask();
						rewardName = "a horned tribal mask";

					}
				}
				if (chance > .95 && m.Skills.Begging.Base >= 90 && reward == null)
				{
					int rand = Utility.Random(9);

					if (rand == 0)
					{
						reward = new LambLeg();
						rewardName = "a leg of lamb";
					}
					else if (rand == 1)
					{
						reward = new OrcishKinMask();
						rewardName = "an orc mask";
					}
					else if (rand == 3)
					{
						reward = new TribalBerry(2);
						rewardName = "tribal berries";
					}
					else if (rand == 4)
					{
						reward = new TribalMask();
						rewardName = "a tribal mask";
					}
					else if (rand == 6)
					{
						reward = new BeggerCoins(25);
						rewardName = "25 dull silver coins.";
					}
					else if (rand == 7)
					{
						reward = new Beads();
						rewardName = "beads";
					}

				}
				if (reward == null)
				{
					JunkBeg(m, t, chance);
				}
				else
					Reward(m, t, reward, rewardName);
			}
			
			public static void OrcBeg(Mobile m, object targeted, double chance)
			{
				Mobile t = (Mobile)targeted;
				Item reward = null;
				string rewardName = "";


				if (chance > .99 && m.Skills.Begging.Base >= 100)
				{
					int rand = Utility.Random(2);
					if (rand == 0)
					{
						reward = new GruesomeStandardArtifact(); //Gruesome Standard Artifact!
					}
					else if (rand == 1)
					{
						reward = new BeggerCoins(50);
						rewardName = "50 Dull Silver Coins.";
					}
					else if (rand == 2)
					{
						reward = new FurCape();

					}
				}
				if (chance >= .95)
				{
					int rand = Utility.Random(7);

					if (rand == 0)
					{
						reward = new LambLeg();
					}
					else if (rand == 1)
					{
						reward = new Head();

					}
					else if (rand == 2)
					{
						reward = new FishSteak();

					}
					else if (rand == 3)
					{
						reward = new Pickaxe();

					}
					else if (rand == 5)
					{
						reward = new IronIngot(1);
					}

					else if (rand == 5)
					{
						reward = new BeggerCoins(25);
						rewardName = "25 Dull Silver Coins.";
					}


				}
				Reward(m, t, reward, rewardName);
			}
			public static void JunkBeg(Mobile m, object targeted, double chance) //Nothing Good. Here have some crap
			{

				Mobile t = (Mobile)targeted;
				bool orcs = IsOrc(t);
				Container theirPack = t.Backpack;
				Item reward = null;
				string rewardName = "";

				if (chance >= .76 && m.Skills.Begging.Base >= 75)
				{
					int rand = Utility.Random(10);

					if (rand == 0)
					{
						reward = new WoodenBowlOfPeas();
						rewardName = "a bowl of peas";
					}
					else if (rand == 1)
					{
						reward = new CheeseWedge();
						rewardName = "a cheese wedge";
					}
					else if (rand == 2)
					{
						reward = new Dates();
						rewardName = "some dates";

					}
					else if (rand == 3)
					{
						reward = new BeggerCoins(6);
						rewardName = "6 dull silver coins.";
					}
					else if (rand == 4)
					{
						reward = new BeverageBottle(BeverageType.Ale);
						rewardName = "a bottle of ale";
					}
					else if (rand == 5)
					{
						reward = new CheesePizza();
						rewardName = "a cheese pizza";
					}
					else if (rand == 6)
					{
						reward = new Shirt();
						rewardName = "a shirt";
					}
				}
				Console.WriteLine("RewardName (3) {0}", reward);
				if (chance >= .25 && reward == null)
				{
						reward = new FrenchBread();
						rewardName = "french bread";
				}

				if (reward == null && orcs == false) //Gold from Non Orcs and if you got nothing else from above.
				{
					int toConsume = theirPack.GetAmount(typeof(Gold)) / 10;
					int max = 10 + (m.Fame / 2500);

					if (max > 14)
					{
						max = 14;
					}
					else if (max < 10)
					{
						max = 10;
					}

					if (toConsume > max)
					{
						toConsume = max;
					}

					if (toConsume > 0)
					{
						int consumed = theirPack.ConsumeUpTo(typeof(Gold), toConsume);

						if (consumed > 0)
						{
							t.PublicOverheadMessage(MessageType.Regular, t.SpeechHue, 500405);
							// I feel sorry for thee...

							Gold gold = new Gold(consumed);

							reward = new Gold(consumed);
							rewardName = "Gold";
							m.PlaySound(gold.GetDropSound());
							if (orcs == false)
							{
								if (m.Karma > -3000)
								{
									int toLose = m.Karma + 3000;

									if (toLose > 40)
									{
										toLose = 40;
									}

									Titles.AwardKarma(m, -toLose, true);
								}
							}
						}
						else
						{
							if (orcs == false) //Orcs Dont speak English
								t.PublicOverheadMessage(MessageType.Regular, t.SpeechHue, 500407);
							// I have not enough money to give thee any!
						}
					}
					else
					{
						if (orcs == false) //Orcs Dont Speak English
							t.PublicOverheadMessage(MessageType.Regular, t.SpeechHue, 500407);
						// I have not enough money to give thee any!
					}

				}
				Reward(m, t, reward, rewardName);
			}

			public static void Reward(Mobile m, object targeted, Item reward, String rewardName) //Gift Time!
			{
				Mobile t = (Mobile)targeted;
				bool orcs = IsOrc(t);
				bool savage = IsSavage(t);

				if (reward != null)
				{
					m.AddToBackpack(reward);
					t.Say(1074854); // Here, take this...
					m.SendLocalizedMessage(1074853, rewardName); // You have been given ~1_name~

				}
				else
					m.SendMessage("They don't seem notice you"); //Nope didnt get anything at all!



				if (!orcs && !savage && m.Karma > -3000) //If greater than -3K Karma you lose some
				{
					int toLose = m.Karma + 3000;

					if (toLose > 40)
					{
						toLose = 40;
					}

					Titles.AwardKarma(m, -toLose, true);
				}

				else
				{
					t.SendLocalizedMessage(500404); // They seem unwilling to give you any money.
				}
			}
		}
	}
}




