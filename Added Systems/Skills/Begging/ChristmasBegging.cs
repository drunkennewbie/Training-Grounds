using System;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Engines.Events
{
	public class HolidayGivings
	{
		public static TimeSpan OneSecond = TimeSpan.FromSeconds(1);
		public static void Initialize()
		{
			DateTime now = DateTime.UtcNow;

			EventSink.Speech += new SpeechEventHandler(EventSink_Speech);

		}



		public static bool CheckMobile(Mobile mobile)
		{
			return (mobile != null && mobile.Map != null && !mobile.Deleted && mobile.Alive && mobile.Map != Map.Internal);
		}

		private static void EventSink_Speech(SpeechEventArgs e)
		{
			if (Insensitive.Contains(e.Speech, "Happy Holidays"))
			{
				e.Mobile.Target = new HolidayGivingsTarget();

				// e.SendMessage("Who would you wish Happy Holidays to?");
			}
		}

		private class HolidayGivingsTarget : Target
		{
			public HolidayGivingsTarget()
				: base(15, false, TargetFlags.None)
			{
			}

			protected override void OnTarget(Mobile from, object targ)
			{
				if (targ != null && CheckMobile(from))
				{
					if (!(targ is Mobile))
					{
						from.SendMessage("There is Little chance of getting gifts from that!");
						return;
					}
					if (!(targ is BaseVendor) || ((BaseVendor)targ).Deleted)
					{
						from.SendMessage("They dont seem to care.");
						return;
					}

					DateTime now = DateTime.UtcNow;

					BaseVendor m_Begged = targ as BaseVendor;

					if (CheckMobile(m_Begged))
					{
						if (m_Begged.NextChristmasGiving > now)
						{
							from.SendMessage("They appear not to have any gifts at the moment");
							return;
						}

						m_Begged.NextChristmasGiving = now + TimeSpan.FromMinutes(Utility.RandomMinMax(60, 120));
						ChristmasPrize(from, targ);

					}
				}
			}
		}

		public static void ChristmasPrize(Mobile from, object targ) //What do you get? Krampus or Nicholas?
		{
			if (from.Backpack != null && !from.Backpack.Deleted)
			{
				Mobile begged = (Mobile)targ;
				if (Utility.RandomDouble() > .10)
				{
					switch (Utility.Random(3))
					{
						case 0:
							begged.Say(1076768);
							break; // Oooooh, aren't you cute! 
						case 1:
							begged.Say("Alright since you were good this year");
							break;
						case 2:
							begged.Say(1076778);
							break; // Here you go! Enjoy! 
						default:
							break;
					}

					if (Utility.RandomDouble() <= .01 && from.Skills.Begging.Value >= 100)
					{
						from.AddToBackpack(new BegGiftBoxCube());
						from.SendLocalizedMessage(1076777); // You receive a special treat!
					}
					else
					{
						HolidayCandy(from, begged);

					}
				}
				else
				{
					from.SendMessage("Krumpus laughs in the distance");

					int m_Action = Utility.Random(3);

					if (m_Action == 0)
					{
						Timer.DelayCall<Mobile>(OneSecond, OneSecond, 10, new TimerStateCallback<Mobile>(Bleeding), from);
					}
					else
					{
						Timer.DelayCall<Mobile>(TimeSpan.FromSeconds(2), new TimerStateCallback<Mobile>(ToGate), from);
					}
				}
			}
		}
		public static void HolidayCandy(Mobile from, Mobile begged)
		{
			int rand = Utility.Random(7);
			Item reward = null;
			if (rand == 0)
			{
				reward = new CandyCane();
				from.AddToBackpack(reward);

			}
			else if (rand == 1)
			{
				reward = new GingerBreadCookie();
				from.AddToBackpack(reward);
			}
			else if (rand == 2)
			{
				reward = new BeverageBottle(BeverageType.Milk);
				from.AddToBackpack(reward);
			}
			else if (rand == 3)
			{
				reward = new Cookies();
				from.AddToBackpack(reward);
			}
			else
			{
				Timer.DelayCall<Mobile>(TimeSpan.FromSeconds(3), apologize, begged);
			}


		}

		public static void apologize(Mobile begged) //Ops didnt have enough
		{
			begged.Say("Awww... Sorry nevermind I guess I dont have anything to give");
		}
		public static void ToGate(Mobile target) //Teleports Player Away
		{
			if (TrickOrTreat.CheckMobile(target))
			{
				target.LocalOverheadMessage(Network.MessageType.Regular, 0x3b2, false, "Krumpus teleports you away");

				target.MoveToWorld(RandomMoongate(target), target.Map);
			}
		}
		public static Point3D RandomMoongate(Mobile target) //Picks Moongate Location
		{
			Map map = target.Map;
			return Felucca_Locations[Utility.Random(Felucca_Locations.Length)];

		}
		private static Point3D[] Felucca_Locations = //Locations of Moongates
		{
			new Point3D( 4467, 1283, 5 ), // Moonglow
			new Point3D( 1336, 1997, 5 ), // Britain
			new Point3D( 1499, 3771, 5 ), // Jhelom
			new Point3D(  771,  752, 5 ), // Yew
			new Point3D( 2701,  692, 5 ), // Minoc
			new Point3D( 1828, 2948,-20), // Trinsic
			new Point3D(  643, 2067, 5 ), // Skara Brae
			
		};

		public static void Bleeding(Mobile m_From) //Fake Bleeding
		{
			m_From.LocalOverheadMessage(Network.MessageType.Regular, 0x3b2, false, "You feel a whip hit your back");
			Point3D point = RandomPointOneAway(m_From.X, m_From.Y, m_From.Z, m_From.Map);
			int amount = Utility.RandomMinMax(3, 7);

			for (int i = 0; i < amount; i++)
			{
				new Blood(Utility.RandomMinMax(0x122C, 0x122F)).MoveToWorld(RandomPointOneAway(m_From.X, m_From.Y, m_From.Z, m_From.Map), m_From.Map);
			}


		}
		public static Point3D RandomPointOneAway(int x, int y, int z, Map map)
		{
			Point3D loc = new Point3D(x + Utility.Random(-1, 3), y + Utility.Random(-1, 3), 0);

			loc.Z = (map.CanFit(loc, 0)) ? map.GetAverageZ(loc.X, loc.Y) : z;

			return loc;
		}

		/*public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }*/
	}
}

