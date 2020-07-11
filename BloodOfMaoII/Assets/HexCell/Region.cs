using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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

		private Tilemap tilemap;


		public Region(List<Vector3Int> regionTileCoords, Tilemap tilemap)
		{
			tileCoords = regionTileCoords;
			this.tilemap = tilemap;
			regionSize = regionTileCoords.Count;
			connectedRegions = new List<Region>();
			edgeTiles = new List<Vector3Int>();

			HexMapGenerator mapGenerator = GameObject.FindGameObjectWithTag(Tags.HexMapGenerator).GetComponent<HexMapGenerator>();

			foreach (Vector3Int coord in tileCoords)
			{
				TerrainTileBase[] surroundingTiles = mapGenerator.GetSurroundingTiles(coord);
				foreach (TerrainTileBase ttb in surroundingTiles)
				{
					if (ttb == null)
						continue;
					if (ttb.type != regionType)
						edgeTiles.Add(coord); // if we don't break here we'll get an edge tile per face instead of per tile
				}
			}
		}

		public Region() { }


		public static void ConnectRegions(Region regionA, Region regionB)
		{
			if (regionA.isAccessibleFromMainRegion)
				regionB.isAccessibleFromMainRegion = true;
			else if (regionB.isAccessibleFromMainRegion)
				regionA.isAccessibleFromMainRegion = true;

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


	}
}