using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtomosZ.BoMII.Terrain.Generation
{
	[Serializable]
	public class MapGenerationRules
	{
		public int smoothSteps;

		public bool allowConnectRegions;
		[Tooltip("Size of passages that connect regions.")]
		public int passageSize = 1;

		public List<TerrainTile> tiles;
	}

	/// <summary>
	/// In which the stage is set: landmasses are formed and oceans are created.
	/// </summary>
	[Serializable]
	public class StageOneRules : MapGenerationRules
	{
		public bool useNoise;

		[Tooltip("Standard square Cells: Range(3,6)" +
		"\nA value of 4 is standard. A value of 5 with randomFillPercent" +
		"around 63 generates very eerie-looking platforms after 6 or 7 smooth steps. (try seed = Test Seed)" +
		"\nValues of 3 or 6 creates The Nothing." +
		"\nHex Cells: Range(2,5)\nAny value other than 3 is really unstable.")]
		[Range(2, 5)]
		public int minNeighboursToTurnToWater = 3;

		[Tooltip("Minimum neighbours 4:\n\t40 to 45: Large caverns.\n\t45 to 50: caves." +
			"\n\t50 to 55: small caves & rooms.\n\t55 to 60: small rooms." +
			"\n\tValues below 30 are too open and above 60 are to filled.")]
		[Range(10, 100)]
		public int randomFillPercent;
	}

	[Serializable]
	public class StageTwoRules : MapGenerationRules
	{

	}


	[Serializable]
	public class DungeonRules : MapGenerationRules
	{
		public TerrainTile openTile;
		public TerrainTile wallTile;
	}
}