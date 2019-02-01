
namespace Server.Items
{

	public class BegXmas
	{
		public static int RandomGiftBoxHue { get { return m_NormalHues[Utility.Random(m_NormalHues.Length)]; } }
		public static int RandomNeonBoxHue { get { return m_NeonHues[Utility.Random(m_NeonHues.Length)]; } }

		private static readonly int[] m_NormalHues =
		{
			0x672,
			0x454,
			0x507,
			0x4ac,
			0x504,
			0x84b,
			0x495,
			0x97c,
			0x493,
			0x4a8,
			0x494,
			0x4aa,
			0xb8b,
			0x84f,
			0x491,
			0x851,
			0x503,
			0xb8c,
			0x4ab,
			0x84B
		};
		private static readonly int[] m_NeonHues =
		{
			0x438,
			0x424,
			0x433,
			0x445,
			0x42b,
			0x448
		};
	}

	public class BegGiftBoxCube : Item
	{
		[Constructable]
		public BegGiftBoxCube() : base(0x46A2)
		{
			Hue = GiftBoxHues.RandomGiftBoxHue;
		}

		public BegGiftBoxCube(Serial serial) : base(serial)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (from == null || from.Backpack == null)
			{
				return;
			}


			if (!IsChildOf(from.Backpack))
			{
				from.SendMessage("This must be in your backpack to open it.");
				return;
			}
			OpenBox(from);

		}



		void OpenBox(Mobile m)
		{
			this.Delete();
			int rand = Utility.Random(8);
			Item reward = null;
			string rewardName = null;
			if (rand == 0)
			{
				reward = new CandyCane();
			}
			else if (rand == 2)
			{
				reward = new Coal();

			}
			else if (rand == 4)
			{
				reward = new CookedBird(2);

			}
			else if (rand == 6)
			{
				//reward = new FurBedRoll();

			}
			else if (rand == 8)
			{
				reward = new BadCard();
			}
			if (reward != null)
			{
				if (rewardName == null)
					rewardName = reward.Name;

				m.AddToBackpack(reward);
				m.SendLocalizedMessage(1074853, rewardName); // You have been given ~1_name~
			}
			else
			{
				switch (Utility.Random(3))
				{
					default:
						m.SendMessage("Krumpus box!");
						AOS.Damage(m, 99, 0, 100, 0, 0, 0);
						m.FixedParticles(0x36BD, 20, 10, 5044, EffectLayer.Head);
						m.PlaySound(0x307);
						break;
					case 1:
						m.SendMessage("The box was empty");
						break;
					case 2:
						Poison poison = Poison.Lethal;
						m.ApplyPoison(m, poison);
						m.SendMessage("A puff of green dust comes out of the box, words appear Krumpus Strikes again!");
						break;
				}

			}




		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();


		}
	}
}
