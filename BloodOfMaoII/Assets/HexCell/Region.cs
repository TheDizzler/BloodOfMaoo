using System;
using System.Collections.Generic;
using AtomosZ.BoMII.Terrain.Generation;
using UnityEngine;

namespace AtomosZ.BoMII.Terrain
{
	/// <summary>
	/// A region represents a contiguous area of a single biome.
	/// </summary>
	public class Region : IComparable<Region>
	{
		public TerrainTileBase.TerrainType regionType;
		public List<Vector3Int> tileCoords;
		public List<Vector3Int> edgeTiles;
		public List<Region> connectedRegions;
		public int regionSize;
		public bool isAccessibleFromMainRegion;
		public bool isMainRegion;



		public Region(List<Vector3Int> regionTileCoords)
		{
			tileCoords = regionTileCoords;
			regionSize = regionTileCoords.Count;

			connectedRegions = new List<Region>();
			edgeTiles = new List<Vector3Int>();

			HexMapGenerator mapGenerator = GameObject.FindGameObjectWithTag(Tags.HexMapGenerator).GetComponent<HexMapGenerator>();
			regionType = mapGenerator.GetTile(regionTileCoords[0]).type;

			foreach (Vector3Int coord in tileCoords)
			{
				TerrainTileBase[] surroundingTiles = mapGenerator.GetSurroundingTiles(coord);
				foreach (TerrainTileBase ttb in surroundingTiles)
				{
					if (ttb == null) // should we consider the map edge to be a tile edge?
						continue;
					if (ttb.type != regionType)
					{
						edgeTiles.Add(coord); // if we don't break here we'll get an edge tile per face instead of per tile
						break;
					}
				}
			}
		}

		public Region() { }


		public static void ConnectRegions(Region regionA, Region regionB)
		{
			if (regionA.isAccessibleFromMainRegion)
				regionB.SetAccessibleFromMainRegion();
			else if (regionB.isAccessibleFromMainRegion)
				regionA.SetAccessibleFromMainRegion();

			regionA.connectedRegions.Add(regionB);
			regionB.connectedRegions.Add(regionA);
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
}