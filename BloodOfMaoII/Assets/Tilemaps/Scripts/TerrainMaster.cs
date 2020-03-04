using System;
using System.Collections.Generic;
using AtomosZ.BoMII.Terrain.Generators;
using UnityEngine;
using UnityEngine.Tilemaps;
using static AtomosZ.BoMII.Terrain.TerrainTile;


namespace AtomosZ.BoMII.Terrain
{
	public class TerrainMaster : MonoBehaviour
	{
		[SerializeField] private MapGenerator mapGenerator = null;
		// tilemap related variables
		public Tilemap tilemap = null;
		[SerializeField] private GenesisTile genesis = null;
		[SerializeField] private SpawnTile spawnTilePrefab = null;
		[SerializeField] private TerrainTileBase[] terrainTiles = null;

		private Camera cam;
		private Vector3 cellsize;


		public void Start()
		{
			cam = Camera.main;
			cellsize = tilemap.cellSize;
			//mapGenerator.DrawMapInEditor();
		}




		public void Update()
		{
			Vector3 mousepos = Input.mousePosition;
			Ray ray = cam.ScreenPointToRay(mousepos);
			var plane = new Plane(Vector3.back, Vector3.zero);
			if (plane.Raycast(ray, out float hitDist))
			{
				genesis.Disable(false);
				var worldpoint = ray.GetPoint(hitDist);
				var tilepoint = tilemap.WorldToCell(worldpoint);
				// lock it to grid
				genesis.transform.position = tilemap.CellToWorld(tilepoint);

				TileBase tile = tilemap.GetTile(tilepoint);
				if (tile != null)
				{
					genesis.SetTileValid(false);
					if (Input.GetMouseButtonDown(0))
						Debug.Log(tilepoint);
				}
				else
				{
					genesis.SetTileValid(true);
					if (Input.GetMouseButtonDown(0))
					{
						Debug.Log(tilepoint);
						RevealArea(worldpoint, 5);
					}
				}
			}
			else
				genesis.Disable(true);
		}




		/// <summary>
		/// WorldPos should be (optionally unclamped) world position, radius how many tiles from center
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
					spawner.SpawnSelf(tilemap, terrainTiles[UnityEngine.Random.Range(0, 2)]);
				}
			}

			//StartCoroutine(StartSimulation(spawners, mappos));
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


		//private IEnumerator StartSimulation(List<SpawnTile> spawners, Vector3Int mappos)
		//{
		//	while (simulate)
		//	{
		//		foreach (SpawnTile tile in spawners)
		//		{

		//		}
		//	}
		//}


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