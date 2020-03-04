using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


namespace AtomosZ.BoMII.Terrain.Generators
{
	public class MapGenerator : MonoBehaviour
	{
		public enum DrawMode { NoiseMap, ColorMap, Mesh, HexGrid };

		public const int mapChunkSize = 241;

		public DrawMode drawMode;
		public bool autoUpdate = true;

		[Range(0, 6)]
		[SerializeField] private int levelOfDetail = 1;
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

		private Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
		private Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

#if UNITY_EDITOR
		public void DrawMapInEditor()
		{
			MapData mapData = GenerateMapData();
			MapDisplay display = GetComponent<MapDisplay>();
			switch (drawMode)
			{
				case DrawMode.NoiseMap:
					display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
					break;
				case DrawMode.ColorMap:
					display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
					break;
				case DrawMode.Mesh:
					display.DrawMesh(
						MeshGenerator.GenerateTerrainMesh(
							mapData.heightMap, meshHeighMultiplier, heightMapCurve, levelOfDetail),
						TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
					break;
				case DrawMode.HexGrid:

					break;
			}
		}
#endif

		public void RequestMapData(Action<MapData> callback)
		{
			ThreadStart threadStart = delegate
			{
				MapDataThread(callback);
			};

			new Thread(threadStart).Start();
		}

		public void RequestMeshData(MapData mapData, Action<MeshData> callback)
		{
			ThreadStart threadStart = delegate
			{
				MeshDataThread(mapData, callback);
			};

			new Thread(threadStart).Start();
		}

		public void Update()
		{
			if (mapDataThreadInfoQueue.Count > 0)
			{
				for (int i = 0; i < mapDataThreadInfoQueue.Count; ++i)
				{
					MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
					threadInfo.callback(threadInfo.parameter);
				}
			}

			if (meshDataThreadInfoQueue.Count > 0)
				{
				for (int i = 0; i < meshDataThreadInfoQueue.Count; ++i)
				{
					MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
					threadInfo.callback(threadInfo.parameter);
				}
			}
		}

		private void MapDataThread(Action<MapData> callback)
		{
			MapData mapData = GenerateMapData();
			lock (mapDataThreadInfoQueue)
			{
				mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
			}
		}

		private void MeshDataThread(MapData mapData, Action<MeshData> callback)
		{
			MeshData meshData = MeshGenerator.GenerateTerrainMesh(
				mapData.heightMap, meshHeighMultiplier, heightMapCurve, levelOfDetail);
			lock (meshDataThreadInfoQueue)
			{
				meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
			}
		}

		private MapData GenerateMapData()
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

			return new MapData(noiseMap, colorMap);
		}


		public void OnValidate()
		{
			if (lacunarity < 1)
				lacunarity = 1;
		}

		private struct MapThreadInfo<T>
		{
			public readonly Action<T> callback;
			public readonly T parameter;

			public MapThreadInfo(Action<T> callback, T parameter)
			{
				this.callback = callback;
				this.parameter = parameter;
			}
		}
	}


	public struct MapData
	{
		public float[,] heightMap;
		public Color[] colorMap;


		public MapData(float[,] heightMap, Color[] colorMap)
		{
			this.heightMap = heightMap;
			this.colorMap = colorMap;
		}
	}
}