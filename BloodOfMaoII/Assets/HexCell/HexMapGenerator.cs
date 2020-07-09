﻿using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using static AtomosZ.BoMII.Terrain.TerrainTileBase;

namespace AtomosZ.BoMII.Terrain
{
	/// <summary>
	/// Unity uses Offset Coordinates for hex maps.
	/// </summary>
	public class HexMapGenerator : MonoBehaviour
	{
		public static float outerRadius = 10f;
		public static float innerRadius = outerRadius * 0.866025404f;

		public string stableSeed;
		public bool useRandomSeed;
		public string randomSeed;
		public int width;
		public int height;
		[Tooltip("Minimum neighbours 4:\n\t40 to 45: Large caverns.\n\t45 to 50: caves." +
			"\n\t50 to 55: small caves & rooms.\n\t55 to 60: small rooms." +
			"\n\tValues below 30 are too open and above 60 are to filled.")]
		[Range(10, 100)]
		public int randomFillPercent;
		public int smoothSteps = 5;
		[Tooltip("Standard square Cells: Range(3,6)" +
			"\nA value of 4 is standard. A value of 5 with randomFillPercent" +
			"around 63 generates very eerie-looking platforms after 6 or 7 smooth steps. (try seed = Test Seed)" +
			"\nValues of 3 or 6 creates The Nothing." +
			"\nHex Cells: Range(2,5)\nAny value other than 3 is really unstable.")]
		[Range(2, 5)]
		public int minNeighboursToTurnBlack = 3;
		[Tooltip("Minimum size of wall or pillar that can exist (will be filled in with empty space)")]
		public int wallThresholdSize = 15;
		[Tooltip("Minimum size of room that can exist (will be filled in with wall)")]
		public int roomThresholdSize = 50;
		[Tooltip("Size of passages that connect rooms.")]
		public int passageSize = 1;
		public int borderSize = 1;
		[Tooltip("Whether the border can be culled in the smoothing step.")]
		public bool keepBorder;
		[Tooltip("For heavy debugging of cells. Generation is slow. Use small maps.")]
		public bool debugCells;

		public TerrainTileBase blackTile;
		public TerrainTileBase whiteTile;
		public GameObject locTextPrefab;
		public Transform textHolder;


		public Tilemap tilemap = null;

		private Camera cam = null;
		private Vector3 cellsize;


		void Start()
		{
			cam = Camera.main;
			cellsize = tilemap.cellSize;
			ClearMap();
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

				TileBase tile = tilemap.GetTile(tilepoint);
				if (tile != null)
				{
					if (Input.GetMouseButtonDown(0))
						Debug.Log(tilepoint);
				}
				else
				{
					if (Input.GetMouseButtonDown(0))
					{
						Debug.Log("Tilepoint: " + tilepoint);
					}
				}
			}

		}


		public bool IsMapExist()
		{
			return true;
		}

		public TerrainTileBase GetTile(Vector3Int worldCoords)
		{
			return tilemap.GetTile<TerrainTileBase>(worldCoords);
		}

		public TerrainTileBase GetTile(int x, int y)
		{
			return tilemap.GetTile<TerrainTileBase>(GetOffsetCoords(x, y));
		}

		public void ClearMap()
		{
			tilemap.ClearAllTiles();
			for (int i = textHolder.childCount - 1; i >= 0; --i)
			{
				if (Application.isPlaying)
					Destroy(textHolder.GetChild(i).gameObject);
				else
					DestroyImmediate(textHolder.GetChild(i).gameObject);
			}
		}


		public void GenerateMap()
		{
			tilemap.ClearAllTiles();
			for (int i = textHolder.childCount - 1; i >= 0; --i)
			{
				if (Application.isPlaying)
					Destroy(textHolder.GetChild(i).gameObject);
				else
					DestroyImmediate(textHolder.GetChild(i).gameObject);
			}

			RandomFillMap();
			if (debugCells)
				DebugWallCount();

			for (int i = 0; i < smoothSteps; ++i)
			{
				if (!SmoothMap())
					break;
			}

			ProcessMap();
		}


		private void ProcessMap()
		{
			List<List<Vector3Int>> floorRegions = GetRegions(TerrainTileBase.TerrainType.White);
			List<List<Vector3Int>> wallRegions = GetRegions(TerrainTileBase.TerrainType.Black);


		}

		private List<List<Vector3Int>> GetRegions(TerrainTileBase.TerrainType regionType)
		{
			List<List<Vector3Int>> regions = new List<List<Vector3Int>>();
			Dictionary<Vector3Int, bool> mapFlags = new Dictionary<Vector3Int, bool>();

			for (int x = 0; x < width; ++x) // need to change this to a spiralling check starting at 0,0
			{
				for (int y = 0; y < height; ++y)
				{
					Vector3Int coords = GetOffsetCoords(x, y);
					TerrainTileBase tile = GetTile(coords);
					if ((mapFlags.TryGetValue(coords, out bool searched) == true && searched == true) || tile.type != regionType)
						continue;
					List<Vector3Int> newRegion = GetRegionTiles(coords);
					foreach (Vector3Int regionCoord in newRegion)
						mapFlags[regionCoord] = true;
					regions.Add(newRegion);
				}
			}

			return regions;
		}

		private List<Vector3Int> GetRegionTiles(Vector3Int startCoordinates)
		{
			List<Vector3Int> tiles = new List<Vector3Int>();
			Dictionary<Vector3Int, bool> mapFlags = new Dictionary<Vector3Int, bool>();

			TerrainTileBase.TerrainType tileType = GetTile(startCoordinates).type;

			Queue<Vector3Int> queue = new Queue<Vector3Int>();
			queue.Enqueue(startCoordinates); // these are NOT offset, ie not world coords (start from 0,0, positive only)

			mapFlags[startCoordinates] = true;

			while (queue.Count > 0)
			{
				Vector3Int checkTile = queue.Dequeue();
				tiles.Add(checkTile);

				TerrainTileBase[] surroundingTiles = GetSurroundingTiles(checkTile);
				foreach (TerrainTileBase neighbour in surroundingTiles)
				{
					if (neighbour == null)
						continue;

					if (mapFlags.TryGetValue(neighbour.coordinates, out bool searched) == false && neighbour.type == tileType)
					{
						queue.Enqueue(neighbour.coordinates);
						neighbour.text.SetText(tileType.ToString());
					}

					
					mapFlags[neighbour.coordinates] = true;
				}
			}

			return tiles;
		}


		public bool SmoothMap(bool regenerateMeshImmediately = false)
		{
			TerrainTileBase.TerrainType[,] newMap = new TerrainTileBase.TerrainType[width, height];
			for (int x = 0; x < width; ++x)
			{
				for (int y = 0; y < height; ++y)
				{
					if (keepBorder
						&& y == 0 || x == 0 || x == width - 1 || y == height - 1)
					{
						newMap[x, y] = (int)TerrainTileBase.TerrainType.Black;
						continue;
					}

					Vector3Int coords = GetOffsetCoords(x, y);
					TerrainTileBase centerTile = tilemap.GetTile<TerrainTileBase>(coords);

					if (centerTile == null)
					{
						Debug.LogError("no tile: " + coords + " (x: " + x + "y: " + y + ")");
						continue;
					}

					int wallCount = 0;
					TerrainTileBase[] surroundingTiles = GetSurroundingTiles(centerTile.coordinates);
					foreach (TerrainTileBase tile in surroundingTiles)
					{
						if (tile == null || tile.type == TerrainTileBase.TerrainType.Black)
							++wallCount;
					}


					if (wallCount > minNeighboursToTurnBlack)
					{
						newMap[x, y] = TerrainTileBase.TerrainType.Black;
					}
					else if (wallCount < minNeighboursToTurnBlack)
					{
						newMap[x, y] = TerrainTileBase.TerrainType.White;
					}
					else
						newMap[x, y] = centerTile.type;
				}
			}

			bool changesMade = false;
			for (int x = 0; x < width; ++x)
			{
				for (int y = 0; y < height; ++y)
				{
					Vector3Int coords = GetOffsetCoords(x, y);
					TerrainTileBase tile = tilemap.GetTile<TerrainTileBase>(coords);
					if (newMap[x, y] != tile.type)
					{
						if (tile.type != TerrainTileBase.TerrainType.Black)
							tile = CreateAndSetTile(coords, blackTile, tile);
						else
							tile = CreateAndSetTile(coords, whiteTile, tile);

						changesMade = true;
					}
				}
			}

			if (debugCells && changesMade)
			{
				DebugWallCount();
			}

			return changesMade;
		}


		private void DebugWallCount()
		{
			for (int x = 0; x < width; ++x)
			{
				for (int y = 0; y < height; ++y)
				{
					Vector3Int coords = GetOffsetCoords(x, y);
					TerrainTileBase centerTile = tilemap.GetTile<TerrainTileBase>(coords);

					if (centerTile == null)
					{
						Debug.Log("no tile: " + coords + " (x: " + x + "y: " + y + ")");
						continue;
					}

					int wallCount = 0;
					TerrainTileBase[] surroundingTiles = GetSurroundingTiles(centerTile.coordinates);
					foreach (TerrainTileBase tile in surroundingTiles)
					{
						if (tile == null || tile.type == TerrainTileBase.TerrainType.Black)
							++wallCount;
					}

					centerTile.text.SetText(centerTile.coordinates + "\nWallCount: " + wallCount);
				}
			}
		}


		private TerrainTileBase[] GetSurroundingTiles(Vector3Int tileCoords)
		{
			TerrainTileBase[] tiles = new TerrainTileBase[6];
			tiles[0] = GetAdjacentTileTo(tileCoords, Cardinality.N);
			tiles[1] = GetAdjacentTileTo(tileCoords, Cardinality.NE);
			tiles[2] = GetAdjacentTileTo(tileCoords, Cardinality.SE);
			tiles[3] = GetAdjacentTileTo(tileCoords, Cardinality.S);
			tiles[4] = GetAdjacentTileTo(tileCoords, Cardinality.SW);
			tiles[5] = GetAdjacentTileTo(tileCoords, Cardinality.NW);
			return tiles;
		}

		private TerrainTileBase GetAdjacentTileTo(Vector3Int coordinates, Cardinality cardinality)
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


		private Vector3Int GetAdjacentTileCoordinatesTo(Vector3Int sourceTileCoords, Cardinality cardinality)
		{
			Vector3Int tileLoc = sourceTileCoords;

			switch (cardinality)
			{
				case Cardinality.N:
					tileLoc.x += 1;
					break;
				case Cardinality.NE:
					tileLoc.y += 1;
					break;
				case Cardinality.SE:
					tileLoc.y += 1;
					tileLoc.x -= 1;
					break;
				case Cardinality.S:
					tileLoc.x -= 1;
					break;
				case Cardinality.SW:
					tileLoc.y -= 1;
					tileLoc.x -= 1;
					break;
				case Cardinality.NW:
					tileLoc.y -= 1;
					break;
			}

			return tileLoc;
		}


		private void RandomFillMap()
		{
			if (useRandomSeed)
				randomSeed = Time.time.ToString();
			else
				randomSeed = stableSeed;

			System.Random rng = new System.Random(randomSeed.GetHashCode());

			for (int x = 0; x < width; ++x)
			{
				for (int y = 0; y < height; ++y)
				{
					Vector3Int coord = GetOffsetCoords(x, y);

					if (y == 0 || x == 0 || y == height - 1 || x == width - 1
						|| rng.Next(0, 100) < randomFillPercent)
					{
						CreateAndSetTile(coord, blackTile);
					}
					else
					{
						CreateAndSetTile(coord, whiteTile);
					}
				}
			}
		}

		private TerrainTileBase CreateAndSetTile(Vector3Int coord, TerrainTileBase tilePrefab, TerrainTileBase originalTile = null)
		{
			TerrainTileBase newTile = Instantiate(tilePrefab);
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
					text.name = coord.ToString();
					text.SetText(coord.ToString() + "\nWallCount: " + 0);
					newTile.text = text;
				}
				else
				{
					newTile.text = originalTile.text;
				}
			}

			tilemap.SetTile(coord, newTile);
			return newTile;
		}


		private Vector3Int GetOffsetCoords(int x, int y)
		{
			return new Vector3Int(
						Mathf.CeilToInt(-height * .5f) + y,
						Mathf.CeilToInt(-width * .5f) + x, 0);
		}

		private bool IsInMapRange(int x, int y)
		{
			return x >= 0 && y >= 0 && x < width && y < height;
		}

	}
}