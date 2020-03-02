using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static AtomosZ.BoMII.Terrain.TerrainTile;

namespace AtomosZ.BoMII.Terrain
{
	public class TerrainMaster : MonoBehaviour
	{
		public Tilemap tilemap;
		public TerrainTileBase tilebase;
		public GenesisTile genesis;
		public TerrainTile terrainSpawnerPrefab;
		public SpawnTile spawnTilePrefab;
		public BoundsInt mapBounds;
		public Vector3 cellsize;
		private Camera cam;


		public void Start()
		{
			cam = Camera.main;
			mapBounds = tilemap.cellBounds;
			cellsize = tilemap.cellSize;
		}

		public void Update()
		{
			Vector3Int tilepoint = tilemap.WorldToCell(cam.ScreenToWorldPoint(Input.mousePosition));
			genesis.transform.position = tilemap.CellToWorld(tilepoint); // lock it to grid
			TileBase tile = tilemap.GetTile(tilepoint);
			if (tile != null)
			{
				genesis.SetEnabled(false);
				if (Input.GetMouseButtonDown(0))
				{
					Debug.Log(tilepoint);
				}
			}
			else
			{
				genesis.SetEnabled(true);
				if (Input.GetMouseButtonDown(0))
				{
					//TerrainTile spawner = Instantiate(terrainSpawnerPrefab, genesis.transform.position, Quaternion.identity, this.transform);
					//tilemap.SetTile(tilemap.LocalToCell(genesis.transform.position), tilebase);
					//spawner.StartSpawnTiles(1);
					RevealArea(cam.ScreenToWorldPoint(Input.mousePosition), 3);
				}
			}
		}

		/// <summary>
		/// WorldPos should be (unclamped) world position, radius how many tiles from center
		/// is revealed (0 == only center tile, 1 == 7 tiles total)
		/// </summary>
		/// <param name="worldPos"></param>
		/// <param name="radius"></param>
		public void RevealArea(Vector3 worldPos, int radius)
		{
			Vector3Int mappos = tilemap.WorldToCell(worldPos);
			worldPos = tilemap.CellToWorld(mappos);
			List<SpawnTile> spawners = new List<SpawnTile>();
			spawners.Add(Instantiate(spawnTilePrefab, worldPos, Quaternion.identity, this.transform));

			int r = 0;
			if (r < radius)
			{
				List<SpawnTile> subspawners = CreateSpawnersAround(worldPos);
				spawners.AddRange(subspawners);

				while (r + 1 < radius)
				{
					subspawners = CreateSpawnersAround(subspawners);
					spawners.AddRange(subspawners);
					++r;
				}
			}

			foreach (SpawnTile spawner in spawners)
			{
				TileBase tile = tilemap.GetTile(tilemap.WorldToCell(spawner.transform.position));
				if (tile != null)
				{
					// self destruct instead of spawning
					Destroy(spawner.gameObject);
				}
				else
				{
					spawner.SpawnSelf(tilemap, tilebase);
				}
			}
		}


		private List<SpawnTile> CreateSpawnersAround(List<SpawnTile> subspawners)
		{
			List<SpawnTile> newSpawners = new List<SpawnTile>();
			foreach (SpawnTile sub in subspawners)
			{
				newSpawners.AddRange(CreateSpawnersAround(sub.transform.position));
			}

			return newSpawners;
		}


		private List<SpawnTile> CreateSpawnersAround(Vector3 worldPos)
		{
			List<SpawnTile> newSpawners = new List<SpawnTile>();
			foreach (CardinalTiles dir in Enum.GetValues(typeof(CardinalTiles)))
			{
				Vector3 worldtilepos = GetWorldOfTileAt(dir, worldPos);
				SpawnTile newspawner = Instantiate(spawnTilePrefab, worldtilepos, Quaternion.identity, this.transform);
				if (!newspawner.Overlaps())
				{
					newSpawners.Add(newspawner);
					newspawner.name = Enum.GetName(typeof(CardinalTiles), dir);
				}
				else
					Destroy(newspawner.gameObject);
			}

			return newSpawners;
		}

		private Vector3 GetWorldOfTileAt(CardinalTiles direction, Vector3 worldpos)
		{
			switch (direction)
			{
				case CardinalTiles.N:
					worldpos.y += (int)cellsize.y;
					break;
				case CardinalTiles.S:
					worldpos.y -= (int)cellsize.y;
					break;
				case CardinalTiles.SE:
					worldpos.x += (int)cellsize.x * .75f;
					worldpos.y -= (int)cellsize.y * .5f;
					break;
				case CardinalTiles.SW:
					worldpos.x -= (int)cellsize.x * .75f;
					worldpos.y -= (int)cellsize.y * .5f;
					break;
				case CardinalTiles.NE:
					worldpos.x += (int)cellsize.x * .75f;
					worldpos.y += (int)cellsize.y * .5f;
					break;
				case CardinalTiles.NW:
					worldpos.x -= (int)cellsize.x * .75f;
					worldpos.y += (int)cellsize.y * .5f;
					break;
			}

			return worldpos;
		}
	}
}