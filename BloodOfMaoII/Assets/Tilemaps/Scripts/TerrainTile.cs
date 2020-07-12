using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static AtomosZ.BoMII.Terrain.HexTools;
using static AtomosZ.BoMII.Terrain.TerrainTileBase;

namespace AtomosZ.BoMII.Terrain
{
	public class TerrainTile : MonoBehaviour
	{
		//public TerrainType terrainType = TerrainType.NotSet;
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
						spawnTiles[(int)Cardinality.NE] = child;
						break;
					case "TileSpawnPoint - N":
						spawnTiles[(int)Cardinality.N] = child;
						break;
					case "TileSpawnPoint - NW":
						spawnTiles[(int)Cardinality.NW] = child;
						break;
					case "TileSpawnPoint - SW":
						spawnTiles[(int)Cardinality.SW] = child;
						break;
					case "TileSpawnPoint - S":
						spawnTiles[(int)Cardinality.S] = child;
						break;
					case "TileSpawnPoint - SE":
						spawnTiles[(int)Cardinality.SE] = child;
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

	[Serializable]
	public struct TerrainType
	{
		public string name;
		public float height;
		public Color color;
	}
}