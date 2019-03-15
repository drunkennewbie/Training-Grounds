using System;
using Server;
using Server.Mobiles;
using Server.ContextMenus;
using System.Collections.Generic;
using Server.Gumps;
using Server.Multis;



namespace Server.Items
{
	[Flipable(0x4C5E, 0x4C5F)]
	public class CorpseSummonBell : Item, ISecurable
	{
		private static Dictionary<Mobile, CorpseRetrieveTimer> _Timers = new Dictionary<Mobile, CorpseRetrieveTimer>();

		private SecureLevel m_Level;

		[CommandProperty(AccessLevel.GameMaster)]
		public SecureLevel Level
		{
			get { return m_Level; }
			set { m_Level = value; }
		}

		
		private bool _SummonAll;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool SummonAll { get { return _SummonAll; } set { _SummonAll = value; InvalidateProperties(); } }

		[Constructable]
		public CorpseSummonBell() : base (0x4C5E)
		{
			Name = "Corpse Summon Bell";
			SummonAll = true;
			Weight = 100;
		}

		public CorpseSummonBell(Serial serial) : base (serial)
		{
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);

			SetSecureLevelEntry.AddTo(from, this, list); // Set secure level
		}

		public bool HasAccces(Mobile m)
		{
			if (m.AccessLevel >= AccessLevel.GameMaster)
				return true;

			BaseHouse house = BaseHouse.FindHouseAt(this);

			return (house != null && house.HasAccess(m));
		}

		public override void OnDoubleClick(Mobile m)
		{
			if (!m.InRange(this.GetWorldLocation(), 2))
			{
				m.SendLocalizedMessage(500295); // You are too far away to do that.
			}
			else if (IsLockedDown && !HasAccces(m))
				m.SendLocalizedMessage(502436); // That is not accessible.
			else if (!_Timers.ContainsKey(m))
			{
				TryGetCorpse(m);
			}
		}

		private void TryGetCorpse(Mobile m)
		{
			if (CanGetCorpse(m))
			{
				m.PlaySound(0xF5);

				if (_SummonAll)
				{
					var corpses = GetCorpses(m);

					if (corpses != null)
					{
						m.SendMessage("The bell rings loudly and tries to draw your {0} corpses to you", corpses.Count.ToString()); 

						_Timers[m] = new CorpseRetrieveTimer(m, corpses, this);
					}
					else
					{
						m.SendLocalizedMessage(503381); //You feel a gathering of magical energy around you, but it strangely dissipates with no effect.
					}
				}
				else
				{
					var corpse = GetCorpse(m);

					if (corpse != null)
					{
						m.SendMessage("The bell rings loudly and tries to draw your corpse to you");

						_Timers[m] = new CorpseRetrieveTimer(m, new List<Corpse> { corpse }, this);
					}
					else
					{
						m.SendLocalizedMessage(503381); // //You feel a gathering of magical energy around you, but it strangely dissipates with no effect.
					}
				}
			}
		}

		private bool CanGetCorpse(Mobile m, bool firstCheck = true)
		{
			if (Spells.SpellHelper.CheckCombat(m))
			{
				m.SendLocalizedMessage(1071514); // You cannot use this item during the heat of battle.
				return false;
			}

			return true;
		}

		public void TryEndSummon(Mobile m, List<Corpse> corpses)
		{
			if (_Timers.ContainsKey(m))
				_Timers.Remove(m);

			if (corpses == null || corpses.Count == 0)
			{
				m.SendMessage("The bell stops ringing... it seems unable to locate your corpse");
				return;
			}

			bool tooFar = false;
			bool notEnoughTime = false;
			bool tooManySummons = false;
			bool success = true;

			if (_SummonAll)
			{
				List<Corpse> copy = new List<Corpse>(corpses);

				foreach (var c in copy)
				{
					bool remove = false;

					if (c.Map != m.Map)
					{
						remove = true;
						tooFar = true;
					}

					if (Corpse.PlayerCorpses.ContainsKey(c) && Corpse.PlayerCorpses[c] >= 3)
					{
						remove = true;
						tooManySummons = true;
					}

					if (!m.InRange(c.GetWorldLocation(), 30))
					{
						remove = true;
						tooFar = true;
					}
					
					if (remove)
						corpses.Remove(c);
				}

				if (corpses.Count == 0)
					success = false;
			}
			else
			{
				Corpse c = corpses[0];

				if (c.Map != m.Map)
					tooFar = true;

				if (c.Killer is PlayerMobile && c.Killer != m && c.TimeOfDeath + TimeSpan.FromSeconds(180) > DateTime.UtcNow)
					notEnoughTime = true;

				if (Corpse.PlayerCorpses != null && Corpse.PlayerCorpses.ContainsKey(c) && Corpse.PlayerCorpses[c] >= 3)
					tooManySummons = true;

				if (tooFar || notEnoughTime || tooManySummons)
				{
					if (tooFar)
						m.SendLocalizedMessage(1071512); // ...but the corpse is too far away!
					else
						m.SendLocalizedMessage(1071517); // ...but the corpse has already been summoned too many times!

					success = false;
				}
			}

			if (success)
			{
				m.PlaySound(0xFA);

				foreach (var c in corpses)
				{
					c.MoveToWorld(m.Location, m.Map);

					if (Corpse.PlayerCorpses != null && Corpse.PlayerCorpses.ContainsKey(c))
						Corpse.PlayerCorpses[c]++;
				}

				if (_SummonAll)
				{
					m.SendLocalizedMessage(1071530, corpses.Count.ToString()); // ...and succeeds in summoning ~1_COUNT~ of them!

					if (tooFar)
						m.SendLocalizedMessage(1071513); // ...but one of them is too far away!
					else if (notEnoughTime)
						m.SendLocalizedMessage(1071516); // ...but one of them deflects the magic because of the stain of war!
					else if (tooManySummons)
						m.SendLocalizedMessage(1071519); // ...but one of them has already been summoned too many times!
				}
				else
					m.SendLocalizedMessage(1071529); // ...and succeeds in the summoning of it!

			}
		}

		private int GetCorpseCount(Mobile m)
		{
			if (Corpse.PlayerCorpses == null)
				return 0;

			int count = 0;

			foreach (var kvp in Corpse.PlayerCorpses)
			{
				if (kvp.Key.Owner == m && kvp.Value < 3)
					count++;
			}

			return count;
		}

		private List<Corpse> GetCorpses(Mobile m)
		{
			if (Corpse.PlayerCorpses == null)
				return null;

			List<Corpse> list = null;

			foreach (var kvp in Corpse.PlayerCorpses)
			{
				if (kvp.Key.Owner == m && kvp.Value < 3)
				{
					if (list == null)
						list = new List<Corpse>();

					if (!list.Contains(kvp.Key))
						list.Add(kvp.Key);
				}

				if (list != null && list.Count >= 15)
					break;
			}

			return list;
		}

		private Corpse GetCorpse(Mobile m)
		{
			var corpse = m.Corpse as Corpse;

			if (corpse == null || Corpse.PlayerCorpses == null || !Corpse.PlayerCorpses.ContainsKey(corpse))
				return null;

			return corpse;
		}

		public static bool TryRemoveTimer(Mobile m)
		{
			if (_Timers.ContainsKey(m))
			{
				_Timers[m].Stop();
				_Timers.Remove(m);

				m.FixedEffect(0x3735, 6, 30);
				m.PlaySound(0x5C);

				m.SendLocalizedMessage(1071525); // You have been disrupted while attempting to pull your corpse!
				return true;
			}

			return false;
		}

		public bool IsSummoning()
		{
			foreach (var timer in _Timers.Values)
			{
				if (timer.Bell == this)
					return true;
			}

			return false;
		}

		public class CorpseRetrieveTimer : Timer
		{
			public Mobile From { get; set; }
			public List<Corpse> Corpses { get; set; }
			public CorpseSummonBell Bell { get; set; }

			public CorpseRetrieveTimer(Mobile from, List<Corpse> corpses, CorpseSummonBell bell)
				: base(TimeSpan.FromSeconds(3))
			{
				From = from;
				Corpses = corpses;
				Bell = bell;

				Start();
			}

			protected override void OnTick()
			{
				Bell.TryEndSummon(From, Corpses);
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0); // version
			writer.Write((int)m_Level);

			writer.Write(_SummonAll);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();

			m_Level = (SecureLevel)reader.ReadInt();
			_SummonAll = reader.ReadBool();
		}
	}
}
