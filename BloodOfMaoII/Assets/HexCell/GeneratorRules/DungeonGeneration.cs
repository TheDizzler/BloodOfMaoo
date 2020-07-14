using System.Collections.Generic;
using UnityEngine;
using static AtomosZ.BoMII.Terrain.TileDefinitions;

namespace AtomosZ.BoMII.Terrain.Generation
{
	public static class DungeonGeneration
	{
		private static HexMapGenerator mapGen;
		private static DungeonRules rules;


		public static void GenerateDungeon()
		{

		}

		private static void ProcessMap()
		{
			List<List<Vector3Int>> wallRegions = mapGen.GetRegions(TerrainType.DungeonWall);
			TerrainData wallData = mapGen.GetTerrainData(TerrainType.DungeonWall);

			foreach (List<Vector3Int> wallRegion in wallRegions)
				if (wallRegion.Count < wallData.thresholdSize)
					foreach (Vector3Int coord in wallRegion)
						mapGen.CreateAndSetTile(coord, rules.openTile, mapGen.GetTile(coord));

			List<List<Vector3Int>> roomRegions = mapGen.GetRegions(TerrainType.DungeonFloor);
			TerrainData roomData = mapGen.GetTerrainData(TerrainType.DungeonFloor);

			List<Region> survivingRegions = new List<Region>();
			foreach (List<Vector3Int> roomRegion in roomRegions)
			{
				if (roomRegion.Count < roomData.thresholdSize)
					foreach (Vector3Int coord in roomRegion)
						mapGen.CreateAndSetTile(coord, rules.wallTile, mapGen.GetTile(coord));
				else
					survivingRegions.Add(new Region(roomRegion));
			}

			if (survivingRegions.Count == 0)
			{
				Debug.Log("Map contains no rooms!");
				return;
			}

			survivingRegions.Sort();
			survivingRegions[0].isMainRegion = true;
			survivingRegions[0].isAccessibleFromMainRegion = true;

			ConnectClosestRegions(survivingRegions);
		}

		private static void ConnectClosestRegions(List<Region> allRegions, bool forceAccessibilityFromMainRegion = false)
		{
			List<Region> regionListA = new List<Region>();
			List<Region> regionListB = new List<Region>();

			if (forceAccessibilityFromMainRegion)
			{
				foreach (Region region in allRegions)
				{
					if (!region.isAccessibleFromMainRegion)
						regionListA.Add(region);
					else
						regionListB.Add(region);
				}
			}
			else
			{
				regionListA = allRegions;
				regionListB = allRegions;
			}

			int shortestDist = 0;
			Vector3Int bestTileA = new Vector3Int();
			Vector3Int bestTileB = new Vector3Int();
			Region bestRegionA = new Region();
			Region bestRegionB = new Region();

			bool possibleConnectionFound = false;

			foreach (Region regionA in regionListA)
			{
				if (!forceAccessibilityFromMainRegion)
				{
					possibleConnectionFound = false;
					if (regionA.connectedRegions.Count > 0)
						continue;
				}

				foreach (Region regionB in regionListB)
				{
					if (regionA == regionB || regionA.IsConnected(regionB))
						continue;

					for (int tileIndexA = 0; tileIndexA < regionA.edgeTiles.Count; ++tileIndexA)
					{
						for (int tileIndexB = 0; tileIndexB < regionB.edgeTiles.Count; ++tileIndexB)
						{
							Vector3Int tileA = regionA.edgeTiles[tileIndexA];
							Vector3Int tileB = regionB.edgeTiles[tileIndexB];
							int distanceBetweenRooms = HexTools.DistanceInTiles(tileA, tileB);

							if (distanceBetweenRooms < shortestDist || !possibleConnectionFound)
							{
								shortestDist = (int)distanceBetweenRooms;
								possibleConnectionFound = true;
								bestTileA = tileA;
								bestTileB = tileB;
								bestRegionA = regionA;
								bestRegionB = regionB;
							}
						}
					}
				}

				if (possibleConnectionFound && !forceAccessibilityFromMainRegion)
				{
					CreatePassage(bestRegionA, bestRegionB, bestTileA, bestTileB);
				}
			}

			if (possibleConnectionFound && forceAccessibilityFromMainRegion)
			{
				CreatePassage(bestRegionA, bestRegionB, bestTileA, bestTileB);
				ConnectClosestRegions(allRegions, true);
			}

			if (!forceAccessibilityFromMainRegion)
				ConnectClosestRegions(allRegions, true);
		}

		private static void CreatePassage(Region regionA, Region regionB, Vector3Int tileA, Vector3Int tileB)
		{
			List<Vector3Int> line = HexTools.GetLine(tileA, tileB);
			Passageway passageway = new Passageway(regionA, regionB, tileA, tileB, line);

			Region.ConnectRegions(regionA, regionB, passageway);
			//Debug.DrawLine(tilemap.CellToWorld(tileA), tilemap.CellToWorld(tileB), Color.green, 10);

			foreach (Vector3Int tileCoord in line)
			{
				mapGen.ChangeAllTilesAround(tileCoord, rules.passageSize, rules.openTile);
			}
		}
	}
}