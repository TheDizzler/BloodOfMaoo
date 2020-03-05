using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtomosZ.BoMII.Terrain.Generators
{
	public class EndlessTerrain : MonoBehaviour
	{
		public const float scale = .25f;
		public const float viewerMoveThresholdForChunkUpdate = 25f;
		public const float sqrViewerMoveThresholdForChunkUpdate
			= viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

		public static float DMax, DMin;
		public static float maxViewDist;
		public static Vector2 viewerPosition;
		public static MapGenerator mapGenerator;

		public Transform viewer;
		public LODInfo[] detailLevels;

		[SerializeField] private Transform mapChunkParent = null;
		[SerializeField] private Material mapMaterial = null;
		[Range(0, 1)]
		[SerializeField] private float dMax = .5f;
		[Range(0, 1)]
		[SerializeField] private float dMin = .5f;

		private Vector2 viewerPositionOld;
		private int chunkSize;
		private int chunksVisibleInViewDist;
		private Dictionary<Vector2, TerrainChunk> terrainChunkDic = new Dictionary<Vector2, TerrainChunk>();
		static private List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();


		public void Start()
		{
			mapGenerator = FindObjectOfType<MapGenerator>();
			chunkSize = MapGenerator.mapChunkSize - 1;

			maxViewDist = detailLevels[detailLevels.Length - 1].visibleDistThreshold;
			chunksVisibleInViewDist = Mathf.RoundToInt(maxViewDist / chunkSize);

			UpdateVisibleChunks();
		}

		public void RefreshMap()
		{
			float halfChunk = chunkSize / 2;
			DMax = Mathf.Lerp(.5f * halfChunk, halfChunk - 1, dMax);
			DMin = Mathf.Lerp(1, .5f * halfChunk, dMin);
			Debug.Log("DMax: " + DMax + " DMin: " + DMin);
			foreach (KeyValuePair<Vector2, TerrainChunk> kvp in terrainChunkDic)
			{
				kvp.Value.DestroySelf();
			}

			terrainChunkDic.Clear();
			terrainChunksVisibleLastUpdate.Clear();
			viewerPositionOld = new Vector2(-9999, -9999);
		}

		public void Update()
		{
			viewerPosition = new Vector2(viewer.position.x, viewer.position.y) / scale;
			if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
			{
				UpdateVisibleChunks();
				viewerPositionOld = viewerPosition;
			}
		}



		private void UpdateVisibleChunks()
		{
			for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; ++i)
			{
				terrainChunksVisibleLastUpdate[i].SetVisible(false);
			}

			terrainChunksVisibleLastUpdate.Clear();

			int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
			int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

			for (int yOffset = -chunksVisibleInViewDist; yOffset <= chunksVisibleInViewDist; ++yOffset)
			{
				for (int xOffset = -chunksVisibleInViewDist; xOffset <= chunksVisibleInViewDist; ++xOffset)
				{
					Vector2 viewedChunkCoord = new Vector2(
						currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
					if (terrainChunkDic.TryGetValue(viewedChunkCoord, out TerrainChunk chunk))
					{
						chunk.UpdateTerrainChunk();
					}
					else
					{
						TerrainChunk newChunk = new TerrainChunk(
							viewedChunkCoord, chunkSize, detailLevels, mapChunkParent, mapMaterial);
						terrainChunkDic.Add(viewedChunkCoord, newChunk);
					}
				}
			}
		}

		public class TerrainChunk
		{
			private GameObject terrainObject;
			private Vector2 position;
			private Bounds bounds;
			private MeshRenderer meshRenderer;
			private MeshFilter meshFilter;
			private LODInfo[] detailLevels;
			private LODMesh[] lodMeshes;
			private MapData mapData;
			private bool mapDataReceived;
			private int previousLODIndex = -1;


			public TerrainChunk(Vector2 coords, int size, LODInfo[] detailLvls, Transform parent, Material material)
			{
				detailLevels = detailLvls;
				position = coords * size;
				bounds = new Bounds(position, Vector2.one * size);
				Vector3 posV3 = new Vector3(position.x, 0, position.y);

				terrainObject = new GameObject("Terrain Chunk " + position);
				terrainObject.transform.position = posV3 * scale;
				terrainObject.transform.SetParent(parent, false);
				terrainObject.transform.localScale = Vector3.one * scale;


				meshRenderer = terrainObject.AddComponent<MeshRenderer>();
				meshRenderer.material = material;
				meshFilter = terrainObject.AddComponent<MeshFilter>();
				SetVisible(false);

				lodMeshes = new LODMesh[detailLevels.Length];
				for (int i = 0; i < detailLevels.Length; ++i)
				{
					lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
				}

				mapGenerator.RequestMapData(position, OnMapDataReceived);
			}

			public void UpdateTerrainChunk()
			{
				if (!mapDataReceived || terrainObject == null)
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
				terrainObject.SetActive(visible);
			}

			public bool IsVisible()
			{
				return terrainObject.activeSelf;
			}

			public void DestroySelf()
			{
				Destroy(terrainObject);
			}

			private void OnMapDataReceived(MapData mapData)
			{
				if (terrainObject == null)
					return;
				this.mapData = mapData;
				mapDataReceived = true;

				Texture2D texture = TextureGenerator.TextureFromColorMap(
					mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
				meshRenderer.material.mainTexture = texture;


				UpdateTerrainChunk();
			}
		}

		private class LODMesh
		{
			public Mesh mesh;
			public bool hasRequestedMesh;
			public bool hasMesh;
			private int lod;
			Action updateCallback;


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