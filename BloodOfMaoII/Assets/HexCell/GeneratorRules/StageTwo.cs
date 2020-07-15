using System.Collections.Generic;
using UnityEngine;
using static AtomosZ.BoMII.Terrain.TileDefinitions;

namespace AtomosZ.BoMII.Terrain.Generation
{
	public static class StageTwo
	{
		public static HexMapGenerator mapGen;
		private static StageTwoRules rules;
		public static List<TerrainTile> tiles;


		public static void RunGeneration(HexMapGenerator hexMapGenerator,
			Dictionary<TerrainType, List<Region>> regionDict)
		{
			mapGen = hexMapGenerator;
			rules = mapGen.stageTwo;

			//FillLandRegionsWithNoiseTerrain(regionDict[TerrainType.LandGenerator]);
			tiles = FillLandRegionsWithRandomTerrain(regionDict[TerrainType.LandGenerator]);

			if (!RunSimulation(tiles))
				Debug.Log("Simulation stable!");
		}


		public static void NextGeneration()
		{
			if (!RunSimulation(tiles))
				Debug.Log("Simulation stable!");
		}

		private static bool RunSimulation(List<TerrainTile> tiles)
		{
			Dictionary<TerrainTile, TerrainType> changesToMake =
				new Dictionary<TerrainTile, TerrainType>();

			foreach (TerrainTile tile in tiles)
			{
				Dictionary<TerrainType, int> neighbouringTerrainCount = new Dictionary<TerrainType, int>();
				TerrainTile[] neighbours = mapGen.GetSurroundingTiles(tile.coordinates);
				foreach (TerrainTile neighbour in neighbours)
				{
					TerrainType terrainToCheck;
					if (neighbour == null)
						terrainToCheck = tile.terrainType;
					else
						terrainToCheck = neighbour.terrainType;
					if (neighbouringTerrainCount.TryGetValue(terrainToCheck, out int count))
						++neighbouringTerrainCount[terrainToCheck];
					else
						neighbouringTerrainCount[terrainToCheck] = 1;
				}

				if (TileTransmogrifier(tile, neighbouringTerrainCount, out TerrainType newType))
					changesToMake[tile] = newType;
			}

			foreach (var change in changesToMake)
			{
				mapGen.CreateAndSetTile(change.Key.coordinates, mapGen.GetTerrainData(change.Value).tile, change.Key);
			}

			return changesToMake.Count > 0;
		}


		private static bool TileTransmogrifier(TerrainTile tile,
			Dictionary<TerrainType, int> neighbouringTerrainCount, out TerrainType newType)
		{
			Dictionary<TerrainType, TerrainData> terrainData = mapGen.GetTerrainData();

			TerrainType currentType = tile.terrainType;
			TerrainData currentData = terrainData[currentType];

			if (GetCount(neighbouringTerrainCount, currentType) >= currentData.stableMinNeighbours)
			{ // if this has enough similar neighbours let it live another day!
				newType = tile.terrainType;
				return false;
			}

			foreach (var terrainKVP in terrainData)
			{
				if (terrainKVP.Key == currentType) // already checked
					continue;

				if (terrainKVP.Key == TerrainType.WaterGenerator)
				{	// this will probably be a special case in some instances
					continue;
				}

				if (GetCount(neighbouringTerrainCount, terrainKVP.Key) > terrainKVP.Value.stableMinNeighbours)
				{
					newType = terrainKVP.Key;
					return true;
				}
			}

			newType = tile.terrainType;
			return false;
		}


		private static int GetCount(Dictionary<TerrainType, int> neighbouringTerrainCount, TerrainType terrainType)
		{
			if (!neighbouringTerrainCount.TryGetValue(terrainType, out int count))
				count = 0;
			return count;
		}


		private static List<TerrainTile> FillLandRegionsWithRandomTerrain(List<Region> regionList)
		{
			List<TerrainTile> tiles = new List<TerrainTile>();
			System.Random rng = new System.Random(mapGen.noiseSettings.GetSeed());

			for (int i = 0; i < regionList.Count; ++i)
			{
				for (int j = 0; j < regionList[i].regionSize; ++j)
				{
					Vector3Int tileCoord = regionList[i].tileCoords[j];
					int rnd = rng.Next(0, rules.tiles.Count);
					tiles.Add(mapGen.CreateAndSetTile(tileCoord, rules.tiles[rnd]));
				}
			}

			return tiles;
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