using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AtomosZ.BoMII.Terrain
{
	[CreateAssetMenu(menuName = "Tiles/BaseTile")]
	public class TerrainTileBase : Tile
	{
		public enum Cardinality { NE, N, NW, SW, S, SE };
		//public enum TerrainType { NotSet = -100, DeepWater = -50, Water = 1, Grass = 5, Trees, Hills = 9, Mountains = 14 };

		//public TerrainType terrainType = TerrainType.NotSet;
		public enum TerrainType { Black, White };
		public TerrainType type;
		public Vector3Int coordinates;
		public TextMeshPro text;

		private Tilemap tilemap;


		private void OnEnable()
		{
			tilemap = GameObject.FindGameObjectWithTag(Tags.TerrainTilemap).GetComponent<Tilemap>();
		}
	}
}