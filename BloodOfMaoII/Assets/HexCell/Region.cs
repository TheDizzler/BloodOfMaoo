using System;
using System.Collections.Generic;
using AtomosZ.BoMII.Terrain.Generation;
using UnityEngine;
using static AtomosZ.BoMII.Terrain.TerrainTile;
using static AtomosZ.BoMII.Terrain.TileDefinitions;

namespace AtomosZ.BoMII.Terrain
{
	/// <summary>
	/// A region represents a contiguous area of a single biome.
	/// </summary>
	public class Region : IComparable<Region>
	{
		public TerrainType regionType;
		public List<Vector3Int> tileCoords;
		public List<Vector3Int> edgeTiles;
		public List<Region> connectedRegions;
		public Dictionary<Vector3Int, Passageway> tileWithPassage;
		public int regionSize;
		public bool isAccessibleFromMainRegion;
		public bool isMainRegion;



		public Region(List<Vector3Int> regionTileCoords)
		{
			tileCoords = regionTileCoords;
			regionSize = regionTileCoords.Count;

			connectedRegions = new List<Region>();
			edgeTiles = new List<Vector3Int>();
			tileWithPassage = new Dictionary<Vector3Int, Passageway>();

			HexMapGenerator mapGenerator = GameObject.FindGameObjectWithTag(Tags.HexMapGenerator).GetComponent<HexMapGenerator>();
			regionType = mapGenerator.GetTile(regionTileCoords[0]).terrainType;

			foreach (Vector3Int coord in tileCoords)
			{
				mapGenerator.GetTile(coord).region = this;
				TerrainTile[] surroundingTiles = mapGenerator.GetSurroundingTiles(coord);
				foreach (TerrainTile ttb in surroundingTiles)
				{
					if (ttb == null) // should we consider the map edge to be a tile edge?
						continue;
					if (ttb.terrainType != regionType)
					{
						edgeTiles.Add(coord); // if we don't break here we'll get an edge tile per face instead of per tile
						break;
					}
				}
			}
		}

		public Region() { }


		public static void ConnectRegions(Region regionA, Region regionB, Passageway passageway)
		{
			if (regionA.isAccessibleFromMainRegion)
				regionB.SetAccessibleFromMainRegion();
			else if (regionB.isAccessibleFromMainRegion)
				regionA.SetAccessibleFromMainRegion();

			regionA.connectedRegions.Add(regionB);
			regionB.connectedRegions.Add(regionA);
			regionA.tileWithPassage[passageway.passageTiles[0]] = passageway;
			regionB.tileWithPassage[passageway.passageTiles[passageway.passageTiles.Count -1]] = passageway;
		}

		public bool IsConnected(Region otherRegion)
		{
			return connectedRegions.Contains(otherRegion);
		}

		public int CompareTo(Region otherRegion)
		{
			return otherRegion.regionSize.CompareTo(regionSize);
		}

		private void SetAccessibleFromMainRegion()
		{
			if (!isAccessibleFromMainRegion)
			{
				isAccessibleFromMainRegion = true;
				foreach (Region connectedRoom in connectedRegions)
					connectedRoom.SetAccessibleFromMainRegion();
			}
		}
	}

	/// <summary>
	/// A sort of region that connects to major regions.
	/// </summary>
	public class Passageway
	{
		public TerrainType regionType;
		public List<Vector3Int> passageTiles;
		public Region regionA;
		public Region regionB;


		public Passageway(Region regionA, Region regionB, Vector3Int tileA, Vector3Int tileB, List<Vector3Int> line)
		{
			regionType = regionA.regionType;
			this.regionA = regionA;
			this.regionB = regionB;
			passageTiles = line;
		}

	}
}