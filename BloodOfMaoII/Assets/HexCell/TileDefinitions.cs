using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtomosZ.BoMII.Terrain
{
	/// <summary>
	/// 1:1 between TerrainType and TerrainData.
	/// </summary>
	[CreateAssetMenu(menuName = "Tiles/TileDefinitions")]
	public class TileDefinitions : ScriptableObject
	{
		public enum TerrainType
		{
			// used for map generation
			WaterGenerator, LandGenerator,

			// water
			DeepWater, Water, ShallowWater, IcyWater,

			// fields
			Dirt, Grass, Sand, Snow, Desert,

			// forests
			LightForest, DeepForest,

			// high terrain
			Hill, Mountain,

			// dungeon
			DungeonWall, DungeonFloor,
		};

		public List<TerrainData> terrainData;
		public Dictionary<TerrainType, TerrainData> terrainDictionary;

		private TerrainData nullData = new TerrainData();


		private void OnEnable()
		{
			terrainData.Sort();
		}

		public TerrainData GetData(TerrainType terrainType)
		{
			foreach (TerrainData data in terrainData)
				if (data.tile.terrainType == terrainType)
					return data;
			return nullData;
		}

		public Dictionary<TerrainType, TerrainData> GetData()
		{
			if (terrainDictionary == null)
			{
				terrainDictionary = new Dictionary<TerrainType, TerrainData>();
				foreach (TerrainData data in terrainData)
				{
					if (data.tile.terrainType != TerrainType.LandGenerator
						|| data.tile.terrainType != TerrainType.WaterGenerator)
					{
						terrainDictionary[data.tile.terrainType] = data;
					}
				}
			}

			return terrainDictionary;
		}
	}


	[Serializable]
	public struct TerrainData : IComparable<TerrainData>
	{
		public string name;
		public float startHeight;

		public TerrainTile tile;

		[Tooltip("Minimun number of neighbours of the same type to guarantee not to change" +
			" in a single generation.")]
		public int stableMinNeighbours;

		[Tooltip("Minimum size of region that can exist (will be filled in with tiles decided by generator)")]
		public int regionThresholdSize;


		public int CompareTo(TerrainData other)
		{
			return startHeight.CompareTo(other.startHeight);
		}
	}
}