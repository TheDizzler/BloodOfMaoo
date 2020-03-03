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
		public enum DrawMode { NoiseMap, ColorMap, Mesh, HexGrid };
		//private enum TerrainIndex {DeepWater, Water = 10, Sand = 20, Grass = 30, Hills = 60, Mountain = 75, Ice = 90 }
		public const int mapChunkSize = 241;
		[Range(0,6)]
		[SerializeField] private int levelOfDetail = 1;

		public DrawMode drawMode;
		public bool autoUpdate = true;

		// tilemap related variables
		public Tilemap tilemap = null;
		[SerializeField] private GenesisTile genesis = null;
		[SerializeField] private SpawnTile spawnTilePrefab = null;
		[SerializeField] private TerrainTileBase[] terrainTiles = null;
		
		// noise map gen related variables
		//[SerializeField] private int mapChunkSize, mapChunkSize;
		[SerializeField] private float noiseScale = 1;
		[Range(1, 126)]
		[SerializeField] private int octaves = 1;
		[Range(0, 1)]
		[SerializeField] private float persistance = 1;
		[SerializeField] private float lacunarity = 1;
		[SerializeField] private int seed = 1;
		[SerializeField] private Vector2 offset = new Vector2();
		[SerializeField] private float meshHeighMultiplier = 1;
		[SerializeField] private AnimationCurve heightMapCurve = null;

		[SerializeField] private TerrainType[] regions = null;


		private Camera cam;
		private Vector3 cellsize;


		public void Start()
		{
			cam = Camera.main;
			cellsize = tilemap.cellSize;
			GenerateMap();
		}

		public void GenerateMap()
		{
			// calculate the offsets based on the tile position
			float[,] noiseMap = Noise.GenerateNoiseMap(
				mapChunkSize, mapChunkSize, seed, noiseScale,
				octaves, persistance, lacunarity, offset);

			//tilemap.ClearAllTiles();
			//int halfMapWidth = mapChunkSize / 2;
			//int halfMapHeight = mapChunkSize / 2;

			Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
			for (int y = 0; y < mapChunkSize; ++y)
			{
				for (int x = 0; x < mapChunkSize; ++x)
				{
					float currentHeight = noiseMap[x, y];
					for (int i = 0; i < regions.Length; ++i)
					{
						if (currentHeight <= regions[i].height)
						{
							colorMap[y * mapChunkSize + x] = regions[i].color;
							//tilemap.SetTile(new Vector3Int( y - halfMapHeight, x - halfMapWidth, 0), terrainTiles[i]);
							break;
						}
					}
				}
			}

			MapDisplay display = GetComponent<MapDisplay>();
			switch (drawMode)
			{
				case DrawMode.NoiseMap:
					display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
					break;
				case DrawMode.ColorMap:
					display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
					break;
				case DrawMode.Mesh:
					display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeighMultiplier, heightMapCurve, levelOfDetail),
						TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
					break;
				case DrawMode.HexGrid:
					
					break;
			}
		}

		public void OnValidate()
		{
			if (lacunarity < 1)
				lacunarity = 1;
		}

		public void Update()
		{
			Vector3Int tilepoint = tilemap.WorldToCell(cam.ScreenToWorldPoint(Input.mousePosition));
			genesis.transform.position = tilemap.CellToWorld(tilepoint); // lock it to grid
			TileBase tile = tilemap.GetTile(tilepoint);
			if (tile != null)
			{
				genesis.SetEnabled(false);
			}
			else
			{
				genesis.SetEnabled(true);
				if (Input.GetMouseButtonDown(0))
				{
					RevealArea(cam.ScreenToWorldPoint(Input.mousePosition), 5);
				}
			}
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