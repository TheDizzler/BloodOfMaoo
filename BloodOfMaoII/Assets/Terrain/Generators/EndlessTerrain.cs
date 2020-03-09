using System;
using System.Collections.Generic;
using UnityEngine;
using static AtomosZ.BoMII.Terrain.TerrainChunk;

namespace AtomosZ.BoMII.Terrain.Generators
{
	public class EndlessTerrain : MonoBehaviour
	{
		public const float scale = .25f;
		public const float viewerMoveThresholdForChunkUpdate = 25f;
		public const float sqrViewerMoveThresholdForChunkUpdate
			= viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

		public static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();
		public static float DMax, DMin;
		public static float maxViewDist;
		public static Vector2 viewerPosition;
		public static MapGenerator mapGenerator;
		

		public Transform viewer;
		public LODInfo[] detailLevels;

		[SerializeField] private GameObject terrainChunkPrefab = null;
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
						
						TerrainChunk newChunk = Instantiate(terrainChunkPrefab).GetComponent<TerrainChunk>();
						newChunk.Initialize(
							viewedChunkCoord, chunkSize, detailLevels, mapChunkParent, mapMaterial);
						terrainChunkDic.Add(viewedChunkCoord, newChunk);
					}
				}
			}
		}

	}
}