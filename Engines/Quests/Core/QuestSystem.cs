using System;
using System.Collections.Generic;
using Server.Commands;
using Server.Engines.Quests.Gumps;
using Server.Engines.Quests.Objectives;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using System.Collections;
using Server.Commands.Generic;
using Server.Items;
using System.IO;
using Server.ContextMenus;
using Server.Targeting;

namespace Server.Engines.Quests
{
	public delegate void QuestCallback();

	public abstract class QuestSystem
	{
		
		public static readonly Type[] QuestTypes = new Type[]
			{
				typeof( Doom.TheSummoningQuest ),
				typeof( Necro.DarkTidesQuest ),
				typeof( Haven.UzeraanTurmoilQuest ),
				typeof( Collector.CollectorQuest ),
				typeof( Hag.WitchApprenticeQuest ),
				typeof( Naturalist.StudyOfSolenQuest ),
				typeof( Matriarch.SolenMatriarchQuest ),
				typeof( Ambitious.AmbitiousQueenQuest ),
				typeof( Ninja.EminosUndertakingQuest ),
				typeof( Samurai.HaochisTrialsQuest ),
				typeof( Zento.TerribleHatchlingsQuest )
			};

		public static bool Enabled { get { return true; } }

		public const int MaxConcurrentQuestsAllowed = 10; //Max Quests allowed.
		public const int SpeechColorDefault = 0x3B2; //Speech Color

		public static readonly bool AutoGenerateNew = true;
		public static readonly bool Debug = true;

		private static Dictionary<Type, Quests> m_Quests;
		private static Dictionary<Type, List<Quests>> m_QuestGivers;
		private static Dictionary<PlayerMobile, QuestContext> m_Contexts;

		public static readonly List<Quests> EmptyList = new List<Quests>();

	    public static Dictionary<Type, Quests> Quests //Quest Info
		{
			get { return m_Quests; }
		}
		public static Dictionary<Type, List<Quests>> QuestGivers //Quest Giver Information
		{
			get { return m_QuestGivers; }
		}
		public static Dictionary <PlayerMobile, QuestContext> Contexts
		{
			get { return m_Contexts; }
		}

		static QuestSystem()
		{
			m_Quests = new Dictionary<Type, Quests>();
			m_QuestGivers = new Dictionary<Type, List<Quests>>();
			m_Contexts = new Dictionary<PlayerMobile, QuestContext>();

			string cfgPath = Path.Combine(Core.BaseDirectory, Path.Combine("Data", "NewQuests.cfg"));

			Type baseQuestType = typeof(Quests);
			Type baseQuesterType = typeof(IQuestGivers); //Quest Giver Interface

			if (File.Exists(cfgPath))
			{
				using (StreamReader sr = new StreamReader(cfgPath))
				{
					string line;

					while ((line = sr.ReadLine()) != null)
					{
						if (line.Length == 0 || line.StartsWith("#"))
							continue;

						string[] split = line.Split('\t');

						Type type = ScriptCompiler.FindTypeByName(split[0]);

						if (type == null || !baseQuestType.IsAssignableFrom(type))
						{
							if (Debug)
								Console.WriteLine("Warning: {1} quest type '{0}'", split[0], (type == null) ? "Null" : "Invalid");

							continue;
						}

						Quests quest = null;

						try
						{
							quest = Activator.CreateInstance(type) as Quests;
						}
						catch { }

						if (quest == null)
							continue;

						Register(type, quest);

						for (int i = 1; i < split.Length; ++i)
						{
							Type questerType = ScriptCompiler.FindTypeByName(split[i]);

							if (questerType == null || !baseQuesterType.IsAssignableFrom(questerType))
							{
								if (Debug)
									Console.WriteLine("Warning: {1} quester type '{0}'", split[i], (questerType == null) ? "Unknown" : "Invalid");

								continue;
							}

							RegisterQuestGiver(quest, questerType);
						}
					}
				}
			}
		}

		private static void Register (Type type, Quests quest)
		{
			m_Quests[type] = quest;
		}

		private static void RegisterQuestGiver (Quests quest, Type questertype)
		{
			List<Quests> questList;

			if (!m_QuestGivers.TryGetValue(questertype, out questList))
				m_QuestGivers[questertype] = questList = new List<Quests>();
			questList.Add(quest);
		}
		private static void Register (Quests quest, params Type[] questerTypes)
		{
			Register(quest.GetType(), quest);
			foreach (Type questerType in questerTypes)
				RegisterQuestGiver(quest, questerType);
		}

		public static void Initialize()
		{
			if (AutoGenerateNew)
			{
				foreach (Quests quest in m_Quests.Values)
				{
					if (quest != null && !quest.Deserialized)
						quest.Generate();
				}
			}


			QuestPersistence.EnsureExistence();
			//Admin Commands
			CommandSystem.Register("QuestsInfo", AccessLevel.Administrator, new CommandEventHandler(QuestsInfo_OnCommand));
			CommandSystem.Register("SaveQuest", AccessLevel.Administrator, new CommandEventHandler(SaveQuest_OnCommand));
			CommandSystem.Register("SaveAllQuests", AccessLevel.Administrator, new CommandEventHandler(SaveAllQuests_OnCommand));
			CommandSystem.Register("InvalidQuestItems", AccessLevel.Administrator, new CommandEventHandler(InvalidQuestItems_OnCommand));
			CommandSystem.Register("QuestItem", AccessLevel.Player, new CommandEventHandler(ToggleQuestItem_OnCommand));

			TargetCommands.Register(new ViewQuestsCommand());
			TargetCommands.Register(new ViewContextCommand());
			
			EventSink.QuestGumpRequest += new QuestGumpRequestHandler(EventSink_QuestGumpRequest);
		}

		[Usage("QuestItem")]
		[Description("Toggles item for Quest usage")]
		public static void ToggleQuestItem_OnCommand(CommandEventArgs e)
		{
			var pm = e.Mobile as PlayerMobile;
			if (pm == null)
				return;
			if (!pm.CheckAlive())
				return;

			
			Server.Engines.Quests.Gumps.BaseQuestGump.CloseOtherGumps(pm);
			pm.CloseGump(typeof(Server.Engines.Quests.Gumps.QuestLogDetailedGump));
			pm.CloseGump(typeof(Server.Engines.Quests.Gumps.QuestLogGump));
			pm.CloseGump(typeof(Server.Engines.Quests.Gumps.NQuestOfferGump));
	
			pm.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(ToggleQuestItems));
			pm.SendLocalizedMessage(1072352); // Target the item you wish to toggle Quest Item status on <ESC> to cancel
		}
		public static void ToggleQuestItems(Mobile from, object obj)
		{
			var pm = from as PlayerMobile;
			if (pm == null)
				return;
		
			if (!from.CheckAlive())
				return;

			Item item = obj as Item;

			if (item == null)
				return;

			if (from.Backpack == null || item.Parent != from.Backpack)
			{
				from.SendLocalizedMessage(1074769); // An item must be in your backpack (and not in a container within) to be toggled as a quest item.
			}
			else if (item.QuestItem)
			{
				item.QuestItem = false;
				from.SendLocalizedMessage(1072354); // You remove Quest Item status from the item
			}
			else if (QuestSystem.MarkQuestItem(pm, item))
			{
				from.SendLocalizedMessage(1072353); // You set the item to Quest Item status
			}
			else
			{
				from.SendLocalizedMessage(1072355, "", 0x23); // That item does not match any of your quest criteria
			}

		}

		[Usage("QuestsInfo")]
		[Description("Displays general information about the quest system, or a quest by type name.")]
		public static void QuestsInfo_OnCommand(CommandEventArgs e)
		{
			Mobile m = e.Mobile;

			if (e.Length == 0)
			{
				m.SendMessage("Quest table length: {0}", m_Quests.Count);
				return;
			}

			Type index = ScriptCompiler.FindTypeByName(e.GetString(0));
			Quests quest;

			if (index == null || !m_Quests.TryGetValue(index, out quest))
			{
				m.SendMessage("Invalid quest type name.");
				return;
			}

			m.SendMessage("Activated: {0}", quest.Activated);
			m.SendMessage("Number of objectives: {0}", quest.Objectives.Count);
			m.SendMessage("Objective type: {0}", quest.ObjectiveType);
			m.SendMessage("Number of active instances: {0}", quest.Instances.Count);
		}

		public class ViewQuestsCommand : BaseCommand
		{
			public ViewQuestsCommand()
			{
				AccessLevel = AccessLevel.GameMaster;
				Supports = CommandSupport.Simple;
				Commands = new string[] { "ViewQuests" };
				ObjectTypes = ObjectTypes.Mobiles;
				Usage = "ViewQuests";
				Description = "Displays a targeted mobile's quest overview.";
			}

			public override void Execute(CommandEventArgs e, object obj)
			{
				Mobile from = e.Mobile;
				PlayerMobile pm = obj as PlayerMobile;

				if (pm == null)
				{
					LogFailure("That is not a player.");
					return;
				}

				CommandLogging.WriteLine(from, "{0} {1} viewing quest overview of {2}", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(pm));
				from.SendGump(new QuestLogGump(pm, false));
			}
		}

		private class ViewContextCommand : BaseCommand
		{
			public ViewContextCommand()
			{
				AccessLevel = AccessLevel.GameMaster;
				Supports = CommandSupport.Simple;
				Commands = new string[] { "ViewContext" };
				ObjectTypes = ObjectTypes.Mobiles;
				Usage = "ViewContext";
				Description = "Opens the ML quest context for a targeted mobile.";
			}

			public override void Execute(CommandEventArgs e, object obj)
			{
				PlayerMobile pm = obj as PlayerMobile;

				if (pm == null)
					LogFailure("They have no quest context.");
				else
					e.Mobile.SendGump(new PropertiesGump(e.Mobile, GetOrCreateContext(pm)));
			}
		}

		[Usage("SaveQuest <type> [saveEnabled=true]")]
		[Description("Allows serialization for a specific quest to be turned on or off.")]
		public static void SaveQuest_OnCommand(CommandEventArgs e)
		{
			Mobile m = e.Mobile;

			if (e.Length == 0 || e.Length > 2)
			{
				m.SendMessage("Syntax: SaveQuest <id> [saveEnabled=true]");
				return;
			}

			Type index = ScriptCompiler.FindTypeByName(e.GetString(0));
			Quests quest;

			if (index == null || !m_Quests.TryGetValue(index, out quest))
			{
				m.SendMessage("Invalid quest type name.");
				return;
			}

			bool enable = (e.Length == 2) ? e.GetBoolean(1) : true;

			quest.SaveEnabled = enable;
			m.SendMessage("Serialization for quest {0} is now {1}.", quest.GetType().Name, enable ? "enabled" : "disabled");

			if (AutoGenerateNew && !enable)
				m.SendMessage("Please note that automatic generation of new quests is ON. This quest will be regenerated on the next server start.");
		}

		[Usage("SaveAllQuests [saveEnabled=true]")]
		[Description("Allows serialization for all quests to be turned on or off.")]
		public static void SaveAllQuests_OnCommand(CommandEventArgs e)
		{
			Mobile m = e.Mobile;

			if (e.Length > 1)
			{
				m.SendMessage("Syntax: SaveAllQuests [saveEnabled=true]");
				return;
			}

			bool enable = (e.Length == 1) ? e.GetBoolean(0) : true;

			foreach (Quests quest in m_Quests.Values)
				quest.SaveEnabled = enable;

			m.SendMessage("Serialization for all quests is now {0}.", enable ? "enabled" : "disabled");

			if (AutoGenerateNew && !enable)
				m.SendMessage("Please note that automatic generation of new quests is ON. All quests will be regenerated on the next server start.");
		}

		[Usage("InvalidQuestItems")]
		[Description("Provides an overview of all quest items not located in the top-level of a player's backpack.")]
		public static void InvalidQuestItems_OnCommand(CommandEventArgs e)
		{
			Mobile m = e.Mobile;

			ArrayList found = new ArrayList();

			foreach (Item item in World.Items.Values)
			{
				if (item.QuestItem)
				{
					Backpack pack = item.Parent as Backpack;

					if (pack != null)
					{
						PlayerMobile player = pack.Parent as PlayerMobile;

						if (player != null && player.Backpack == pack)
							continue;
					}

					found.Add(item);
				}
			}

			if (found.Count == 0)
				m.SendMessage("No matching objects found.");
			else
				m.SendGump(new InterfaceGump(m, new string[] { "Object" }, found, 0, null));
		}

		private static bool FindQuest(IQuestGivers quester, PlayerMobile pm, QuestContext context, out Quests quest, out QuestInstance entry)
		{
			quest = null;
			entry = null;

			List<Quests> quests = quester.Quests;
			Type questerType = quester.GetType();

			// 1. Check quests in progress with this NPC (overriding deliveries is intended)
			if (context != null)
			{
				foreach (Quests questEntry in quests)
				{
					QuestInstance instance = context.FindInstance(questEntry);

					if (instance != null && (instance.Quester == quester || (!questEntry.IsEscort && instance.QuesterType == questerType)))
					{
						entry = instance;
						quest = questEntry;
						return true;
					}
				}
			}

			// 2. Check deliveries (overriding chain offers is intended)
			if ((entry = HandleDelivery(pm, quester, questerType)) != null)
			{
				quest = entry.Quest;
				return true;
			}

			// 3. Check chain quest offers
			if (context != null)
			{
				foreach (Quests questEntry in quests)
				{
					if (questEntry.IsChainTriggered && context.ChainOffers.Contains(questEntry))
					{
						quest = questEntry;
						return true;
					}
				}
			}

			// 4. Random quest
			quest = RandomStarterQuest(quester, pm, context);

			return (quest != null);
		}

		public static void OnDoubleClick(IQuestGivers quester, PlayerMobile pm) //Double Click to Start Quest
		{
			if (quester.Deleted || !pm.Alive)
				return;

			QuestContext context = GetContext(pm);

			Quests quest;
			QuestInstance entry;

			if (!FindQuest(quester, pm, context, out quest, out entry))
			{
				Tell(quester, pm, 1080107); // I'm sorry, I have nothing for you at this time.
				return;
			}

			if (entry != null)
			{
				TurnToFace(quester, pm);

				if (entry.Failed)
					return; // Note: OSI sends no gump at all for failed quests, they have to be cancelled in the quest overview
				else if (entry.ClaimReward)
					entry.SendRewardOffer();
				else if (entry.IsCompleted())
					entry.SendReportBackGump();
				else
					entry.SendProgressGump();
			}
			else if (quest.CanOffer(quester, pm, context, true))
			{
				TurnToFace(quester, pm);

				quest.SendOffer(quester, pm);
			}
		}

		public static bool CanMarkQuestItem(PlayerMobile pm, Item item, Type type)
		{
			QuestContext context = GetContext(pm);

			if (context != null)
			{
				foreach (QuestInstance quest in context.QuestInstances)
				{
					if (!quest.ClaimReward && quest.AllowsQuestItem(item, type))
						return true;
				}
			}

			return false;
		}

		private static void OnMarkQuestItem(PlayerMobile pm, Item item, Type type)
		{
			QuestContext context = GetContext(pm);

			if (context == null)
				return;

			List<QuestInstance> instances = context.QuestInstances;

			// We don't foreach because CheckComplete() can potentially modify the MLQuests list
			for (int i = instances.Count - 1; i >= 0; --i)
			{
				QuestInstance instance = instances[i];

				if (instance.ClaimReward)
					continue;

				foreach (BaseObjectiveInstance objective in instance.Objectives)
				{
					if (!objective.Expired && objective.AllowsQuestItem(item, type))
					{
						objective.CheckComplete(); // yes, this can happen multiple times (for multiple quests)
						break;
					}
				}
			}
		}

		public static bool MarkQuestItem(PlayerMobile pm, Item item)
		{
			Type type = item.GetType();

			if (CanMarkQuestItem(pm, item, type))
			{
				item.QuestItem = true;
				OnMarkQuestItem(pm, item, type);

				return true;
			}

			return false;
		}

		public static void HandleSkillGain(PlayerMobile pm, SkillName skill)
		{
			QuestContext context = GetContext(pm);

			if (context == null)
				return;

			List<QuestInstance> instances = context.QuestInstances;

			for (int i = instances.Count - 1; i >= 0; --i)
			{
				QuestInstance instance = instances[i];

				if (instance.ClaimReward)
					continue;

				foreach (BaseObjectiveInstance objective in instance.Objectives)
				{
					if (!objective.Expired && objective is GainSkillObjectiveInstance && ((GainSkillObjectiveInstance)objective).Handles(skill))
					{
						objective.CheckComplete();
						break;
					}
				}
			}
		}

		public static void HandleKill(PlayerMobile pm, Mobile mob)
		{
			QuestContext context = GetContext(pm);

			if (context == null)
				return;

			List<QuestInstance> instances = context.QuestInstances;

			Type type = null;

			for (int i = instances.Count - 1; i >= 0; --i)
			{
				QuestInstance instance = instances[i];

				if (instance.ClaimReward)
					continue;

				/* A kill only counts for a single objective within a quest,
				 * but it can count for multiple quests. This is something not
				 * currently observable on OSI, so it is assumed behavior.
				 */
				foreach (BaseObjectiveInstance objective in instance.Objectives)
				{
					if (!objective.Expired && objective is KillObjectiveInstance)
					{
						KillObjectiveInstance kill = (KillObjectiveInstance)objective;

						if (type == null)
							type = mob.GetType();

						if (kill.AddKill(mob, type))
						{
							kill.CheckComplete();
							break;
						}
					}
				}
			}
		}

		public static QuestInstance HandleDelivery(PlayerMobile pm, IQuestGivers quester, Type questerType)
		{
			QuestContext context = GetContext(pm);

			if (context == null)
				return null;

			List<QuestInstance> instances = context.QuestInstances;
			QuestInstance deliverInstance = null;

			for (int i = instances.Count - 1; i >= 0; --i)
			{
				QuestInstance instance = instances[i];

				// Do NOT skip quests on ClaimReward, because the quester still needs the quest ref!
				//if ( instance.ClaimReward )
				//	continue;

				foreach (BaseObjectiveInstance objective in instance.Objectives)
				{
					// Note: On OSI, expired deliveries can still be completed. Bug?
					if (!objective.Expired && objective is DeliverObjectiveInstance)
					{
						DeliverObjectiveInstance deliver = (DeliverObjectiveInstance)objective;

						if (deliver.IsDestination(quester, questerType))
						{
							if (!deliver.HasCompleted) // objective completes only once
							{
								deliver.HasCompleted = true;
								deliver.CheckComplete();

								// The quest is continued with this NPC (important for chains)
								instance.Quester = quester;
							}

							if (deliverInstance == null)
								deliverInstance = instance;

							break; // don't return, we may have to complete more deliveries
						}
					}
				}
			}

			return deliverInstance;
		}

		public static QuestContext GetContext(PlayerMobile pm)
		{
			QuestContext context;
			m_Contexts.TryGetValue(pm, out context);

			return context;
		}

		public static QuestContext GetOrCreateContext(PlayerMobile pm)
		{
			QuestContext context;

			if (!m_Contexts.TryGetValue(pm, out context))
				m_Contexts[pm] = context = new QuestContext(pm);

			return context;
		}

		public static void HandleDeath(PlayerMobile pm)
		{
			QuestContext context = GetContext(pm);

			if (context != null)
				context.HandleDeath();
		}

		public static void HandleDeletion(PlayerMobile pm)
		{
			QuestContext context = GetContext(pm);

			if (context != null)
			{
				context.HandleDeletion();
				m_Contexts.Remove(pm);
			}
		}

		public static void HandleDeletion(IQuestGivers quester)
		{
			foreach (Quests quest in quester.Quests)
			{
				List<QuestInstance> instances = quest.Instances;

				for (int i = instances.Count - 1; i >= 0; --i)
				{
					QuestInstance instance = instances[i];

					if (instance.Quester == quester)
						instance.OnQuesterDeleted();
				}
			}
		}

		public static void EventSink_QuestGumpRequest(QuestGumpRequestArgs args)
		{
			PlayerMobile pm = args.Mobile as PlayerMobile;

			pm.SendGump(new QuestLogGump(pm));
		}

		private static List<Quests> m_EligiblePool = new List<Quests>();

		public static Quests RandomStarterQuest(IQuestGivers quester, PlayerMobile pm, QuestContext context)
		{
			List<Quests> quests = quester.Quests;

			if (quests.Count == 0)
				return null;

			m_EligiblePool.Clear();
			Quests fallback = null;

			foreach (Quests quest in quests)
			{
				if (quest.IsChainTriggered || (context != null && context.IsDoingQuest(quest)))
					continue;

				/*
				 * Save first quest that reaches the CanOffer call.
				 * If no quests are valid at all, return this quest for displaying the CanOffer error message.
				 */
				if (fallback == null)
					fallback = quest;

				if (quest.CanOffer(quester, pm, context, false))
					m_EligiblePool.Add(quest);
			}

			if (m_EligiblePool.Count == 0)
				return fallback;

			return m_EligiblePool[Utility.Random(m_EligiblePool.Count)];
		}

		public static void TurnToFace(IQuestGivers quester, Mobile mob)
		{
			if (quester is Mobile)
			{
				Mobile m = (Mobile)quester;
				m.Direction = m.GetDirectionTo(mob);
			}
		}

		public static void Tell(IQuestGivers quester, PlayerMobile pm, int cliloc)
		{
			TurnToFace(quester, pm);

			if (quester is Mobile)
				((Mobile)quester).PrivateOverheadMessage(MessageType.Regular, SpeechColorDefault, cliloc, pm.NetState);
			else if (quester is Item)
				MessageHelper.SendLocalizedMessageTo((Item)quester, pm, cliloc, SpeechColorDefault);
			else
				pm.SendLocalizedMessage(cliloc, "", SpeechColorDefault);
		}

		public static void Tell(IQuestGivers quester, PlayerMobile pm, int cliloc, string args)
		{
			TurnToFace(quester, pm);

			if (quester is Mobile)
				((Mobile)quester).PrivateOverheadMessage(MessageType.Regular, SpeechColorDefault, cliloc, args, pm.NetState);
			else if (quester is Item)
				MessageHelper.SendLocalizedMessageTo((Item)quester, pm, cliloc, args, SpeechColorDefault);
			else
				pm.SendLocalizedMessage(cliloc, args, SpeechColorDefault);
		}

		public static void Tell(IQuestGivers quester, PlayerMobile pm, string message)
		{
			TurnToFace(quester, pm);

			if (quester is Mobile)
				((Mobile)quester).PrivateOverheadMessage(MessageType.Regular, SpeechColorDefault, false, message, pm.NetState);
			else if (quester is Item)
				MessageHelper.SendMessageTo((Item)quester, pm, message, SpeechColorDefault);
			else
				pm.SendMessage(SpeechColorDefault, message);
		}

		public static void TellDef(IQuestGivers quester, PlayerMobile pm, TextDefinition def)
		{
			if (def == null)
				return;

			if (def.Number > 0)
				Tell(quester, pm, def.Number);
			else if (def.String != null)
				Tell(quester, pm, def.String);
		}

		public static void WriteQuestRef(GenericWriter writer, Quests quest)
		{
			writer.Write((quest != null && quest.SaveEnabled) ? quest.GetType().FullName : null);
		}

		public static Quests ReadQuestRef(GenericReader reader)
		{
			string typeName = reader.ReadString();

			if (typeName == null)
				return null; // not serialized

			Type questType = ScriptCompiler.FindTypeByFullName(typeName);

			if (questType == null)
				return null; // no longer a type

			return FindQuest(questType);
		}

		public static Quests FindQuest(Type questType)
		{
			Quests result;
			m_Quests.TryGetValue(questType, out result);

			return result;
		}

		public static List<Quests> FindQuestList(Type questerType)
		{
			List<Quests> result;

			if (m_QuestGivers.TryGetValue(questerType, out result))
				return result;

			return EmptyList;
		}

		#region OldCode
		public abstract object Name{ get; }
		public abstract object OfferMessage{ get; }

		public abstract int Picture{ get; }

		public abstract bool IsTutorial{ get; }
		public abstract TimeSpan RestartDelay{ get; }

		public abstract Type[] TypeReferenceTable{ get; }

		private PlayerMobile m_From;
		private ArrayList m_Objectives;
		private ArrayList m_Conversations;

		public PlayerMobile From
		{
			get{ return m_From; }
			set{ m_From = value; }
		}

		public ArrayList Objectives
		{
			get{ return m_Objectives; }
			set{ m_Objectives = value; }
		}

		public ArrayList Conversations
		{
			get{ return m_Conversations; }
			set{ m_Conversations = value; }
		}

		private Timer m_Timer;

		public virtual void StartTimer()
		{
			if ( m_Timer != null )
				return;

			m_Timer = Timer.DelayCall( TimeSpan.FromSeconds( 0.5 ), TimeSpan.FromSeconds( 0.5 ), new TimerCallback( Slice ) );
		}

		public virtual void StopTimer()
		{
			if ( m_Timer != null )
				m_Timer.Stop();

			m_Timer = null;
		}

		public virtual void Slice()
		{
			for ( int i = m_Objectives.Count - 1; i >= 0; --i )
			{
				QuestObjective obj = (QuestObjective)m_Objectives[i];

				if ( obj.GetTimerEvent() )
					obj.CheckProgress();
			}
		}

		public virtual void OnKill( BaseCreature creature, Container corpse )
		{
			for ( int i = m_Objectives.Count - 1; i >= 0; --i )
			{
				QuestObjective obj = (QuestObjective)m_Objectives[i];

				if ( obj.GetKillEvent( creature, corpse ) )
					obj.OnKill( creature, corpse );
			}
		}

		public virtual bool IgnoreYoungProtection( Mobile from )
		{
			for ( int i = m_Objectives.Count - 1; i >= 0; --i )
			{
				QuestObjective obj = (QuestObjective)m_Objectives[i];

				if ( obj.IgnoreYoungProtection( from ) )
					return true;
			}

			return false;
		}

		public QuestSystem( PlayerMobile from )
		{
			m_From = from;
			m_Objectives = new ArrayList();
			m_Conversations = new ArrayList();
		}

		public QuestSystem()
		{
		}

		public virtual void BaseDeserialize( GenericReader reader )
		{
			Type[] referenceTable = this.TypeReferenceTable;

			int version = reader.ReadEncodedInt();

			switch ( version )
			{
				case 0:
				{
					int count = reader.ReadEncodedInt();

					m_Objectives = new ArrayList( count );

					for ( int i = 0; i < count; ++i )
					{
						QuestObjective obj = QuestSerializer.DeserializeObjective( referenceTable, reader );

						if ( obj != null )
						{
							obj.System = this;
							m_Objectives.Add( obj );
						}
					}

					count = reader.ReadEncodedInt();

					m_Conversations = new ArrayList( count );

					for ( int i = 0; i < count; ++i )
					{
						QuestConversation conv = QuestSerializer.DeserializeConversation( referenceTable, reader );

						if ( conv != null )
						{
							conv.System = this;
							m_Conversations.Add( conv );
						}
					}

					break;
				}
			}

			ChildDeserialize( reader );
		}

		public virtual void ChildDeserialize( GenericReader reader )
		{
			int version = reader.ReadEncodedInt();
		}

		public virtual void BaseSerialize( GenericWriter writer )
		{
			Type[] referenceTable = this.TypeReferenceTable;

			writer.WriteEncodedInt( (int) 0 ); // version

			writer.WriteEncodedInt( (int) m_Objectives.Count );

			for ( int i = 0; i < m_Objectives.Count; ++i )
				QuestSerializer.Serialize( referenceTable, (QuestObjective) m_Objectives[i], writer );

			writer.WriteEncodedInt( (int) m_Conversations.Count );

			for ( int i = 0; i < m_Conversations.Count; ++i )
				QuestSerializer.Serialize( referenceTable, (QuestConversation) m_Conversations[i], writer );

			ChildSerialize( writer );
		}

		public virtual void ChildSerialize( GenericWriter writer )
		{
			writer.WriteEncodedInt( (int) 0 ); // version
		}

		public bool IsObjectiveInProgress( Type type )
		{
			QuestObjective obj = FindObjective( type );

			return ( obj != null && !obj.Completed );
		}

		public QuestObjective FindObjective( Type type )
		{
			for ( int i = m_Objectives.Count - 1; i >= 0; --i )
			{
				QuestObjective obj = (QuestObjective)m_Objectives[i];

				if ( obj.GetType() == type )
					return obj;
			}

			return null;
		}

		public virtual void SendOffer()
		{
			m_From.SendGump( new QuestOfferGump( this ) );
		}

		public virtual void GetContextMenuEntries( List<ContextMenuEntry> list )
		{
			if ( m_Objectives.Count > 0 )
				list.Add( new QuestCallbackEntry( 6154, new QuestCallback( ShowQuestLog ) ) ); // View Quest Log

			if ( m_Conversations.Count > 0 )
				list.Add( new QuestCallbackEntry( 6156, new QuestCallback( ShowQuestConversation ) ) ); // Quest Conversation

			list.Add( new QuestCallbackEntry( 6155, new QuestCallback( BeginCancelQuest ) ) ); // Cancel Quest
		}

		public virtual void ShowQuestLogUpdated()
		{
			m_From.CloseGump( typeof( QuestLogUpdatedGump ) );
			m_From.SendGump( new QuestLogUpdatedGump( this ) );
		}

		public virtual void ShowQuestLog()
		{
			if ( m_Objectives.Count > 0 )
			{
				m_From.CloseGump( typeof( QuestItemInfoGump ) );
				m_From.CloseGump( typeof( QuestLogUpdatedGump ) );
				m_From.CloseGump( typeof( QuestObjectivesGump ) );
				m_From.CloseGump( typeof( QuestConversationsGump ) );

				m_From.SendGump( new QuestObjectivesGump( m_Objectives ) );

				QuestObjective last = (QuestObjective)m_Objectives[m_Objectives.Count - 1];

				if ( last.Info != null )
					m_From.SendGump( new QuestItemInfoGump( last.Info ) );
			}
		}

		public virtual void ShowQuestConversation()
		{
			if ( m_Conversations.Count > 0 )
			{
				m_From.CloseGump( typeof( QuestItemInfoGump ) );
				m_From.CloseGump( typeof( QuestObjectivesGump ) );
				m_From.CloseGump( typeof( QuestConversationsGump ) );

				m_From.SendGump( new QuestConversationsGump( m_Conversations ) );

				QuestConversation last = (QuestConversation)m_Conversations[m_Conversations.Count - 1];

				if ( last.Info != null )
					m_From.SendGump( new QuestItemInfoGump( last.Info ) );
			}
		}

		public virtual void BeginCancelQuest()
		{
			m_From.SendGump( new QuestCancelGump( this ) );
		}

		public virtual void EndCancelQuest( bool shouldCancel )
		{
			if ( m_From.Quest != this )
				return;

			if ( shouldCancel )
			{
				m_From.SendLocalizedMessage( 1049015 ); // You have canceled your quest.
				Cancel();
			}
			else
			{
				m_From.SendLocalizedMessage( 1049014 ); // You have chosen not to cancel your quest.
			}
		}

		public virtual void Cancel()
		{
			ClearQuest( false );
		}

		public virtual void Complete()
		{
			ClearQuest( true );
		}

		public virtual void ClearQuest( bool completed )
		{
			StopTimer();

			if ( m_From.Quest == this )
			{
				m_From.Quest = null;

				TimeSpan restartDelay = this.RestartDelay;

				if ( ( completed && restartDelay > TimeSpan.Zero ) || ( !completed && restartDelay == TimeSpan.MaxValue ) )
				{
					List<QuestRestartInfo> doneQuests = m_From.DoneQuests;

					if ( doneQuests == null )
						m_From.DoneQuests = doneQuests = new List<QuestRestartInfo>();

					bool found = false;

					Type ourQuestType = this.GetType();

					for ( int i = 0; i < doneQuests.Count; ++i )
					{
						QuestRestartInfo restartInfo = doneQuests[i];

						if ( restartInfo.QuestType == ourQuestType )
						{
							restartInfo.Reset( restartDelay );
							found = true;
							break;
						}
					}

					if ( !found )
						doneQuests.Add( new QuestRestartInfo( ourQuestType, restartDelay ) );
				}
			}
		}

		public virtual void AddConversation( QuestConversation conv )
		{
			conv.System = this;

			if ( conv.Logged )
				m_Conversations.Add( conv );

			m_From.CloseGump( typeof( QuestItemInfoGump ) );
			m_From.CloseGump( typeof( QuestObjectivesGump ) );
			m_From.CloseGump( typeof( QuestConversationsGump ) );

			if ( conv.Logged )
				m_From.SendGump( new QuestConversationsGump( m_Conversations ) );
			else
				m_From.SendGump( new QuestConversationsGump( conv ) );

			if ( conv.Info != null )
				m_From.SendGump( new QuestItemInfoGump( conv.Info ) );
		}

		public virtual void AddObjective( QuestObjective obj )
		{
			obj.System = this;
			m_Objectives.Add( obj );

			ShowQuestLogUpdated();
		}

		public virtual void Accept()
		{
			if ( m_From.Quest != null )
				return;

			m_From.Quest = this;
			m_From.SendLocalizedMessage( 1049019 ); // You have accepted the Quest.

			StartTimer();
		}

		public virtual void Decline()
		{
			m_From.SendLocalizedMessage( 1049018 ); // You have declined the Quest.
		}

		public static bool CanOfferQuest( Mobile check, Type questType )
		{
			bool inRestartPeriod;

			return CanOfferQuest( check, questType, out inRestartPeriod );
		}

		public static bool CanOfferQuest( Mobile check, Type questType, out bool inRestartPeriod )
		{
			inRestartPeriod = false;

			PlayerMobile pm = check as PlayerMobile;

			if ( pm == null )
				return false;

			if ( pm.HasGump( typeof( QuestOfferGump ) ) )
				return false;

			if ( questType == typeof( Necro.DarkTidesQuest ) && pm.Profession != 4 ) // necromancer
				return false;

			if ( questType == typeof( Haven.UzeraanTurmoilQuest ) && pm.Profession != 1 && pm.Profession != 2 && pm.Profession != 5 ) // warrior / magician / paladin
				return false;

			if ( questType == typeof( Samurai.HaochisTrialsQuest ) && pm.Profession != 6 ) // samurai
				return false;

			if ( questType == typeof( Ninja.EminosUndertakingQuest ) && pm.Profession != 7 ) // ninja
				return false;

			List<QuestRestartInfo> doneQuests = pm.DoneQuests;

			if ( doneQuests != null )
			{
				for ( int i = 0; i < doneQuests.Count; ++i )
				{
					QuestRestartInfo restartInfo = doneQuests[i];

					if ( restartInfo.QuestType == questType )
					{
						DateTime endTime = restartInfo.RestartTime;

						if ( DateTime.UtcNow < endTime )
						{
							inRestartPeriod = true;
							return false;
						}

						doneQuests.RemoveAt( i-- );
						return true;
					}
				}
			}

			return true;
		}

		public static void FocusTo( Mobile who, Mobile to )
		{
			if ( Utility.RandomBool() )
			{
				who.Animate( 17, 7, 1, true, false, 0 );
			}
			else
			{
				switch ( Utility.Random( 3 ) )
				{
					case 0: who.Animate( 32, 7, 1, true, false, 0 ); break;
					case 1: who.Animate( 33, 7, 1, true, false, 0 ); break;
					case 2: who.Animate( 34, 7, 1, true, false, 0 ); break;
				}
			}

			who.Direction = who.GetDirectionTo( to );
		}

		public static int RandomBrightHue()
		{
			if ( 0.1 > Utility.RandomDouble() )
				return Utility.RandomList( 0x62, 0x71 );

			return Utility.RandomList( 0x03, 0x0D, 0x13, 0x1C, 0x21, 0x30, 0x37, 0x3A, 0x44, 0x59 );
		}
	}

	public class QuestCancelGump : BaseQuestGump
	{
		private QuestSystem m_System;

		public QuestCancelGump( QuestSystem system ) : base( 120, 50 )
		{
			m_System = system;

			Closable = false;

			AddPage( 0 );

			AddImageTiled( 0, 0, 348, 262, 2702 );
			AddAlphaRegion( 0, 0, 348, 262 );

			AddImage( 0, 15, 10152 );
			AddImageTiled( 0, 30, 17, 200, 10151 );
			AddImage( 0, 230, 10154 );

			AddImage( 15, 0, 10252 );
			AddImageTiled( 30, 0, 300, 17, 10250 );
			AddImage( 315, 0, 10254 );

			AddImage( 15, 244, 10252 );
			AddImageTiled( 30, 244, 300, 17, 10250 );
			AddImage( 315, 244, 10254 );

			AddImage( 330, 15, 10152 );
			AddImageTiled( 330, 30, 17, 200, 10151 );
			AddImage( 330, 230, 10154 );

			AddImage( 333, 2, 10006 );
			AddImage( 333, 248, 10006 );
			AddImage( 2, 248, 10006 );
			AddImage( 2, 2, 10006 );

			AddHtmlLocalized( 25, 22, 200, 20, 1049000, 32000, false, false ); // Confirm Quest Cancellation
			AddImage( 25, 40, 3007 );

			if ( system.IsTutorial )
			{
				AddHtmlLocalized( 25, 55, 300, 120, 1060836, White, false, false ); // This quest will give you valuable information, skills and equipment that will help you advance in the game at a quicker pace.<BR><BR>Are you certain you wish to cancel at this time?
			}
			else
			{
				AddHtmlLocalized( 25, 60, 300, 20, 1049001, White, false, false ); // You have chosen to abort your quest:
				AddImage( 25, 81, 0x25E7 );
				AddHtmlObject( 48, 80, 280, 20, system.Name, DarkGreen, false, false );

				AddHtmlLocalized( 25, 120, 280, 20, 1049002, White, false, false ); // Can this quest be restarted after quitting?
				AddImage( 25, 141, 0x25E7 );
				AddHtmlLocalized( 48, 140, 280, 20, (system.RestartDelay < TimeSpan.MaxValue) ? 1049016 : 1049017, DarkGreen, false, false ); // Yes/No
			}

			AddRadio( 25, 175, 9720, 9723, true, 1 );
			AddHtmlLocalized( 60, 180, 280, 20, 1049005, White, false, false ); // Yes, I really want to quit!

			AddRadio( 25, 210, 9720, 9723, false, 0 );
			AddHtmlLocalized( 60, 215, 280, 20, 1049006, White, false, false ); // No, I don't want to quit.

			AddButton( 265, 220, 247, 248, 1, GumpButtonType.Reply, 0 );
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			if ( info.ButtonID == 1 )
				m_System.EndCancelQuest( info.IsSwitched( 1 ) );
		}
	}

	public class QuestOfferGump : BaseQuestGump
	{
		private QuestSystem m_System;

		public QuestOfferGump( QuestSystem system ) : base( 75, 25 )
		{
			m_System = system;

			Closable = false;

			AddPage( 0 );

			AddImageTiled( 50, 20, 400, 400, 2624 );
			AddAlphaRegion( 50, 20, 400, 400 );

			AddImage( 90, 33, 9005 );
			AddHtmlLocalized( 130, 45, 270, 20, 1049010, White, false, false ); // Quest Offer
			AddImageTiled( 130, 65, 175, 1, 9101 );

			AddImage( 140, 110, 1209 );
			AddHtmlObject( 160, 108, 250, 20, system.Name, DarkGreen, false, false );

			AddHtmlObject( 98, 140, 312, 200, system.OfferMessage, LightGreen, false, true );

			AddRadio( 85, 350, 9720, 9723, true, 1 );
			AddHtmlLocalized( 120, 356, 280, 20, 1049011, White, false, false ); // I accept!

			AddRadio( 85, 385, 9720, 9723, false, 0 );
			AddHtmlLocalized( 120, 391, 280, 20, 1049012, White, false, false ); // No thanks, I decline.

			AddButton( 340, 390, 247, 248, 1, GumpButtonType.Reply, 0 );

			AddImageTiled( 50, 29, 30, 390, 10460 );
			AddImageTiled( 34, 140, 17, 279, 9263 );

			AddImage( 48, 135, 10411 );
			AddImage( -16, 285, 10402 );
			AddImage( 0, 10, 10421 );
			AddImage( 25, 0, 10420 );

			AddImageTiled( 83, 15, 350, 15, 10250 );

			AddImage( 34, 419, 10306 );
			AddImage( 442, 419, 10304 );
			AddImageTiled( 51, 419, 392, 17, 10101 );

			AddImageTiled( 415, 29, 44, 390, 2605 );
			AddImageTiled( 415, 29, 30, 390, 10460 );
			AddImage( 425, 0, 10441 );

			AddImage( 370, 50, 1417 );
			AddImage( 379, 60, system.Picture );
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			if ( info.ButtonID == 1 )
			{
				if ( info.IsSwitched( 1 ) )
					m_System.Accept();
				else
					m_System.Decline();
			}
		}
	}

	public abstract class BaseQuestGump : Gump
	{
		public const int Black = 0x0000;
		public const int White = 0x7FFF;
		public const int DarkGreen = 10000;
		public const int LightGreen = 90000;
		public const int Blue = 19777215;

		public static int C16232( int c16 )
		{
			c16 &= 0x7FFF;

			int r = ( ((c16 >> 10) & 0x1F) << 3 );
			int g = ( ((c16 >> 05) & 0x1F) << 3 );
			int b = ( ((c16 >> 00) & 0x1F) << 3 );

			return (r << 16) | (g << 8) | (b << 0);
		}

		public static int C16216( int c16 )
		{
			return c16 & 0x7FFF;
		}

		public static int C32216( int c32 )
		{
			c32 &= 0xFFFFFF;

			int r = ( ((c32 >> 16) & 0xFF) >> 3 );
			int g = ( ((c32 >> 08) & 0xFF) >> 3 );
			int b = ( ((c32 >> 00) & 0xFF) >> 3 );

			return (r << 10) | (g << 5) | (b << 0);
		}

		public static string Color( string text, int color )
		{
			return String.Format( "<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", color, text );
		}

		public static ArrayList BuildList( object obj )
		{
			ArrayList list = new ArrayList();

			list.Add( obj );

			return list;
		}

		public void AddHtmlObject( int x, int y, int width, int height, object message, int color, bool back, bool scroll )
		{
			if ( message is string )
			{
				string html = (string)message;

				AddHtml( x, y, width, height, Color( html, C16232( color ) ), back, scroll );
			}
			else if ( message is int )
			{
				int html = (int)message;

				AddHtmlLocalized( x, y, width, height, html, C16216( color ), back, scroll );
			}
		}

		public BaseQuestGump( int x, int y ) : base( x, y )
		{
		}
	}
#endregion
}
