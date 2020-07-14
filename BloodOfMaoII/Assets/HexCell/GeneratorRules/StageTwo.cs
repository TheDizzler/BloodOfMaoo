using System.Collections.Generic;
using UnityEngine;
using static AtomosZ.BoMII.Terrain.TileDefinitions;

namespace AtomosZ.BoMII.Terrain.Generation
{
	public static class StageTwo
	{
		public static HexMapGenerator mapGen;
		private static StageTwoRules rules;


		public static void RunGeneration(HexMapGenerator hexMapGenerator,
			Dictionary<TerrainType, List<Region>> regionDict)
		{
			mapGen = hexMapGenerator;
			rules = mapGen.stageTwo;

			//FillLandRegionsWithNoiseTerrain(regionDict[TerrainType.LandGenerator]);
			FillLandRegionsWithRandomTerrain(regionDict[TerrainType.LandGenerator]);
		}


		private static void FillLandRegionsWithRandomTerrain(List<Region> regionList)
		{
			System.Random rng = new System.Random(mapGen.noiseSettings.GetSeed());

			for (int i = 0; i < regionList.Count; ++i)
			{
				for (int j = 0; j < regionList[i].regionSize; ++j)
				{
					Vector3Int tileCoord = regionList[i].tileCoords[j];
					int rnd = rng.Next(0, rules.tiles.Count);
					mapGen.CreateAndSetTile(tileCoord, rules.tiles[rnd]);
				}
			}
		}


		/// <summary>
		/// TODO: everything
		/// </summary>
		/// <param name="regionList"></param>
		private static void FillLandRegionsWithNoiseTerrain(List<Region> regionList)
		{
			float[,] originalNoiseMap = StageOne.noiseMap;

			for (int i = 0; i < regionList.Count; ++i)
			{
				for (int j = 0; j < regionList[i].regionSize; ++j)
				{
					Vector3Int tileCoord = regionList[i].tileCoords[j];

				}
			}
		}
	}
}