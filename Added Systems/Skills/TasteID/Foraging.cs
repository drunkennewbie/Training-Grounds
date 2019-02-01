using System;
using Server.Items;
using Server.Mobiles;
using Server.Engines.Plants;
using System.Linq;

namespace Server.Engines.Harvest
{
	public class Foraging : HarvestSystem
	{
		private static Foraging m_System;

		public static Foraging System
		{
			get
			{
				if (m_System == null)
					m_System = new Foraging();

				return m_System;
			}
		}

		private HarvestDefinition m_Definition;

		public HarvestDefinition Definition
		{
			get { return m_Definition; }
		}

		private Foraging()
		{
			HarvestResource[] res;
			HarvestVein[] veins;

			#region Foraging
			HarvestDefinition forage = new HarvestDefinition();

			// Resource banks are every 8x8 tiles
			forage.BankWidth = 8;
			forage.BankHeight = 8;

			// Every bank holds from 5-15 attempts
			forage.MinTotal = 5;
			forage.MaxTotal = 15;

			// A resource bank will respawn its content every 20 to 30 minutes
			forage.MinRespawn = TimeSpan.FromMinutes(20.0);
			forage.MaxRespawn = TimeSpan.FromMinutes(30.0);

			// Skill checking is done on the Foraging skill
			forage.Skill = SkillName.TasteID;

			// Set the list of harvestable tiles
			forage.Tiles = m_tiles;
			forage.RangedTiles = true;

			// Players must be within 4 tiles to harvest
			forage.MaxRange = 4;

			// One item per harvest action
			forage.ConsumedPerHarvest = 1;
			forage.ConsumedPerFeluccaHarvest = 1;

			// The foraging
			forage.EffectActions = new int[] { 16 };
			forage.EffectSounds = new int[0];
			forage.EffectCounts = new int[] { 1 };
			forage.EffectDelay = TimeSpan.Zero;
			forage.EffectSoundDelay = TimeSpan.FromSeconds(8.0);

			forage.NoResourcesMessage = "Looks like nothing to forage here"; //Nothing to Forage
			forage.FailMessage = "You search around and couldnt find anything"; // Failed to Forage
			forage.TimedOutOfRangeMessage = "You need to be closer to the forage point"; // Distance?
			forage.OutOfRangeMessage = "You are to far to forage that location"; // To far
			forage.PackFullMessage = "You do not have room in your backpack for foraged item"; // Full backpack
			forage.ToolBrokeMessage = "You broke your foraging kit"; // You broke your foraging kit

			res = new HarvestResource[]
				{
				new HarvestResource( 00.0, 00.0, 100.0, "You found a Bark Fragment",  typeof( BarkFragment ) ),
				new HarvestResource( 00.0, 00.0, 100.0, "You found some Spider Silk", typeof( SpidersSilk )),
				new HarvestResource( 00.0, 00.0, 100.0, "You found some Mandrake Root", typeof( MandrakeRoot)),
				new HarvestResource( 00.0, 00.0, 100.0, "You found some Ginseng", typeof( Ginseng ) ),
				new HarvestResource( 00.0, 00.0, 100.0, "You found some Garlic", typeof( Garlic )),
				new HarvestResource( 00.0, 00.0, 100.0, "You found some Bloodmoss", typeof( Bloodmoss )),
				new HarvestResource( 00.0, 00.0, 100.0, "You found some Black Pearl", typeof( BlackPearl )),
				new HarvestResource( 00.0, 00.0, 100.0, "You found some Sulfuruous Ash", typeof( SulfurousAsh )),
				new HarvestResource( 00.0, 00.0, 100.0, "You found some Nightshade", typeof( Nightshade )),
				};

			veins = new HarvestVein[]
				{
					new HarvestVein( 60.0, 0.0, res[0], null ),	// Barkfragment
					new HarvestVein( 5.0, 0.5, res[1], res[0] ), // Spider Silk
					new HarvestVein( 5.0, 0.5, res[2], res[0] ), // Mandrake Root
					new HarvestVein( 5.0, 0.5, res[3], res[0] ), // Ginseng
					new HarvestVein( 5.0, 0.5, res[4], res[0] ), // Garlic
					new HarvestVein( 5.0, 0.5, res[5], res[0] ), // Bloodmoss
					new HarvestVein( 5.0, 0.5, res[6], res[0] ), // BlackPearl
					new HarvestVein( 5.0, 0.5, res[7], res[0] ), // Sulfurous Ash
					new HarvestVein( 5.0, 0.5, res[8], res[0] ), // Night Shade
					
				};

			forage.Resources = res;
			forage.Veins = veins;
			m_Definition = forage;
			Definitions.Add(forage);
			#endregion
		}

		public override void OnConcurrentHarvest(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
		{
			from.SendMessage("You are already foraging");
		}

		private class MutateEntry
		{
			public double m_ReqSkill, m_MinSkill, m_MaxSkill;
			public Type[] m_Types;
			public bool m_swamp, m_sand, m_dirt, m_snow, m_grass;

			public MutateEntry(double reqSkill, double minSkill, double maxSkill, bool grass, bool dirt, bool sand, bool swamp, bool snow, params Type[] types)
			{
				m_ReqSkill = reqSkill;
				m_MinSkill = minSkill;
				m_MaxSkill = maxSkill;
				m_grass = grass;
				m_dirt = dirt;
				m_sand = sand;
				m_swamp = swamp;
				m_snow = snow;
				m_Types = types;
			}
		}

		private static MutateEntry[] m_MutateTable = new MutateEntry[]
			{
				//      Req Skill, Min skill, Max Skill, Grass, Dirt, Sand, Swamp, Snow, Item
				new MutateEntry(  80.0,  80.0,  4080.0, true, true, false, false, false, typeof( FertileDirt ) ), //Fertile Dirt - Dirt
			    new MutateEntry(  80.0,  80.0,  4080.0, true, true, false,  true, false, typeof( Worm )), //Worm - Dirt
			    new MutateEntry(  50.0,  50.0,  200.0,  true, false, false, false, false, typeof( Carrot )), //Carrot - Grass
			    new MutateEntry(  50.0,  50.0,  200.0,  true, false, false, false, false, typeof( Onion )), //Onion - Grass
				new MutateEntry(  80.0,  80.0,  4080.0, false, false, true, false, false, typeof( Sand ) ), //Sand - Sand
				new MutateEntry(  80.0,  80.0,  4080.0, true, true, true, true, false, typeof( RustyKatana ), typeof( RustyHatchet ), typeof( RustyDagger )), //Rusty Weapons
				new MutateEntry(  80.0,  80.0,  4080.0, false, false, false, false, true, typeof( Snowball ) ), //Snowball - Snow
				new MutateEntry(   0.0, 200.0,  -200.0, true, true, true, true, true, new Type[1]{ null } ) //Nothing - All
			};


		public override Type MutateType(Type type, Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, HarvestResource resource)
		{


			int tileId = map.Tiles.GetLandTile(loc.X, loc.Y).ID;
			if (m_grasstiles.Contains(tileId))
			{
				return Gratiles(type, from, true);
			}
			if (m_sandtiles.Contains(tileId))
			{
				return Santiles(type, from, true);
			}
			if (m_swamptiles.Contains(tileId))
			{
				return Swatiles(type, from, true);
			}
			if (m_snowtiles.Contains(tileId))
			{
				return Snowtiles(type, from, true);
			}
			if (m_dirttiles.Contains(tileId))
			{
				return Ditiles(type, from, true);
			}

			return type;

		}
		public Type Gratiles(Type type, Mobile from, bool tile)
		{
			double skillBase = from.Skills[SkillName.TasteID].Base;
			double skillValue = from.Skills[SkillName.TasteID].Value;

			for (int i = 0; i < m_MutateTable.Length; ++i)
			{
				MutateEntry entry = m_MutateTable[i];

				if (tile && !entry.m_grass)
					continue;

				if (skillBase >= entry.m_ReqSkill)
				{
					double chance = (skillValue - entry.m_MinSkill) / (entry.m_MaxSkill - entry.m_MinSkill);

					if (chance > Utility.RandomDouble())
						return entry.m_Types[Utility.Random(entry.m_Types.Length)];
				}
			}

			return type;
		}
		public Type Swatiles(Type type, Mobile from, bool tile)
		{
			double skillBase = from.Skills[SkillName.TasteID].Base;
			double skillValue = from.Skills[SkillName.TasteID].Value;

			for (int i = 0; i < m_MutateTable.Length; ++i)
			{
				MutateEntry entry = m_MutateTable[i];

				if (tile && !entry.m_swamp)
					continue;

				if (skillBase >= entry.m_ReqSkill)
				{
					double chance = (skillValue - entry.m_MinSkill) / (entry.m_MaxSkill - entry.m_MinSkill);

					if (chance > Utility.RandomDouble())
						return entry.m_Types[Utility.Random(entry.m_Types.Length)];
				}
			}

			return type;
		}
		public Type Snowtiles(Type type, Mobile from, bool tile)
		{
			double skillBase = from.Skills[SkillName.TasteID].Base;
			double skillValue = from.Skills[SkillName.TasteID].Value;

			for (int i = 0; i < m_MutateTable.Length; ++i)
			{
				MutateEntry entry = m_MutateTable[i];

				if (tile && !entry.m_snow)
					continue;

				if (skillBase >= entry.m_ReqSkill)
				{
					double chance = (skillValue - entry.m_MinSkill) / (entry.m_MaxSkill - entry.m_MinSkill);

					if (chance > Utility.RandomDouble())
						return entry.m_Types[Utility.Random(entry.m_Types.Length)];
				}
			}

			return type;
		}
		public Type Santiles(Type type, Mobile from, bool tile)
		{
			double skillBase = from.Skills[SkillName.TasteID].Base;
			double skillValue = from.Skills[SkillName.TasteID].Value;

			for (int i = 0; i < m_MutateTable.Length; ++i)
			{
				MutateEntry entry = m_MutateTable[i];

				if (tile && !entry.m_sand)
					continue;


				if (skillBase >= entry.m_ReqSkill)
				{
					double chance = (skillValue - entry.m_MinSkill) / (entry.m_MaxSkill - entry.m_MinSkill);

					if (chance > Utility.RandomDouble())
						return entry.m_Types[Utility.Random(entry.m_Types.Length)];
				}
			}

			return type;
		}
		public Type Ditiles(Type type, Mobile from, bool tile)
		{
			double skillBase = from.Skills[SkillName.TasteID].Base;
			double skillValue = from.Skills[SkillName.TasteID].Value;

			for (int i = 0; i < m_MutateTable.Length; ++i)
			{
				MutateEntry entry = m_MutateTable[i];

				if (tile && !entry.m_dirt)
					continue;


				if (skillBase >= entry.m_ReqSkill)
				{
					double chance = (skillValue - entry.m_MinSkill) / (entry.m_MaxSkill - entry.m_MinSkill);

					if (chance > Utility.RandomDouble())
						return entry.m_Types[Utility.Random(entry.m_Types.Length)];
				}
			}

			return type;
		}

		private static Map SafeMap(Map map)
		{
			if (map == null || map == Map.Internal)
				return Map.Trammel;

			return map;
		}

		public override bool CheckResources(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, bool timed)
		{
			return base.CheckResources(from, tool, def, map, loc, timed);
		}

		public override bool Give(Mobile m, Item item, bool placeAtFeet)
		{
			if (item is RustyKatana || item is RustyHatchet || item is RustyDagger)
			{
				BaseCreature corp;

				if (0.25 > Utility.RandomDouble())
					corp = new WhippingVine();
				else
					corp = new Corpser();

				int x = m.X, y = m.Y;

				Map map = m.Map;

				for (int i = 0; map != null && i < 20; ++i)
				{
					int tx = m.X - 10 + Utility.Random(21);
					int ty = m.Y - 10 + Utility.Random(21);

					LandTile t = map.Tiles.GetLandTile(tx, ty);

					if (t.Z == -5 && ((t.ID >= 0xA8 && t.ID <= 0xAB) || (t.ID >= 0x136 && t.ID <= 0x137)) && !Spells.SpellHelper.CheckMulti(new Point3D(tx, ty, -5), map))
					{
						x = tx;
						y = ty;
						break;
					}
				}

				corp.MoveToWorld(new Point3D(x, y, -5), map);

				corp.Home = corp.Location;
				corp.RangeHome = 10;

				corp.PackItem(Seed.RandomPeculiarSeed(1));

				m.SendMessage("You attempted to pull a rusty weapon out of a random vine, apparently it was attached to a plant");

				return false; // Yes Give the Weapon to Creature, Seed on corpse now.
			}
			return base.Give(m, item, placeAtFeet);

		}

		public override void SendSuccessTo(Mobile from, Item item, HarvestResource resource)
		{
			string name;

			if (item is FertileDirt)
			{
				name = "some Fertile Dirt";

			}
			else if (item is Sand)
			{
				name = "some Sand";
			}
			else if (item is BarkFragment)
			{
				name = "a Bark Fragment";
			}
			else if (item is Snowball)
			{
				name = "a Snowball";
			}
			else if (item is Worm)
			{
				name = "a Worm";
			}
			else
			{
				name = item.ItemData.Name;
			}


			from.SendMessage("You found {0}", name);

		}

		public override void OnHarvestStarted(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
		{
			base.OnHarvestStarted(from, tool, def, toHarvest);

			int tileID;
			Map map;
			Point3D loc;

			if (GetHarvestDetails(from, tool, toHarvest, out tileID, out map, out loc))
				Timer.DelayCall(TimeSpan.FromSeconds(1.5),
					delegate
					{
						Effects.SendLocationEffect(loc, map, 0x3779, 16, 8);
						Effects.PlaySound(loc, map, 0x386);
					});
		}

		public override void OnHarvestFinished(Mobile from, Item tool, HarvestDefinition def, HarvestVein vein, HarvestBank bank, HarvestResource resource, object harvested)
		{
			base.OnHarvestFinished(from, tool, def, vein, bank, resource, harvested);
			from.RevealingAction();
		}

		public override object GetLock(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
		{
			return this;
		}

		public override bool BeginHarvesting(Mobile from, Item tool)
		{
			if (!base.BeginHarvesting(from, tool))
				return false;

			from.SendMessage("Where do you want to forage");
			return true;
		}

		public override bool CheckHarvest(Mobile from, Item tool)
		{
			if (!base.CheckHarvest(from, tool))
				return false;

			if (from.Mounted)
			{
				from.SendMessage("You can't forage while mounted!");
				return false;
			}

			return true;
		}

		public override bool CheckHarvest(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
		{
			if (!base.CheckHarvest(from, tool, def, toHarvest))
				return false;

			if (from.Mounted)
			{
				from.SendMessage("You can't forage while mounted!");
				return false;
			}

			return true;
		}

		private static int[] m_tiles = new int[]
			{
			//Dirt
			0x71, 0x7C, 0x82, 0xA7, 0xDC, 0xE3, 0xE8, 0xEB,
			0x141, 0x144, 0x14C, 0x14F, 0x169, 0x174, 0x1DC, 0x1E7,
			0x1EC, 0x1EF, 0x272, 0x275, 0x27E, 0x281, 0x2D0, 0x2D7,
			0x2E5, 0x2FF, 0x303, 0x31F, 0x32C, 0x32F, 0x33D, 0x340,
			0x345, 0x34C, 0x355, 0x358, 0x367, 0x36E, 0x377, 0x37A,
			0x38D, 0x390, 0x395, 0x39C, 0x3A5, 0x3A8, 0x3F6, 0x405,
			0x547, 0x54E, 0x553, 0x556, 0x597, 0x59E, 0x623, 0x63A,
			0x6F3, 0x6FA, 0x777, 0x791, 0x79A, 0x7A9, 0x7AE, 0x7B1,

			//Swamp
			0x9C4, 0x9EB, 0x3D65, 0x3D65, 0x3DC0, 0x3DD9, 0x3DDB,
			0x3DDC, 0x3DDE, 0x3EF0, 0x3FF6, 0x3FF6, 0x3FFC, 0x3FFE,
			0x3DEC,

			//Snow
			0x10C, 0x10F, 0x114, 0x117, 0x119, 0x11D, 0x179, 0x18A,
			0x385, 0x38C, 0x391, 0x394, 0x39D, 0x3A4, 0x3A9, 0x3AC,
			0x5BF, 0x5D6, 0x5DF, 0x5E2, 0x745, 0x748, 0x751, 0x758,
			0x75D, 0x760, 0x76D, 0x773,

			//Sand
			0x16, 0x3A, 0x44, 0x4B, 0x11E, 0x121, 0x126, 0x12D, 0x192,
			0x192, 0x1A8, 0x1AB, 0x1B9, 0x1D1, 0x282, 0x285, 0x28A,
			0x291, 0x335, 0x33C, 0x341, 0x344, 0x34D, 0x354, 0x359,
			0x35C, 0x3B7, 0x3BE, 0x3C7, 0x3CA, 0x5A7, 0x5B2, 0x64B,
			0x652, 0x657, 0x65A, 0x663, 0x66A, 0x66F, 0x672, 0x7BD,
			0x7D0,

			//Grass
			0x0003, 0x0004, 0x0005, 0x0006, 0x037B, 0x037C, 0x037D,
			0x037E, 0x03CB, 0x03CC, 0x03CD, 0x03CE, 0x3CDE, 0x3CDF,
			0x3CE0, 0x3CE1, 0x3CE2, 0x3CE3, 0x3CE4, 0x3CE5, 0x3CE6,
			0x3CE7, 0x3CE8, 0x3CE8, 0x3CE9, 0x3CEA, 0x3CEB, 0x3CEC,
			0x3CED, 0x3CEE, 0x3CEF, 0x3CF0, 0x3CF1, 0x3CF2, 0x3CF3,
			0x3CF4, 0x3CF5, 0x3CF6, 0x3CF7, 0x3CF8, 0x3CF9, 0x3CFA,
			0x3CFB, 0x3CFC, 0x3CFD, 0x3CFE, 0x3CFF, 0x3D00, 0x3D01,
			0x3D02, 0x3D03, 0x3D04, 0x3D05, 0x3D06, 0x3D07, 0x3D08,
			0x3D09, 0x3D0A, 0x3D0B, 0x3D0C, 0x3D0D, 0x3D0E, 0x3D0F,
			0x3D10, 0x3D11, 0x3D12, 0x3D13, 0x3D14, 0x3D15, 0x3D16,
			0x3D17, 0x3D18, 0x3D19, 0x3D1A, 0x3D1B, 0x3D1C, 0x3D1D,
			0x3D1E, 0x3D1F, 0x3D20, 0x3D21, 0x3D22, 0x3D23, 0x3D24,
			0x3D25, 0x3D26, 0x3D27, 0x3D28, 0x3D29, 0x3D2A, 0x3D2B,
			0x3D2C, 0x3D2D, 0x3D2E, 0x3D2F, 0x3D30, 0x3D31, 0x3D32,
			0x3D33, 0x3D34, 0x3D35, 0x3D36, 0x3D37, 0x3D38, 0x3D39,
			0x3D3A, 0x3D3B, 0x3D3C, 0x3D3D, 0x3D3E, 0x3D3F, 0x3D40,
			0x3D41, 0x3D42, 0x3D43, 0x3D44, 0x3D45, 0x3D46, 0x3D47,
			0x3D48, 0x3D49, 0x3D4A, 0x3D4B, 0x3D4C, 0x3D4D, 0x3D4E,
			0x3D4F, 0x3D50, 0x3D51, 0x3D52, 0x3D53, 0x3D54, 0x3D55,
			0x3D56, 0x3D57, 0x3D58, 0x3D59, 0x3D5A, 0x3D5B, 0x3D5C,
			0x3D5E, 0x3D5F, 0x3D60, 0x3D61, 0x3D62, 0x3D63, 0x3D64,
			0x3D65, 0x3FF6, 0x3FFC, 0x3FFD, 0x3FFE, 0x3FFF
			};


		private static int[] m_dirttiles = new int[]
		{
			0x71, 0x7C, 0x82, 0xA7, 0xDC, 0xE3, 0xE8, 0xEB,
			0x141, 0x144, 0x14C, 0x14F, 0x169, 0x174, 0x1DC, 0x1E7,
			0x1EC, 0x1EF, 0x272, 0x275, 0x27E, 0x281, 0x2D0, 0x2D7,
			0x2E5, 0x2FF, 0x303, 0x31F, 0x32C, 0x32F, 0x33D, 0x340,
			0x345, 0x34C, 0x355, 0x358, 0x367, 0x36E, 0x377, 0x37A,
			0x38D, 0x390, 0x395, 0x39C, 0x3A5, 0x3A8, 0x3F6, 0x405,
			0x547, 0x54E, 0x553, 0x556, 0x597, 0x59E, 0x623, 0x63A,
			0x6F3, 0x6FA, 0x777, 0x791, 0x79A, 0x7A9, 0x7AE, 0x7B1,
		};

		private static int[] m_swamptiles = new int[]
		{
			0x9C4, 0x9EB, 0x3D65, 0x3D65, 0x3DC0, 0x3DD9, 0x3DDB,
			0x3DDC, 0x3DDE, 0x3EF0, 0x3FF6, 0x3FF6, 0x3FFC, 0x3FFE,
			0x3DEC
		};

		private static int[] m_snowtiles = new int[]
		{
			0x10C, 0x10F, 0x114, 0x117, 0x119, 0x11D, 0x179, 0x18A,
			0x385, 0x38C, 0x391, 0x394, 0x39D, 0x3A4, 0x3A9, 0x3AC,
			0x5BF, 0x5D6, 0x5DF, 0x5E2, 0x745, 0x748, 0x751, 0x758,
			0x75D, 0x760, 0x76D, 0x773,
		};


		private static int[] m_sandtiles = new int[]
		{
			0x16, 0x3A, 0x44, 0x4B, 0x11E, 0x121, 0x126, 0x12D, 0x192,
			0x192, 0x1A8, 0x1AB, 0x1B9, 0x1D1, 0x282, 0x285, 0x28A,
			0x291, 0x335, 0x33C, 0x341, 0x344, 0x34D, 0x354, 0x359,
			0x35C, 0x3B7, 0x3BE, 0x3C7, 0x3CA, 0x5A7, 0x5B2, 0x64B,
			0x652, 0x657, 0x65A, 0x663, 0x66A, 0x66F, 0x672, 0x7BD,
			0x7D0
		};

		private static int[] m_grasstiles = new int[]
		{
			0x0003, 0x0004, 0x0005, 0x0006, 0x037B, 0x037C, 0x037D,
			0x037E, 0x03CB, 0x03CC, 0x03CD, 0x03CE, 0x3CDE, 0x3CDF,
			0x3CE0, 0x3CE1, 0x3CE2, 0x3CE3, 0x3CE4, 0x3CE5, 0x3CE6,
			0x3CE7, 0x3CE8, 0x3CE8, 0x3CE9, 0x3CEA, 0x3CEB, 0x3CEC,
			0x3CED, 0x3CEE, 0x3CEF, 0x3CF0, 0x3CF1, 0x3CF2, 0x3CF3,
			0x3CF4, 0x3CF5, 0x3CF6, 0x3CF7, 0x3CF8, 0x3CF9, 0x3CFA,
			0x3CFB, 0x3CFC, 0x3CFD, 0x3CFE, 0x3CFF, 0x3D00, 0x3D01,
			0x3D02, 0x3D03, 0x3D04, 0x3D05, 0x3D06, 0x3D07, 0x3D08,
			0x3D09, 0x3D0A, 0x3D0B, 0x3D0C, 0x3D0D, 0x3D0E, 0x3D0F,
			0x3D10, 0x3D11, 0x3D12, 0x3D13, 0x3D14, 0x3D15, 0x3D16,
			0x3D17, 0x3D18, 0x3D19, 0x3D1A, 0x3D1B, 0x3D1C, 0x3D1D,
			0x3D1E, 0x3D1F, 0x3D20, 0x3D21, 0x3D22, 0x3D23, 0x3D24,
			0x3D25, 0x3D26, 0x3D27, 0x3D28, 0x3D29, 0x3D2A, 0x3D2B,
			0x3D2C, 0x3D2D, 0x3D2E, 0x3D2F, 0x3D30, 0x3D31, 0x3D32,
			0x3D33, 0x3D34, 0x3D35, 0x3D36, 0x3D37, 0x3D38, 0x3D39,
			0x3D3A, 0x3D3B, 0x3D3C, 0x3D3D, 0x3D3E, 0x3D3F, 0x3D40,
			0x3D41, 0x3D42, 0x3D43, 0x3D44, 0x3D45, 0x3D46, 0x3D47,
			0x3D48, 0x3D49, 0x3D4A, 0x3D4B, 0x3D4C, 0x3D4D, 0x3D4E,
			0x3D4F, 0x3D50, 0x3D51, 0x3D52, 0x3D53, 0x3D54, 0x3D55,
			0x3D56, 0x3D57, 0x3D58, 0x3D59, 0x3D5A, 0x3D5B, 0x3D5C,
			0x3D5E, 0x3D5F, 0x3D60, 0x3D61, 0x3D62, 0x3D63, 0x3D64,
			0x3D65, 0x3FF6, 0x3FFC, 0x3FFD, 0x3FFE, 0x3FFF
		};
	}
}

