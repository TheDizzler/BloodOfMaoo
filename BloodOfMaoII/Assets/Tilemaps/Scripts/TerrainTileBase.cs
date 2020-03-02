using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AtomosZ.BoMII.Terrain
{
	[CreateAssetMenu(menuName = "Tiles/BaseTile")]
	public class TerrainTileBase : Tile
	{
		public enum CardinalTiles { NE, N, NW, SW, S, SE };
		public enum TerrainType { NotSet = -100, DeepWater = -50, Water = 1, Plains = 5, Hills = 9, Mountains = 14 };

		public TerrainType terrainType = TerrainType.NotSet;

		private Tilemap tilemap;


		private void OnEnable()
		{
			tilemap = GameObject.FindGameObjectWithTag(Tags.TerrainTilemap).GetComponent<Tilemap>();
		}
	}
}