using System;
using System.Collections.Generic;
using AtomosZ.BoMII.Terrain.Generators;
using UnityEngine;
using static AtomosZ.BoMII.Terrain.Generators.EndlessTerrain;
using static FalloffGenerator;
using Random = UnityEngine.Random;

namespace AtomosZ.BoMII.Terrain
{
	public class TerrainChunk : MonoBehaviour
	{
		public static readonly float[,] ZeroFalloffMap = new float[MapGenerator.mapChunkSize, MapGenerator.mapChunkSize];

		private Vector2 position;
		private Bounds bounds;
		private MeshRenderer meshRenderer;
		private MeshFilter meshFilter;
		private LODInfo[] detailLevels;
		private LODMesh[] lodMeshes;
		private MapData mapData;
		private bool mapDataReceived;
		private int previousLODIndex = -1;
		private List<FalloffSide> falloffSides = new List<FalloffSide>();


		public void Initialize(Vector2 coords, int size, LODInfo[] detailLvls, Transform parent, Material material)
		{
			detailLevels = detailLvls;
			position = coords * size;
			bounds = new Bounds(position, Vector2.one * size);
			Vector3 posV3 = new Vector3(position.x, 0, position.y);

			this.name = "Terrain Chunk " + position;
			transform.position = posV3 * scale;
			transform.SetParent(parent, false);
			transform.localScale = Vector3.one * scale;

			meshRenderer = GetComponent<MeshRenderer>();
			meshRenderer.material = material;
			meshFilter = GetComponent<MeshFilter>();
			SetVisible(false);

			lodMeshes = new LODMesh[detailLevels.Length];
			for (int i = 0; i < detailLevels.Length; ++i)
			{
				lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
			}

			mapGenerator.RequestMapData(position, OnMapDataReceived);
		}


		public void SetFalloffMap(FalloffSide newFalloffSide)
		{
			if (newFalloffSide == FalloffSide.None)
			{
				falloffSides.Clear();
			}
			else
			{
				if (!falloffSides.Contains(newFalloffSide))
					falloffSides.Add(newFalloffSide);
			}

			mapData.falloffMap = null;
			RefreshFalloffMap();

			Texture2D texture = TextureGenerator.TextureFromColorMap(
				mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
			meshRenderer.material.mainTexture = texture;

			UpdateTerrainChunk();
		}


		public void UpdateTerrainChunk()
		{
			if (!mapDataReceived || meshRenderer == null)
				return;
			float viewerDistFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
			bool visible = viewerDistFromNearestEdge <= maxViewDist;

			if (visible)
			{
				int lodIndex = 0;
				for (int i = 0; i < detailLevels.Length - 1; ++i)
				{
					if (viewerDistFromNearestEdge > detailLevels[i].visibleDistThreshold)
						lodIndex = i + 1;
					else
						break;
				}

				if (lodIndex != previousLODIndex)
				{
					LODMesh lodMesh = lodMeshes[lodIndex];
					if (lodMesh.hasMesh)
					{
						previousLODIndex = lodIndex;
						meshFilter.mesh = lodMesh.mesh;
					}
					else if (!lodMesh.hasRequestedMesh)
						lodMesh.RequestMesh(mapData);
				}

				terrainChunksVisibleLastUpdate.Add(this);
			}

			SetVisible(visible);
		}

		public void SetVisible(bool visible)
		{
			gameObject.SetActive(visible);
		}

		public bool IsVisible()
		{
			return gameObject.activeSelf;
		}

		public void DestroySelf()
		{
			Destroy(gameObject);
		}

		private void OnMapDataReceived(MapData mapData)
		{
			if (meshRenderer == null)
				return;
			this.mapData = mapData;
			mapDataReceived = true;

			if (mapData.falloffMap != null) // then it's an island
				RefreshFalloffMap();

			Texture2D texture = TextureGenerator.TextureFromColorMap(
				mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
			meshRenderer.material.mainTexture = texture;

			UpdateTerrainChunk();
		}

		private void RefreshFalloffMap()
		{
			MapGenerator mapGen = GetComponentInParent<MapGenerator>();
			TerrainType[] regions = mapGen.GetRegions();

			if (falloffSides.Count == 0 && mapData.falloffMap == null)
			{// should just be a plain old chunk
				mapData.falloffMap = ZeroFalloffMap;
			}
			else if (mapData.falloffMap == null)
			{
				if (mapData.falloffConstantA < 0)
				{
					mapData.falloffConstantA = Random.Range(.5f, MapGenerator.maxFalloffConstantA);
					mapData.falloffConstantB = Random.Range(.5f, MapGenerator.maxFalloffConstantB);
				}

				if (ContainsAllSides())
					mapData.falloffMap = FalloffGenerator.GenerateIslandFalloffMap(
						MapGenerator.mapChunkSize, mapData.falloffConstantA, mapData.falloffConstantB, true);
				else if (falloffSides.Count > 0)
					mapData.falloffMap = FalloffGenerator.GenerateContinentFalloffMap(
						MapGenerator.mapChunkSize, mapData.falloffConstantA, mapData.falloffConstantB, falloffSides);

			}

			float[,] alteredHeightMap = new float[MapGenerator.mapChunkSize, MapGenerator.mapChunkSize];
			Color[] colorMap = new Color[MapGenerator.mapChunkSize * MapGenerator.mapChunkSize];
			for (int y = 0; y < MapGenerator.mapChunkSize; ++y)
			{
				for (int x = 0; x < MapGenerator.mapChunkSize; ++x)
				{
					alteredHeightMap[x, y] 
						= Mathf.Clamp01(mapData.baseHeightMap[x, y] - mapData.falloffMap[x, y]);

					float currentHeight = alteredHeightMap[x, y];
					for (int i = 0; i < regions.Length; ++i)
					{
						if (currentHeight >= regions[i].height)
						{
							colorMap[y * MapGenerator.mapChunkSize + x] = regions[i].color;
						}
						else
						{
							//tilemap.SetTile(new Vector3Int( y - halfMapHeight, x - halfMapWidth, 0), terrainTiles[i]);
							break;
						}
					}
				}
			}

			mapData.colorMap = colorMap;
			mapData.alteredHeightMap = alteredHeightMap;
			for (int i = 0; i < detailLevels.Length; ++i)
			{
				if (lodMeshes[i].hasMesh)
					lodMeshes[i].mesh = MeshGenerator.GenerateTerrainMesh(
						mapData.alteredHeightMap, mapGen.GetMeshHeightMultiplier(),
						mapGen.GetHeightMapCurve(), lodMeshes[i].lod).CreateMesh();
			}

			if (previousLODIndex > -1)
				meshFilter.mesh = lodMeshes[previousLODIndex].mesh;
		}

		private bool ContainsAllSides()
		{
			return falloffSides.Count >= 4
				&& falloffSides.Contains(FalloffSide.Bottom)
				&& falloffSides.Contains(FalloffSide.Top)
				&& falloffSides.Contains(FalloffSide.Right)
				&& falloffSides.Contains(FalloffSide.Left);
		}

		private class LODMesh
		{
			public Mesh mesh;
			public bool hasRequestedMesh;
			public bool hasMesh;
			public int lod;
			private Action updateCallback;


			public LODMesh(int levelOfDetail, Action updateCllbck)
			{
				lod = levelOfDetail;
				updateCallback = updateCllbck;
			}

			private void OnMeshDataReceived(MeshData meshData)
			{
				mesh = meshData.CreateMesh();
				hasMesh = true;

				updateCallback();
			}

			public void RequestMesh(MapData mapData)
			{
				hasRequestedMesh = true;
				mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
			}
		}

		[Serializable]
		public struct LODInfo
		{
			public int lod;
			public float visibleDistThreshold;
		}
	}
}