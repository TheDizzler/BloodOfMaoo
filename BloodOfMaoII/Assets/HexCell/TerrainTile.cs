using System;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using static AtomosZ.BoMII.Terrain.TileDefinitions;

namespace AtomosZ.BoMII.Terrain
{
	[CreateAssetMenu(menuName = "Tiles/BaseTile")]
	public class TerrainTile : Tile
	{
		public TerrainType terrainType;
		public Vector3Int coordinates;
		/// <summary>
		/// Height evaluated from Noise Map.
		/// </summary>
		[HideInInspector] public float height;

		/// <summary>
		/// For debugging.
		/// </summary>
		[HideInInspector] public TextMeshPro text;
	}
}