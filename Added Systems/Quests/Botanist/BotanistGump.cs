using System;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Items;
using Server.Mobiles;
using Server.Commands;

namespace Server.Gumps
{
	public class BotanistGump : Gump
	{
		public static void Initialize()
		{
			CommandSystem.Register("BotanistGump", AccessLevel.GameMaster, BotanistGump_OnCommand);
		}

		private static void BotanistGump_OnCommand(CommandEventArgs e)
		{
			e.Mobile.SendGump(new BotanistGump(e.Mobile));
		}

		public BotanistGump(Mobile owner) : base(50, 50)
		{
			//----------------------------------------------------------------------------------------------------

			AddPage(0);
			AddImageTiled(54, 33, 369, 400, 2604); //Background?
			AddAlphaRegion(54, 33, 369, 400);

			AddImageTiled(416, 39, 44, 389, 203);
			//--------------------------------------Window size bar--------------------------------------------

			AddImage(97, 49, 9005); //Silver Ribbon Seal
			AddImageTiled(58, 39, 29, 390, 10460); //Top Left Seal
			AddImageTiled(412, 37, 31, 389, 10460); //Top Right Seal
			AddLabel(140, 60, 0x34, "Botanist"); //Title


			AddHtml(107, 140, 300, 230, "<BODY>" +
										//----------------------/----------------------------------------------/
										"<BASEFONT COLOR=YELLOW><I>Penelope works on her plants then stops to look up at you.</I><BR>Hi there stranger!<BR>" +
										"<BASEFONT COLOR=YELLOW>I found after years of research that fertile dirt and worms are great for growing plants, could you do me a favor and get some?<BR><BR>" +
										"<BASEFONT COLOR=YELLOW>I hear you can get worms from zombies and earth elementals are said to have the best dirt.<BR><BR><I>Jemma hands you an empty jar</I><BR><BR>" + "</BODY>", false, true);

			AddImage(430, 9, 10441); //Right Dragon
			AddImageTiled(40, 38, 17, 391, 9263); //Right Bar
			AddImage(6, 25, 10421); //Left Dragon Body
			AddImage(34, 12, 10420); //TopLeft Dragon Head
			AddImageTiled(94, 25, 342, 15, 10304); //Bottom Bar
			AddImageTiled(40, 427, 415, 16, 10304); //Top Bar
			AddImage(-10, 314, 10402); //Bottom Left Tail
			AddImage(56, 150, 10411); //Right Body
			AddImage(155, 120, 2103); //Blue Button
			AddImage(136, 84, 96);

			AddButton(225, 390, 0xF7, 0xF8, 0, GumpButtonType.Reply, 0);

			//--------------------------------------------------------------------------------------------------------------
		}

		public override void OnResponse(NetState state, RelayInfo info) //Function for GumpButtonType.Reply Buttons 
		{
			Mobile from = state.Mobile;

			switch (info.ButtonID)
			{
				case 0: //Case uses the ActionIDs defenied above. Case 0 defenies the actions for the button with the action id 0 
					{
						//Cancel 
						from.SendMessage("Get 10 Fertile Dirt and 5 Worms for Jemma");
						break;
					}

			}
		}
	}
}
