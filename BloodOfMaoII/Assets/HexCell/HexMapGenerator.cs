using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using static AtomosZ.BoMII.Terrain.HexTools;
using static AtomosZ.BoMII.Terrain.TerrainTile;
using static AtomosZ.BoMII.Terrain.TileDefinitions;
using Debug = UnityEngine.Debug;

namespace AtomosZ.BoMII.Terrain.Generation
{
	/// <summary>
	/// Map Generation overview:
	///		Step 1: Create water and landmasses. Smooth 3 or 4 times. Don't cull small islands/bodies of water.
	///		Step 2: Create mountains on landmasses using original noise map or 2nd map with mask (see Planetary Generation).
	///			Turn this into 3D mesh?
	///			Smooth smaller mountains into hills.
	///		Step 3: Create rivers from mountains to ocean/lakes, from lakes to ocean.
	///		Step 3: Create biome regions (forest, fields, etc)
	///		Step 4: ???
	///		Step 5: Profit
	///		
	/// Alternate, Simpler Generation:
	///		Step 1: Same as above.
	///		Step 2: Create tiles from map data and have a cellular automata battle royal.
	/// </summary>
	public class HexMapGenerator : MonoBehaviour
	{
		public TileDefinitions tileDefinitions;
		public StageOneRules stageOne;
		public StageTwoRules stageTwo;
		


		[Tooltip("For heavy debugging of cells. Generation is slow. Use small maps.")]
		public bool debugCells;


		public NoiseSettings noiseSettings;
		public GameObject locTextPrefab;
		public Transform textHolder;

		public int revealRadius = 5;

		public Tilemap tilemap = null;

		[HideInInspector]
		public float tileHeight, tileWidth;

		private Camera cam = null;

		private TerrainTile startTile;
		private TerrainTile currentTile;
		private List<Vector3Int> lastLine = new List<Vector3Int>();
		private List<Vector3Int> lastRadius = new List<Vector3Int>();
		private List<Vector3Int> lastRing = new List<Vector3Int>();
		private Vector3 tilemapScale;



		void Start()
		{
			cam = Camera.main;
			
			GenerateMap();
		}

		void Update()
		{
			Vector3 mousepos = Input.mousePosition;
			Ray ray = cam.ScreenPointToRay(mousepos);
			Plane plane = new Plane(Vector3.back, Vector3.zero);
			if (plane.Raycast(ray, out float hitDist))
			{
				Vector3 worldpoint = ray.GetPoint(hitDist);
				Vector3Int tilepoint = tilemap.WorldToCell(worldpoint);

				TerrainTile tile = GetTile(tilepoint);

				if (startTile != null)
				{
					foreach (Vector3Int lineTile in lastLine)
						tilemap.SetColor(lineTile, Color.white);
					lastLine.Clear();
					foreach (Vector3Int radiusTile in lastRadius)
						tilemap.SetColor(radiusTile, Color.white);
					lastRadius.Clear();
					foreach (Vector3Int ringTile in lastRing)
						tilemap.SetColor(ringTile, Color.white);
					lastRing.Clear();


					if (Input.GetMouseButtonUp(0))
					{
						startTile = null;
					}
					else
					{
						int n = HexTools.DistanceInTiles(startTile.coordinates, tilepoint);
						List<Vector3Int> radius =
							HexTools.GetSpiral(startTile.coordinates, n);
						//HexTools.GetTilesInRange(startTile.coordinates, n);
						List<Vector3Int> line = HexTools.GetLine(startTile.coordinates, tilepoint);
						List<Vector3Int> ring = HexTools.GetRing(startTile.coordinates, n);

						foreach (Vector3Int radiusTile in radius)
						{
							tilemap.SetColor(radiusTile, Color.yellow);
						}

						foreach (Vector3Int radiusTile in ring)
						{
							tilemap.SetColor(radiusTile, Color.red);
						}

						foreach (Vector3Int lineTile in line)
						{
							tilemap.SetColor(lineTile, Color.magenta);
						}



						lastLine = line;
						lastRadius = radius;
						lastRing = ring;
					}
				}
				if (tile == null && Input.GetMouseButtonDown(0))
				{
					RevealArea(tilepoint, revealRadius);
				}
				else
				{
					if (tile != null)
					{
						if (Input.GetMouseButtonDown(0))
						{
							Debug.Log("World: " + worldpoint + " Coords: " + tilepoint);
							startTile = tile;
						}
					}
					else
					{
						if (Input.GetMouseButtonDown(0))
						{
							Debug.Log("Null Tilepoint: " + tilepoint);
						}
					}
				}
			}
		}




		public void ClearMap()
		{
			tilemap.ClearAllTiles();
			if (Application.isPlaying)
				for (int i = textHolder.childCount - 1; i >= 0; --i)
					Destroy(textHolder.GetChild(i).gameObject);
			else
				for (int i = textHolder.childCount - 1; i >= 0; --i)
					DestroyImmediate(textHolder.GetChild(i).gameObject);
		}


		public void GenerateMap()
		{
			ClearMap();
			tilemapScale = GameObject.FindGameObjectWithTag(Tags.TerrainTilemap).transform.localScale;

			tileHeight = Vector3.Distance(
				tilemap.CellToWorld(HexTools.GetAdjacentTileOffset(Vector3Int.zero, Cardinality.N)),
				tilemap.CellToWorld(new Vector3Int(0, 0, 0)));

			float diagonalDist = Vector3.Distance(
				tilemap.CellToWorld(Vector3Int.zero),
				tilemap.CellToWorld(HexTools.GetAdjacentTileOffset(Vector3Int.zero, Cardinality.NE)));

			tileWidth = Mathf.Sqrt(diagonalDist * diagonalDist - Mathf.Pow(tileHeight * .5f, 2)) * 4 / 3;

			tileDefinitions.terrainData.Sort();

			var regionDict = StageOne.RunGeneration(this);
			StageTwo.RunGeneration(this, regionDict);
			//if (useNoise)
			//	FillMapArea(Vector3Int.zero, initialViewRadius);
			//else
			//	RandomFillMap();


			//for (int i = 0; i < smoothSteps; ++i)
			//{
			//	if (!SmoothMap(Vector3Int.zero))
			//		break;
			//}

			//ProcessMap();
		}


		public TerrainTile GetTile(Vector3Int offsetGridCoords)
		{
			return tilemap.GetTile<TerrainTile>(offsetGridCoords);
		}

		public TerrainData GetTerrainData(Vector3Int coord)
		{
			return tileDefinitions.GetData(GetTile(coord).terrainType);
		}

		public TerrainData GetTerrainData(TerrainType terrainType)
		{
			return tileDefinitions.GetData(terrainType);
		}

		public void ChangeAllTilesAround(Vector3Int center, int radius, TerrainTile newType)
		{
			List<Vector3Int> tiles = HexTools.GetTilesInRange(center, radius - 1);
			foreach (Vector3Int tile in tiles)
			{
				TerrainTile ttb = GetTile(tile);
				if (tile == null)
					continue;
				CreateAndSetTile(tile, newType, ttb);
			}
		}

		/// <summary>
		/// Out of curiosity, did benchmarks between 3 region-gathering methods:
		///		Array - start at (0,0), convert to offset coordinates
		///		Ring - get rings around (0,0,0)
		///		TilesBlock - use tilemap.GetTilesBlock() to get all cells in tilemap.cellbounds
		/// 
		/// On a 200x200 grid:
		///			
		///		Array:		00:00:00.1374806
		///		Rings:		00:00:00.1682386
		///		TileBlock:	00:00:00.1255588
		///			Total tiles tagged: 19638
		///		Array:		00:00:00.1491268
		///		Rings:		00:00:00.1774484
		///		TileBlock:	00:00:00.1388011
		///			Total tiles tagged: 20407
		///	
		/// TileBlock is usually the fastest, and would have some usefulness outside of generation
		///		although defining a bounds for an area to search is imprecise on a hex grid.
		/// Rings is easily the slowest, but beign able to define any point and search a defined
		///		radius around it is immensely useful.
		///	Array is dangerous as you would have to define where [0,0] and the end condition.
		/// </summary>
		/// <param name="regionType"></param>
		/// <param name="distanceToCheck"></param>
		/// <returns></returns>
		public List<List<Vector3Int>> GetRegions(TerrainType regionType, int distanceToCheck = int.MaxValue)
		{
			return GetRegionsTilesBlockSearch(regionType);
			//return GetRegionsRingSearch(regionType, distanceToCheck);
		}

		private List<List<Vector3Int>> GetRegionsTilesBlockSearch(TerrainType regionType)
		{
			List<List<Vector3Int>> regions = new List<List<Vector3Int>>();
			Dictionary<Vector3Int, bool> mapFlags = new Dictionary<Vector3Int, bool>();

			TileBase[] allTiles = tilemap.GetTilesBlock(tilemap.cellBounds);

			foreach (TileBase tb in allTiles)
			{
				TerrainTile tt = (TerrainTile)tb;
				if (tt == null || (mapFlags.TryGetValue(tt.coordinates, out bool searched) == true && searched == true) || tt.terrainType != regionType)
					continue;
				List<Vector3Int> newRegion = GetRegionTiles(tt.coordinates);
				foreach (Vector3Int regionCoord in newRegion)
					mapFlags[regionCoord] = true;

				regions.Add(newRegion);
			}

			return regions;
		}

		private List<List<Vector3Int>> GetRegionsRingSearch(TerrainType regionType, int distanceToCheck)
		{
			List<List<Vector3Int>> regions = new List<List<Vector3Int>>();
			Dictionary<Vector3Int, bool> mapFlags = new Dictionary<Vector3Int, bool>();

			Vector3Int centerCoords = Vector3Int.zero;
			bool ringEmpty = false;
			int i = 0;

			while (i <= distanceToCheck && !ringEmpty)
			{
				ringEmpty = true;
				List<Vector3Int> ring = HexTools.GetRing(centerCoords, i++);
				foreach (Vector3Int ringTile in ring)
				{
					TerrainTile ttb = GetTile(ringTile);
					if (ttb == null)
						continue;

					ringEmpty = false;

					if (mapFlags.TryGetValue(ringTile, out bool searched) == true && searched == true)
						continue;

					if (ttb.terrainType == regionType)
					{
						List<Vector3Int> newRegion = GetRegionTiles(ringTile);
						foreach (Vector3Int coord in newRegion)
							mapFlags[coord] = true;

						regions.Add(newRegion);
					}
				}
			}

			return regions;
		}

		private List<Vector3Int> GetRegionTiles(Vector3Int startCoordinates)
		{
			List<Vector3Int> tiles = new List<Vector3Int>();
			Dictionary<Vector3Int, bool> mapFlags = new Dictionary<Vector3Int, bool>();

			TerrainType tileType = GetTile(startCoordinates).terrainType;

			Queue<Vector3Int> queue = new Queue<Vector3Int>();
			queue.Enqueue(startCoordinates);

			mapFlags[startCoordinates] = true;

			while (queue.Count > 0)
			{
				Vector3Int checkTile = queue.Dequeue();
				tiles.Add(checkTile);
				TerrainTile[] surroundingTiles = GetSurroundingTiles(checkTile);

				foreach (TerrainTile neighbour in surroundingTiles)
				{
					if (neighbour == null)
						continue;

					if (mapFlags.TryGetValue(neighbour.coordinates, out bool searched) == false && neighbour.terrainType == tileType)
						queue.Enqueue(neighbour.coordinates);

					mapFlags[neighbour.coordinates] = true;
				}
			}

			return tiles;
		}


		public TerrainTile[] GetSurroundingTiles(Vector3Int tileCoords)
		{
			TerrainTile[] tiles = new TerrainTile[6];
			tiles[0] = GetAdjacentTileTo(tileCoords, Cardinality.N);
			tiles[1] = GetAdjacentTileTo(tileCoords, Cardinality.NE);
			tiles[2] = GetAdjacentTileTo(tileCoords, Cardinality.SE);
			tiles[3] = GetAdjacentTileTo(tileCoords, Cardinality.S);
			tiles[4] = GetAdjacentTileTo(tileCoords, Cardinality.SW);
			tiles[5] = GetAdjacentTileTo(tileCoords, Cardinality.NW);
			return tiles;
		}

		private TerrainTile GetAdjacentTileTo(Vector3Int coordinates, Cardinality cardinality)
		{
			switch (cardinality)
			{
				case Cardinality.N:
					return GetTile(coordinates + new Vector3Int(1, 0, 0));
				case Cardinality.NE: // !!
					return GetTile(coordinates + new Vector3Int(System.Math.Abs(coordinates.y) % 2, 1, 0));
				case Cardinality.SE:
					return GetTile(coordinates + new Vector3Int(System.Math.Abs(coordinates.y) % 2 - 1, 1, 0));
				case Cardinality.S:
					return GetTile(coordinates + new Vector3Int(-1, 0, 0));
				case Cardinality.SW:
					return GetTile(coordinates + new Vector3Int(System.Math.Abs(coordinates.y) % 2 - 1, -1, 0));
				case Cardinality.NW:
					return GetTile(coordinates + new Vector3Int(System.Math.Abs(coordinates.y) % 2, -1, 0));
			}

			return null;
		}


		public void RevealArea(Vector3Int revealCenter, int radius)
		{
			Debug.LogError("Map reveal has been disabled.");
			//FillMapArea(revealCenter, radius);

			//for (int i = 0; i < smoothSteps; ++i)
			//{
			//	if (!SmoothMap(revealCenter, radius))
			//		break;
			//}
		}


		public TerrainTile CreateAndSetTile(Vector3Int coord, TerrainTile tilePrefab, TerrainTile originalTile = null)
		{
			TerrainTile newTile = Instantiate(tilePrefab);
			newTile.coordinates = coord;

			if (debugCells)
			{
				if (originalTile == null)
				{
					GameObject newObj = Instantiate(locTextPrefab);
					Vector3 worldPoint = tilemap.CellToWorld(coord);
					newObj.transform.position = worldPoint;
					newObj.transform.SetParent(textHolder, true);
					TextMeshPro text = newObj.GetComponent<TextMeshPro>();
					text.transform.localScale = tilemapScale * .5f;
					text.name = coord.ToString();
					text.SetText(coord.ToString() + "\n" + HexTools.OffsetToCube(coord));
					newTile.text = text;
				}
				else
				{
					newTile.text = originalTile.text;
					newTile.text.SetText(coord.ToString() + "\n" + HexTools.OffsetToCube(coord));
				}
			}

			if (originalTile != null)
			{
				tilemap.SetTile(coord, null);
			}

			tilemap.SetTile(coord, newTile);
			return newTile;
		}

		
	}
}