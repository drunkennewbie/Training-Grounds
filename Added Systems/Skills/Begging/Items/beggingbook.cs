using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Items
{
	#region A Grammar of Orcish
	public class BookofBeggers : BaseBook
	{
		public static readonly BookContent Content = new BookContent
			(
				"Welcome to our Organization", "Bharat",
				new BookPageInfo
				(
					"This is for the new beggars",
					"of the beggar organization.",
					"We are are considered the",
					"lowest of the common,",
					"well ok, maybe not as low",
					"as thieves.",
					"We are out to look poor",
					"gather information and"
				),
				new BookPageInfo
				(
					"items to better each",
					"other and the world.",
					"",
					"Yes, our purpose is to",
					"better each other and",
					"be a information network",
					"for those who pay enough.",
					"We do work with the thieves"
				),
				new BookPageInfo
				(
					"guild, on ensuring",
					"items and information is",
					"gathered and distrubed",
					"properly.",
					"",
					"Being a member of our",
					"organization has benefits",
					"we will provide you with"
				),
				new BookPageInfo
				(
					"tools to assist you in",
					"begging and gathering",
					"intelligence.",
					"We are not thieves,",
					"murderers, or even",
					"fighters.",
					"We will assist them all",
					"to better ourselves,"
				),
				new BookPageInfo
				(
					"and the greater good.",
					"All information passes",
					"though us and is spread",
					"throughout",
					"",
					"Remember that.",
					"",
					"Questions? ask."
				)
			
			);

		public override BookContent DefaultContent { get { return Content; } }

		[Constructable]
		public BookofBeggers() : base(Utility.Random(0xFEF, 2), false)
		{
		}

		public BookofBeggers(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadEncodedInt();
		}
	}
	#endregion
}
