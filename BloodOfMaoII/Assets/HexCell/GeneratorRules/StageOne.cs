using System.Collections.Generic;
using UnityEngine;
using static AtomosZ.BoMII.Terrain.TileDefinitions;

namespace AtomosZ.BoMII.Terrain.Generation
{
	public static class StageOne
	{
		public static HexMapGenerator mapGen;
		private static StageOneRules rules;


		public static void RunGeneration(HexMapGenerator hexMapGenerator)
		{
			mapGen = hexMapGenerator;
			rules = mapGen.stageOne;

			if (rules.useNoise)
				FillMapArea(Vector3Int.zero, mapGen.initialViewRadius);
			else
				RandomFillMap();


			for (int i = 0; i < rules.smoothSteps; ++i)
			{
				if (!SmoothMap(Vector3Int.zero))
					break;
			}

			ProcessMap();
		}


		private static void ProcessMap()
		{
			List<List<Vector3Int>> waterRegions = mapGen.GetRegions(TerrainType.WaterGeneration);
			TerrainData waterData = mapGen.GetTerrainData(TerrainType.WaterGeneration);

			foreach (List<Vector3Int> waterRegion in waterRegions)
				if (waterRegion.Count < waterData.thresholdSize)
					foreach (Vector3Int coord in waterRegion)
						mapGen.CreateAndSetTile(coord, waterData.tile, mapGen.GetTile(coord));


			List<List<Vector3Int>> landRegions = mapGen.GetRegions(TerrainType.LandGeneration);
			TerrainData landData = mapGen.GetTerrainData(TerrainType.LandGeneration);
			List<Region> survivingLandRegions = new List<Region>();

			foreach (List<Vector3Int> landRegion in landRegions)
			{
				if (landRegion.Count < landData.thresholdSize)
					foreach (Vector3Int coord in landRegion)
						mapGen.CreateAndSetTile(coord, landData.tile, mapGen.GetTile(coord));
				else
					survivingLandRegions.Add(new Region(landRegion));
			}

			if (survivingLandRegions.Count == 0)
			{
				Debug.Log("Map contains no rooms!");
				return;
			}

			survivingLandRegions.Sort();
			survivingLandRegions[0].isMainRegion = true;
			survivingLandRegions[0].isAccessibleFromMainRegion = true;

			//ConnectClosestRegions(survivingRegions);
		}

		private static bool SmoothMap(Vector3Int centerPoint, int radiusToSmooth = int.MaxValue)
		{
			Dictionary<TerrainTile, TerrainType> changesToMake =
				new Dictionary<TerrainTile, TerrainType>();
			bool ringEmpty = false;
			int i = 0;

			while (i <= radiusToSmooth && !ringEmpty)
			{
				ringEmpty = true;
				List<Vector3Int> ring = HexTools.GetRing(centerPoint, i++);

				foreach (Vector3Int ringTile in ring)
				{
					TerrainTile ttb = mapGen.GetTile(ringTile);
					if (ttb == null)
						continue;

					ringEmpty = false;

					int wallCount = 0;
					TerrainTile[] surroundingTiles = mapGen.GetSurroundingTiles(ringTile);
					foreach (TerrainTile tile in surroundingTiles)
					{
						if (tile == null || tile.terrainType == TerrainType.WaterGeneration)
							++wallCount;
					}


					if (wallCount > rules.minNeighboursToTurnToWater && ttb.terrainType != TerrainType.WaterGeneration)
						changesToMake[ttb] = TerrainType.WaterGeneration;
					else if (wallCount < rules.minNeighboursToTurnToWater && ttb.terrainType != TerrainType.LandGeneration)
						changesToMake[ttb] = TerrainType.LandGeneration;
				}
			}

			foreach (var change in changesToMake)
			{
				mapGen.CreateAndSetTile(change.Key.coordinates, mapGen.GetTerrainData(change.Value).tile, change.Key);
			}

			return changesToMake.Count > 0;
		}


		private static TerrainTile CreateAndSetTileFromNoise(Vector3Int coord, float noiseValue)
		{
			foreach (TerrainTile tt in rules.tiles)
			{
				TerrainData td = mapGen.GetTerrainData(tt.terrainType);
				if (noiseValue >= td.startHeight)
					return mapGen.CreateAndSetTile(coord, td.tile);
			}

			Debug.LogError("Did not find a TerrainTile to match noiseValue of " + noiseValue);
			return null;
		}


		private static void FillMapArea(Vector3Int spawnCenter, int radius)
		{
			float[,] noiseMap = Noise.GenerateNoiseMap(
				radius * 2, radius * 2,
				mapGen.noiseSettings, new Vector2(spawnCenter.y, -spawnCenter.x));

			for (int x = 0; x < noiseMap.GetLength(0); ++x)
			{
				for (int y = 0; y < noiseMap.GetLength(1); ++y)
				{
					Vector3Int coord = spawnCenter
						+ new Vector3Int(
							Mathf.CeilToInt(-noiseMap.GetLength(1) * .5f) + y,
							Mathf.CeilToInt(-noiseMap.GetLength(0) * .5f) + x, 0);

					if (mapGen.GetTile(coord) != null || HexTools.DistanceInTiles(coord, spawnCenter) > radius)
						continue;

					CreateAndSetTileFromNoise(coord, noiseMap[x, y]);
				}
			}
		}


		private static void RandomFillMap()
		{
			System.Random rng = new System.Random(mapGen.noiseSettings.GetSeed());

			for (int x = 0; x < mapGen.initialViewRadius; ++x)
			{
				for (int y = 0; y < mapGen.initialViewRadius; ++y)
				{
					Vector3Int coord = ArrayToOffsetCoords(x, y);

					if (y == 0 || x == 0 || y == mapGen.initialViewRadius - 1 || x == mapGen.initialViewRadius - 1
						|| rng.Next(0, 100) < rules.randomFillPercent)
					{
						mapGen.CreateAndSetTile(coord, rules.tiles[0]);
					}
					else
					{
						mapGen.CreateAndSetTile(coord, rules.tiles[1]);
					}
				}
			}
		}


		/// <summary>
		/// WARNING: Can only be used at initial map generation.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		private static Vector3Int ArrayToOffsetCoords(int x, int y)
		{
			return new Vector3Int(
						Mathf.CeilToInt(-mapGen.initialViewRadius * .5f) + y,
						Mathf.CeilToInt(-mapGen.initialViewRadius * .5f) + x, 0);
		}

	}
}