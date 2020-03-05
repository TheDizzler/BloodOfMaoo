using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static AtomosZ.BoMII.Terrain.Generators.Noise;

namespace AtomosZ.BoMII.Terrain.Generators
{
	public class MapGenerator : MonoBehaviour
	{
		public enum DrawMode { NoiseMap, ColorMap, Mesh, FalloffMap, HexGrid };

		public const int mapChunkSize = 241;

		public NormalizeMode normalizeMode;
		public DrawMode drawMode;

		public bool autoUpdate = true;
		public bool useFalloff;

		[Range(0, 6)]
		[SerializeField] private int editorPrefiewLevelOfDetail = 0;
		[Range(1, 100)]
		[SerializeField] private float noiseScale = 1;
		[Range(1, 60)]
		[SerializeField] private int octaves = 1;
		[Range(0, 1)]
		[SerializeField] private float persistance = 1;
		[SerializeField] private float lacunarity = 1;
		[SerializeField] private int seed = 1;
		[SerializeField] private Vector2 offset = new Vector2();
		[SerializeField] private float meshHeighMultiplier = 1;
		[SerializeField] private AnimationCurve heightMapCurve = null;
		[SerializeField] private TerrainType[] regions = null;

		private float[,] falloffMap;

		private Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
		private Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

		public void Awake()
		{
			falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
		}

#if UNITY_EDITOR
		public void DrawMapInEditor()
		{
			falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
			if (Application.isPlaying)
			{
				GetComponent<EndlessTerrain>().RefreshMap();
			}
			else
			{
				MapData mapData = GenerateMapData(Vector2.zero);
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
								mapData.heightMap, meshHeighMultiplier, heightMapCurve, editorPrefiewLevelOfDetail),
							TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
						break;
					case DrawMode.FalloffMap:
						display.DrawTexture(
							TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
						break;
					case DrawMode.HexGrid:

						break;
				}
			}
		}
#endif


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

		public void RequestMapData(Vector2 center, Action<MapData> callback)
		{
			ThreadStart threadStart = delegate
			{
				MapDataThread(center, callback);
			};

			new Thread(threadStart).Start();
		}

		private void MapDataThread(Vector2 center, Action<MapData> callback)
		{
			MapData mapData = GenerateMapData(center);
			lock (mapDataThreadInfoQueue)
			{
				mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
			}
		}

		public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
		{
			ThreadStart threadStart = delegate
			{
				MeshDataThread(mapData, lod, callback);
			};

			new Thread(threadStart).Start();
		}




		private void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
		{
			MeshData meshData = MeshGenerator.GenerateTerrainMesh(
				mapData.heightMap, meshHeighMultiplier, heightMapCurve, lod);
			lock (meshDataThreadInfoQueue)
			{
				meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
			}
		}

		private MapData GenerateMapData(Vector2 center)
		{
			// calculate the offsets based on the tile position
			float[,] noiseMap = Noise.GenerateNoiseMap(
				mapChunkSize, mapChunkSize, seed, noiseScale,
				octaves, persistance, lacunarity, center + offset, normalizeMode);

			//tilemap.ClearAllTiles();
			//int halfMapWidth = mapChunkSize / 2;
			//int halfMapHeight = mapChunkSize / 2;

			Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
			for (int y = 0; y < mapChunkSize; ++y)
			{
				for (int x = 0; x < mapChunkSize; ++x)
				{
					if (useFalloff)
					{
						noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
					}
					float currentHeight = noiseMap[x, y];
					for (int i = 0; i < regions.Length; ++i)
					{
						if (currentHeight >= regions[i].height)
						{
							colorMap[y * mapChunkSize + x] = regions[i].color;
						}
						else
						{
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