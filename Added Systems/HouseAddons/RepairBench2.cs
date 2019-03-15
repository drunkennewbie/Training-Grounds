using System;
using System.Collections.Generic;
using System.Linq;
using Server.ContextMenus;
using Server.Engines.Craft;
using Server.Engines.VeteranRewards;
using Server.Gumps;
using Server.Multis;
using Server.Network;
using Server.Targeting;

namespace Server.Items
{
	[Flipable(0x2DDB, 0x2DDC)]
	public class RepairBench : Item
	{

		#region Repair Bench Item
		[Constructable]
		public RepairBench() : base(0x2DDB)
		{
			Name = "Repair Bench";
			Weight = 150.0;
		}

		public RepairBench(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)1);

			writer.Write((int)Level);

			writer.Write(Tools == null ? 0 : Tools.Count);

			if (Tools != null)
			{
				Tools.ForEach(x =>
				{
					writer.Write((int)x.Skill);
					writer.Write((int)x.SkillValue);
					writer.Write((int)x.Charges);
				});
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();

			if (version != 0)
				Level = (SecureLevel)reader.ReadInt();

			Tools = new List<RepairBenchDefinition>();

			int toolcount = reader.ReadInt();

			for (int x = 0; x < toolcount; x++)
			{
				RepairSkillType skill = (RepairSkillType)reader.ReadInt();
				int skillvalue = reader.ReadInt();
				int charge = reader.ReadInt();

				Tools.Add(new RepairBenchDefinition(GetInfo(skill).System, skill, GetInfo(skill).Cliloc, skillvalue, charge));
			}
		}
		#endregion

		public List<RepairBenchDefinition> Tools;

		public static RepairBenchDefinition[] Definitions = new RepairBenchDefinition[]
		{
			new RepairBenchDefinition(DefTinkering.CraftSystem, RepairSkillType.Tinkering, 1044097, 0, 0),
			new RepairBenchDefinition(DefBlacksmithy.CraftSystem, RepairSkillType.Smithing, 1044067, 0, 0),
			new RepairBenchDefinition(DefCarpentry.CraftSystem, RepairSkillType.Carpentry, 1044071, 0, 0),
			new RepairBenchDefinition(DefTailoring.CraftSystem, RepairSkillType.Tailoring, 1044094, 0, 0),
			new RepairBenchDefinition(DefBowFletching.CraftSystem, RepairSkillType.Fletching, 1015156, 0, 0)
		};

		public class RepairBenchDefinition
		{
			public CraftSystem System { get; set; }
			public RepairSkillType Skill { get; set; }
			public double SkillValue { get; set; }
			public int Charges { get; set; }
			public int Cliloc { get; set; }

			public RepairBenchDefinition(CraftSystem system, RepairSkillType skill, int cliloc, double value, int charges)
			{
				System = system;
				Skill = skill;
				Cliloc = cliloc;
				SkillValue = value;
				Charges = charges;
			}
		}

		public static RepairBenchDefinition GetInfo(RepairSkillType type)
		{
			return Definitions.ToList().Find(x => x.Skill == type);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public SecureLevel Level { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Using { get; set; }

		
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

			   
		public override void OnDoubleClick(Mobile from)
		{
			if (!Using)
			{
			//	Using = true;
			//	from.CloseGump(typeof(RepairBenchGump));
			//	from.SendGump(new RepairBenchGump(from, this));
			}
			else
			{
				from.SendLocalizedMessage(500291); // Someone else is using that.
			}
					
		}

		public class ConfirmRemoveGump : Gump
		{
			private RepairBench m_RepairBench;
			private RepairSkillType m_Skill;

			public ConfirmRemoveGump(RepairBench rb, RepairSkillType skill)
				: base(340, 340)
			{
				m_RepairBench = rb;
				m_Skill = skill;

				AddPage(0);

				AddBackground(0, 0, 291, 113, 0x13BE);
				AddImageTiled(5, 5, 280, 80, 0xA40);
				AddHtmlLocalized(9, 9, 272, 80, 1158874, String.Format("#{0}", rb.Tools.Find(x => x.Skill == skill).Cliloc), 0x7FFF, false, false); // Are you sure you wish to remove all the ~1_SKILL~ charges from the bench? This action will delete all existing charges and will not refund any deeds.

				AddButton(5, 87, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
				AddHtmlLocalized(40, 89, 100, 20, 1060051, 0x7FFF, false, false); // CANCEL

				AddButton(160, 87, 0xFB7, 0xFB8, 1, GumpButtonType.Reply, 0);
				AddHtmlLocalized(195, 89, 120, 20, 1006044, 0x7FFF, false, false); // OK
			}

			public override void OnResponse(NetState sender, RelayInfo info)
			{
				if (m_RepairBench == null && !m_RepairBench.Deleted)
					return;

				Mobile m = sender.Mobile;
				int index = info.ButtonID;

				switch (index)
				{
					case 0: { m_RepairBench.Using = false; break; }
					case 1:
						{
							var tool = m_RepairBench.Tools.Find(x => x.Skill == m_Skill);

							tool.SkillValue = 0;
							tool.Charges = 0;

							m.SendLocalizedMessage(1158873, String.Format("#{0}", tool.Cliloc)); // You clear all the ~1_SKILL~ charges from the bench.

							m.SendGump(new RepairBenchGump(m, m_RepairBench));
							break;
						}
				}
			}
		}

		public class RepairBenchGump : Gump
		{
			private RepairBench m_RepairBench;
			private Timer m_Timer;

			public RepairBenchGump(Mobile from, RepairBench rb)
				: base(100, 100)
			{
				m_RepairBench = rb;

				StopTimer(from);

				m_Timer = Timer.DelayCall(TimeSpan.FromMinutes(1), new TimerStateCallback(CloseGump), from);

				AddPage(0);

				AddImageTiled(0, 0, 400, 470, 2604);
				//AddBackground(0, 0, 370, 470, 1416);

				AddImage(82, 0, 0x6E4);
				AddHtmlLocalized(10, 10, 350, 18, 1114513, "Repair Bench", 0x0, false, false); // <DIV ALIGN=CENTER>~1_TOKEN~</DIV>

				AddHtmlLocalized(70, 57, 120, 20, 1114513, "Skill", 0x7FE0, false, false); // <DIV ALIGN=CENTER>~1_TOKEN~</DIV>
				AddHtmlLocalized(213, 57, 190, 20, 1114513, "Charges", 0x7FE0, false, false); // Charges

				AddItem(20, 80, 0x1EB8);
				AddTooltip(1044097);
				AddButton(70, 97, 0x15E1, 0x15E5, 12, GumpButtonType.Reply, 0);
				AddLabel(113, 97, 0x5F, String.Format("{0:F1}", GetSkillValue(RepairSkillType.Tinkering)));
				AddLabel(218, 97, 0x5F, String.Format("{0}", GetCharges(RepairSkillType.Tinkering)));
				AddButton(318, 97, 0x2716, 0x2716, 22, GumpButtonType.Reply, 0);

				AddItem(20, 125, 0x0FB4);
				AddTooltip(1044067);
				AddButton(70, 137, 0x15E1, 0x15E5, 10, GumpButtonType.Reply, 0);
				AddLabel(113, 137, 0x5F, String.Format("{0:F1}", GetSkillValue(RepairSkillType.Smithing)));
				AddLabel(218, 137, 0x5F, String.Format("{0}", GetCharges(RepairSkillType.Smithing)));
				AddButton(318, 137, 0x2716, 0x2716, 20, GumpButtonType.Reply, 0);

				AddItem(20, 170, 0x1034);
				AddTooltip(1044071);
				AddButton(70, 177, 0x15E1, 0x15E5, 13, GumpButtonType.Reply, 0);
				AddLabel(113, 177, 0x5F, String.Format("{0:F1}", GetSkillValue(RepairSkillType.Carpentry)));
				AddLabel(218, 177, 0x5F, String.Format("{0}", GetCharges(RepairSkillType.Carpentry)));
				AddButton(318, 177, 0x2716, 0x2716, 23, GumpButtonType.Reply, 0);

				AddItem(20, 215, 0x0F9D);
				AddTooltip(1044094);
				AddButton(70, 217, 0x15E1, 0x15E5, 11, GumpButtonType.Reply, 0);
				AddLabel(113, 217, 0x5F, String.Format("{0:F1}", GetSkillValue(RepairSkillType.Tailoring)));
				AddLabel(218, 217, 0x5F, String.Format("{0}", GetCharges(RepairSkillType.Tailoring)));
				AddButton(318, 217, 0x2716, 0x2716, 21, GumpButtonType.Reply, 0);

				AddItem(20, 350, 0x1022);
				AddTooltip(1015156);
				AddButton(70, 337, 0x15E1, 0x15E5, 14, GumpButtonType.Reply, 0);
				AddLabel(113, 337, 0x5F, String.Format("{0:F1}", GetSkillValue(RepairSkillType.Fletching)));
				AddLabel(218, 337, 0x5F, String.Format("{0}", GetCharges(RepairSkillType.Fletching)));
				AddButton(318, 337, 0x2716, 0x2716, 24, GumpButtonType.Reply, 0);

				AddButton(70, 407, 0x15E1, 0x15E5, 1, GumpButtonType.Reply, 0);
				AddHtmlLocalized(95, 407, 200, 30, 1153100, 0x7FFF, false, false); // Add Charges
			}

			public void CloseGump(object state)
			{
				Mobile from = state as Mobile;

				StopTimer(from);

				m_RepairBench.Using = false;

				if (from != null && !from.Deleted)
				{
					from.CloseGump(typeof(RepairBenchGump));
				}
			}

			public void StopTimer(Mobile from)
			{
				if (m_Timer != null)
				{
					m_Timer.Stop();
					m_Timer = null;
				}
			}

			private double GetSkillValue(RepairSkillType skill)
			{
				return m_RepairBench.Tools.Find(x => x.Skill == skill).SkillValue;
			}

			private int GetCharges(RepairSkillType skill)
			{
				return m_RepairBench.Tools.Find(x => x.Skill == skill).Charges;
			}

			private class InternalTarget : Target
			{
				private RepairBench m_RepairBench;
				private RepairBenchGump m_Gump;

				public InternalTarget(Mobile from, RepairBenchGump g, RepairBench rb)
					: base(-1, false, TargetFlags.None)
				{
					m_RepairBench = rb;
					m_Gump = g;
				}

				protected override void OnTarget(Mobile from, object targeted)
				{
					if (m_RepairBench == null || m_RepairBench.Deleted)
					{
						return;
					}

					if (!m_RepairBench.CheckAccessible(from, m_RepairBench))
					{
						m_RepairBench.Using = false;
						//m_RepairBench.AccessibleFailMessage(from);
						return;
					}

					if (targeted is RepairDeed)
					{
						RepairDeed deed = (RepairDeed)targeted;

						if (m_RepairBench.Tools.Any(x => x.Skill == deed.RepairSkill && x.Charges >= 500))
						{
							from.SendLocalizedMessage(1158778); // This would exceed the maximum charges allowed on this magic item.
							from.Target = new InternalTarget(from, m_Gump, m_RepairBench);
						}
						else if (m_RepairBench.Tools.Any(x => x.Skill == deed.RepairSkill && x.Charges != 0 && x.SkillValue != deed.SkillLevel))
						{
							from.SendLocalizedMessage(1158866); // The repair bench contains deeds that do not match the skill of the deed you are trying to add.
							from.Target = new InternalTarget(from, m_Gump, m_RepairBench);
						}
						else
						{
							var tool = m_RepairBench.Tools.Find(x => x.Skill == deed.RepairSkill);

							tool.SkillValue = deed.SkillLevel;
							tool.Charges++;

							deed.Delete();

							from.Target = new InternalTarget(from, m_Gump, m_RepairBench);
						}
					}
					else if (targeted is Container)
					{
						Container c = targeted as Container;

						for (int i = c.Items.Count - 1; i >= 0; --i)
						{
							if (i < c.Items.Count && c.Items[i] is RepairDeed)
							{
								RepairDeed deed = (RepairDeed)c.Items[i];

								if (m_RepairBench.Tools.Any(x => x.Skill == deed.RepairSkill && x.Charges >= 500))
								{
									from.SendLocalizedMessage(1158778); // This would exceed the maximum charges allowed on this magic item.
								}
								else if (m_RepairBench.Tools.Any(x => x.Skill == deed.RepairSkill && x.SkillValue == deed.SkillLevel))
								{
									var tool = m_RepairBench.Tools.Find(x => x.Skill == deed.RepairSkill);

									tool.SkillValue = deed.SkillLevel;
									tool.Charges++;

									deed.Delete();
								}
							}
						}
					}
					else
					{
						from.SendLocalizedMessage(1158865); // That is not a valid repair contract or container.
					}

					m_Gump.StopTimer(from);
					from.CloseGump(typeof(RepairBenchGump));
					from.SendGump(new RepairBenchGump(from, m_RepairBench));
				}

				protected override void OnTargetCancel(Mobile from, TargetCancelType cancelType)
				{
					if (m_RepairBench != null && !m_RepairBench.Deleted)
					{
						m_Gump.StopTimer(from);
						from.CloseGump(typeof(RepairBenchGump));
						from.SendGump(new RepairBenchGump(from, m_RepairBench));
					}
				}
			}

			public override void OnResponse(NetState sender, RelayInfo info)
			{
				Mobile from = sender.Mobile;

				int index = info.ButtonID;

				if (index == 0)
				{
					m_RepairBench.Using = false;
				}
				else if (index == 1)
				{
					StopTimer(from);
					from.SendLocalizedMessage(1158871); // Which repair deed or container of repair deeds do you wish to add to the repair bench?
					from.Target = new InternalTarget(from, this, m_RepairBench);
				}
				else if (index >= 10 && index < 20)
				{
					StopTimer(from);
					int skillindex = index - 10;
					Repair.Do(from, RepairSkillInfo.GetInfo((RepairSkillType)skillindex).System, m_RepairBench);
				}
				else
				{
					BaseHouse house = BaseHouse.FindHouseAt(m_RepairBench);

					if (house != null && house.IsOwner(from))
					{
						StopTimer(from);
						int skillindex = index - 20;
						from.SendGump(new ConfirmRemoveGump(m_RepairBench, (RepairSkillType)skillindex));
					}
					else
					{
						from.SendLocalizedMessage(1005213); // You can't do that
						m_RepairBench.Using = false;
					}
				}
			}
		}
	}
}
