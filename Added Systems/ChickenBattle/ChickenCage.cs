using System;
using System.Collections.Generic;
using Server.ContextMenus;
using Server.Mobiles;
using Server.Network;
using Server.Regions;
using Server.Spells;
using Server.Targeting;

namespace Server.Items
{
	public class ChickenCage : Item, TranslocationItem 
	{
		private int m_Charges;
		private int m_Recharges;
		private BaseCreature m_Pet;
		private string m_PetName;

		private Dictionary<Mobile, List<BaseCreature>> m_Stored = new Dictionary<Mobile, List<BaseCreature>>();
		public Dictionary<Mobile, List<BaseCreature>> Stored { get { return m_Stored; } }

		[Constructable]
		public ChickenCage()
			: base(0x44d0)
		{
			this.Weight = 10.0;
			this.Hue = 0x1;
			this.m_Charges = 1;

			this.m_PetName = "";
		}

		public ChickenCage(Serial serial)
			: base(serial)
		{
		}

		private delegate void CageCallback(Mobile from);
		[CommandProperty(AccessLevel.GameMaster)]
		public int Charges
		{
			get
			{
				return this.m_Charges;
			}
			set
			{
				if (value > this.MaxCharges)
					this.m_Charges = this.MaxCharges;
				else if (value < 0)
					this.m_Charges = 0;
				else
					this.m_Charges = value;

				this.InvalidateProperties();
			}
		}
		[CommandProperty(AccessLevel.GameMaster)]
		public int Recharges
		{
			get
			{
				return this.m_Recharges;
			}
			set
			{
				if (value > this.MaxRecharges)
					this.m_Recharges = this.MaxRecharges;
				else if (value < 0)
					this.m_Recharges = 0;
				else
					this.m_Recharges = value;

				this.InvalidateProperties();
			}
		}
		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxCharges
		{
			get
			{
				return 1;
			}
		}
		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxRecharges
		{
			get
			{
				return 0;
			}
		}
		public string TranslocationItemName
		{
			get
			{
				return "Chicken Cage";
			}
		}
		[CommandProperty(AccessLevel.GameMaster)]
		public BaseCreature Pet
		{
			get
			{
				if (this.m_Pet != null && this.m_Pet.Deleted)
				{
					this.m_Pet = null;
					this.InternalUpdatePetName();
				}

				return this.m_Pet;
			}
			set
			{
				this.m_Pet = value;
				this.InternalUpdatePetName();
			}
		}
		[CommandProperty(AccessLevel.GameMaster)]
		public string PetName
		{
			get
			{
				return this.m_PetName;
			}
		}
		public override void AddNameProperty(ObjectPropertyList list)
		{
			list.Add("A chicken cage : [Caged pet: " + this.m_PetName + "]"); 
		}

		public override void OnSingleClick(Mobile from)
		{
			this.LabelTo(from, "A chicken cage : [Caged pet: " + this.m_PetName + "]");
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);

			if (from.Alive && this.RootParent == from)
			{
				if (this.Pet == null)
				{
					list.Add(new CageEntry(new CageCallback(CageChicken), 6180));
				}
				else
				{
					list.Add(new CageEntry(new CageCallback(CastReleaseChicken), 6181));
					list.Add(new CageEntry(new CageCallback(UpdatePetName), 6183));
				}
			}
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (this.RootParent != from) // TODO: Previous implementation allowed use on ground, without house protection checks. What is the correct behavior?
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1042001); // That must be in your pack for you to use it.
				return;
			}

			if (this.Pet == null)
			{
				this.CageChicken(from);
			}
			else
			{
				this.CastReleaseChicken(from);
			}
		}

		public void CageChicken(Mobile from)
		{
			BaseCreature pet = this.Pet;

			if (this.Deleted || pet != null || this.RootParent != from)
				return;

			from.SendMessage("Target the chicken you want to cage"); // Target your pet that you wish to link to this Crystal Ball of Pet Summoning.
			from.Target = new CageChickenTarget(this);
		}

		public void CastReleaseChicken(Mobile from)
		{
			BaseCreature pet = this.Pet;

			if (this.Deleted || pet == null || this.RootParent != from)
				return;
			if (this.Charges == 0)
			{
				this.Delete();
			}
			else if (pet.Map == Map.Internal && (!pet.IsStabled || (from.Followers + pet.ControlSlots) > from.FollowersMax))
			{
				MessageHelper.SendMessageTo(this, from, "Your chicken doesn't want to come out", 0x5); 
			}
			else if (this.BlessedFor != from)
			{
				MessageHelper.SendMessageTo(this, from, "The chicken refuses to obey you", 0x8FD);
			}
			else
			{
				this.ReleaseChicken(from);
			}
		}

		public void ReleaseChicken(Mobile from)
		{
			if (Deleted || !from.CheckAlive() || !m_Stored.ContainsKey(from))
				return;

			bool claimed = false;
			int stabledCount = 0;

			List<BaseCreature> stabled = m_Stored[from];

			for (int i = 0; i < stabled.Count; ++i)
			{
				BaseCreature pet = stabled[i] as BaseCreature;

				if (pet == null || pet.Deleted)
				{
					pet.IsStabled = false;
					stabled.RemoveAt(i);
					--i;
					continue;
				}

				++stabledCount;

				if ((from.Followers + pet.ControlSlots) <= from.FollowersMax)
				{
					pet.SetControlMaster(from);

					if (pet.Summoned)
						pet.SummonMaster = from;

					pet.ControlTarget = from;
					pet.ControlOrder = OrderType.Follow;

					pet.MoveToWorld(from.Location, from.Map);

					pet.IsStabled = false;

					stabled.RemoveAt(i);
					--i;

					claimed = true;
					this.Delete();
				}
				else
				{
					from.SendMessage("You are unable to release chicken cause you have to many followers"); // ~1_NAME~ remained in the stables because you have too many followers.
				}
			}
			MessageHelper.SendMessageTo(this, from, "You release the chicken from the cage, the cage is to damaged to be reused", 0x43);

			if (from is PlayerMobile)
			{
				((PlayerMobile)from).LastPetBallTime = DateTime.UtcNow;
			}
		}

		public void UpdatePetName(Mobile from)
		{
			this.InternalUpdatePetName();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt((int)1); // version

			writer.WriteEncodedInt((int)this.m_Recharges);
			writer.Write(m_Stored.Count);
			foreach (KeyValuePair<Mobile, List<BaseCreature>> kvp in m_Stored)
			{
				writer.Write(kvp.Key);
				writer.Write(kvp.Value.Count);

				foreach (BaseCreature bc in kvp.Value)
					writer.Write(bc);
			}
			writer.WriteEncodedInt((int)this.m_Charges);
			writer.Write((Mobile)this.Pet);
			writer.Write((string)this.m_PetName);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadEncodedInt();

			switch (version)
			{
				case 1:
					{
						this.m_Recharges = reader.ReadEncodedInt();
						goto case 0;
					}
				case 0:
					{
						this.m_Charges = Math.Min(reader.ReadEncodedInt(), this.MaxCharges);
						this.Pet = (BaseCreature)reader.ReadMobile();
						this.m_PetName = reader.ReadString();
						goto case 2 ;
					}
				case 2:
					{
						int c = reader.ReadInt();

						for (int i = 0; i < c; i++)
						{
							Mobile owner = reader.ReadMobile();
							int count = reader.ReadInt();
							List<BaseCreature> list = new List<BaseCreature>();

							for (int j = 0; j < count; j++)
							{
								Mobile chicken = reader.ReadMobile();

								if (chicken != null && chicken is BaseCreature)
								{
									var bc = chicken as BaseCreature;
									bc.IsStabled = true;
									list.Add(bc);
								}
							}

							if (owner != null && list.Count > 0)
								m_Stored.Add(owner, list);
						}
						break;
					}
			}
		}

		private void InternalUpdatePetName()
		{
			BaseCreature pet = this.Pet;

			if (pet == null)
				this.m_PetName = "";
			else
				this.m_PetName = pet.Name;

			this.InvalidateProperties();
		}

		private class CageEntry : ContextMenuEntry
		{
			private readonly CageCallback m_Callback;
			public CageEntry(CageCallback callback, int number)
				: base(number, 2)
			{
				this.m_Callback = callback;
			}

			public override void OnClick()
			{
				Mobile from = this.Owner.From;

				if (from.CheckAlive())
					this.m_Callback(from);
			}
		}


		public override void Delete()
		{
			if (m_Stored != null && m_Stored.Count > 0)
			{
				List<List<BaseCreature>> masterList = new List<List<BaseCreature>>(m_Stored.Values);

				for (int i = 0; i < masterList.Count; i++)
				{
					for (int j = 0; j < masterList[i].Count; j++)
					{
						if (masterList[i][j] != null && !masterList[i][j].Deleted)
							masterList[i][j].Delete();
					}
				}

				m_Stored.Clear();
			}

			base.Delete();
		}

		private class CageChickenTarget : Target
		{
			private readonly ChickenCage m_Cage;
			public CageChickenTarget(ChickenCage cage)
				: base(-1, false, TargetFlags.None)
			{
				this.m_Cage = cage;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (this.m_Cage.Deleted || this.m_Cage.Pet != null)
					return;

				if (this.m_Cage.RootParent != from)
				{
					from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1042001); // That must be in your pack for you to use it.
				}
				else if (targeted is Chicken || targeted is BattleChickenLizard)
				{
					BaseCreature creature = (BaseCreature)targeted;

					if (!creature.Controlled || creature.ControlMaster != from)
					{
						MessageHelper.SendMessageTo(this.m_Cage, from, "You can't cage a chicken that isnt yours", 0x59); 
					}
					else if (creature.Combatant != null && creature.InRange(creature.Combatant, 12) && creature.Map == creature.Combatant.Map)
					{
						MessageHelper.SendMessageTo(this.m_Cage, from, "Your chicken seems to busy", 0x59);
					}
					else
					{
						MessageHelper.SendMessageTo(this.m_Cage, from, "You pick up the chicken and place it in the cage", 0x59);

						this.m_Cage.Pet = creature;

						m_Cage.ChickenCaged(from, (BaseCreature)targeted);

						

					}
				}
				else if (targeted == this.m_Cage)
				{
					MessageHelper.SendMessageTo(this.m_Cage, from, "That makes no sense at all to even try.", 0x59);
				}
				else
				{
					MessageHelper.SendMessageTo(this.m_Cage, from, "You really doubt that will fit in this cage", 0x59);
				}
			}
		}

		public void ChickenCaged(Mobile from, BaseCreature pet)
		{
			pet.ControlTarget = null;
			pet.ControlOrder = OrderType.Stay;
			pet.Internalize();

			pet.SetControlMaster(null);
			pet.SummonMaster = null;

			pet.IsStabled = true;

			if (!m_Stored.ContainsKey(from))
				m_Stored.Add(from, new List<BaseCreature>());

			if (!m_Stored[from].Contains(pet))
				m_Stored[from].Add(pet);

			this.BlessedFor = from;

		}


	}
}
