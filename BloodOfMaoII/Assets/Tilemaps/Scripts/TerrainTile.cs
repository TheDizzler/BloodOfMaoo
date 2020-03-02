using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AtomosZ.BoMII.Terrain
{
	public class TerrainTile : MonoBehaviour
	{
		public enum CardinalTiles { NE, N, NW, SW, S, SE };
		public enum TerrainType { NotSet = -100, DeepWater = -50, Water = 1, Grass = 5, Hills = 9, Mountains = 14 };

		public TerrainType terrainType = TerrainType.NotSet;
		[SerializeField] private TerrainTileBase tileBase = null;
		private SpawnTile[] spawnTiles = new SpawnTile[6];
		private Tilemap tilemap;
		public Coroutine spawning;
		//[SerializeField] private bool genesis = false;


		public void Awake()
		{
			tilemap = GameObject.FindGameObjectWithTag("TerrainTileMap").GetComponent<Tilemap>();

			List<SpawnTile> delete = new List<SpawnTile>();
			for (int i = 0; i < transform.childCount; ++i)
			{
				SpawnTile child = transform.GetChild(i).GetComponent<SpawnTile>();
				if (child == null)
				{
					Debug.LogWarning("da hell? " + transform.GetChild(i).name);
					continue;
				}

				Vector3Int tilepoint = tilemap.WorldToCell(child.transform.position);
				TileBase tile = tilemap.GetTile<TileBase>(tilepoint);

				if (tile != null || child.Overlaps())
				{
					delete.Add(child);
					continue;
				}

				//if (!child.IsInBounds(tilemap))
				//{
				//	delete.Add(child);
				//	continue;
				//}

				switch (child.name)
				{
					case "TileSpawnPoint - NE":
						spawnTiles[(int)CardinalTiles.NE] = child;
						break;
					case "TileSpawnPoint - N":
						spawnTiles[(int)CardinalTiles.N] = child;
						break;
					case "TileSpawnPoint - NW":
						spawnTiles[(int)CardinalTiles.NW] = child;
						break;
					case "TileSpawnPoint - SW":
						spawnTiles[(int)CardinalTiles.SW] = child;
						break;
					case "TileSpawnPoint - S":
						spawnTiles[(int)CardinalTiles.S] = child;
						break;
					case "TileSpawnPoint - SE":
						spawnTiles[(int)CardinalTiles.SE] = child;
						break;
					default:
						Debug.LogError("Shit is fucked in TerrainTile Town: " + child.name);
						break;
				}
			}

			for (int i = delete.Count - 1; i >= 0; --i)
				Destroy(delete[i].gameObject);
		}



		public void StartSpawnTiles(int viewRange)
		{
			spawning = StartCoroutine(SpawnTiles());
		}

		public IEnumerator SpawnTiles()
		{
			yield return new WaitForSeconds(.5f);
			foreach (SpawnTile spawn in spawnTiles)
			{
				if (spawn == null)
					continue;
				spawn.SpawnSelf(tilemap, tileBase);
				yield return new WaitForSeconds(.5f);
			}

			spawning = null;
			Destroy(this.gameObject);
		}
	}
}