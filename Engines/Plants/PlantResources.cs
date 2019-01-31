using System;
using Server;
using Server.Items;

namespace Server.Engines.Plants
{
	public class PlantResourceInfo
	{
		private static PlantResourceInfo[] m_ResourceList = new PlantResourceInfo[] //Added Cocoa Tree and below
			{
			new PlantResourceInfo( PlantType.ElephantEarPlant, PlantHue.BrightRed, typeof( RedLeaves ) ),
			new PlantResourceInfo( PlantType.PonytailPalm, PlantHue.BrightRed, typeof( RedLeaves ) ),
			new PlantResourceInfo( PlantType.CenturyPlant, PlantHue.BrightRed, typeof( RedLeaves ) ),
			new PlantResourceInfo( PlantType.Poppies, PlantHue.BrightOrange, typeof( OrangePetals ) ),
			new PlantResourceInfo( PlantType.Bulrushes, PlantHue.BrightOrange, typeof( OrangePetals ) ),
			new PlantResourceInfo( PlantType.PampasGrass, PlantHue.BrightOrange, typeof( OrangePetals ) ),
			new PlantResourceInfo( PlantType.SnakePlant, PlantHue.BrightGreen, typeof( GreenThorns ) ),
			new PlantResourceInfo( PlantType.BarrelCactus, PlantHue.BrightGreen, typeof( GreenThorns ) ),
			new PlantResourceInfo( PlantType.CocoaTree, PlantHue.Plain, typeof(CocoaPulp)), //Cocoapulp Resource
			new PlantResourceInfo( PlantType.SugarCanes, PlantHue.Plain, typeof(SackOfSugar)), //Sugar Resource
			new PlantResourceInfo( PlantType.FlaxFlowers, PlantHue.Plain, typeof(Cotton)), //Cotton Resource
			new PlantResourceInfo( PlantType.CypressStraight, PlantHue.Plain, typeof(BarkFragment)), //Bark Fragments Resource
			new PlantResourceInfo( PlantType.CypressTwisted, PlantHue.Plain, typeof(BarkFragment)), //Bark Fragments Resource 
			new PlantResourceInfo( PlantType.Vanilla, PlantHue.Plain, typeof(Vanilla)), //Vanilla Resource
			new PlantResourceInfo( PlantType.SnakePlant, PlantHue.FireRed, typeof( DragonFruit )), //Dragonfruit Resource from Fire Red SnakePlant
		//	new PlantResourceInfo( PlantType.HopsSouth, PlantHue.Plain, typeof(Hops)), //Hops Resource
		//new PlantResourceInfo( PlantType.HopsEast, PlantHue.Plain, typeof(Hops)), //Hops Resource
			new PlantResourceInfo( PlantType.Tea, PlantHue.Plain, typeof(GreenTeaBasket)), //Green Tea anyone?
			};

		public static PlantResourceInfo GetInfo( PlantType plantType, PlantHue plantHue )
		{
			foreach ( PlantResourceInfo info in m_ResourceList )
			{
				if ( info.PlantType == plantType && info.PlantHue == plantHue )
					return info;
			}

			return null;
		}

		private PlantType m_PlantType;
		private PlantHue m_PlantHue;
		private Type m_ResourceType;

		public PlantType PlantType { get { return m_PlantType; } }
		public PlantHue PlantHue { get { return m_PlantHue; } }
		public Type ResourceType { get { return m_ResourceType; } }

		private PlantResourceInfo( PlantType plantType, PlantHue plantHue, Type resourceType )
		{
			m_PlantType = plantType;
			m_PlantHue = plantHue;
			m_ResourceType = resourceType;
		}

		public Item CreateResource()
		{
			return (Item)Activator.CreateInstance( m_ResourceType );
		}
	}
}