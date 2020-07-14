using System;
using System.Collections.Generic;
using UnityEngine;
using static AtomosZ.BoMII.Terrain.TerrainTile;
using static AtomosZ.BoMII.Terrain.TileDefinitions;

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
			WaterGeneration, LandGeneration,

			// water
			DeepWater, Water, ShallowWater, IcyWater,

			// fields
			Dirt, Grass, Sand, Snow,

			// forests
			LightForest, DeepForest,

			// high terrain
			Hill, Mountain,

			// dungeon
			DungeonWall, DungeonFloor,
		};

		public List<TerrainData> terrainData;

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
	}


	[Serializable]
	public struct TerrainData : IComparable<TerrainData>
	{
		public string name;
		//public TerrainType terrainType;
		public float startHeight;
		
		public TerrainTile tile;

		[Tooltip("Minimum size of region that can exist (will be filled in with tiles decided by generator)")]
		public int thresholdSize;


		public int CompareTo(TerrainData other)
		{
			return startHeight.CompareTo(other.startHeight);
		}
	}
}