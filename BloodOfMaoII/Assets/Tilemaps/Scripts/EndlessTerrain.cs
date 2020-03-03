using System.Collections.Generic;
using UnityEngine;

namespace AtomosZ.BoMII.Terrain.Generators
{
	public class EndlessTerrain : MonoBehaviour
	{
		public const float maxViewDist = 225;
		public static Vector2 viewerPosition;
		

		public Transform viewer;

		[SerializeField] private Transform mapChunkParent = null;
		private int chunkSize;
		private int chunksVisibleInViewDist;
		private Dictionary<Vector2, TerrainChunk> terrainChunkDic = new Dictionary<Vector2, TerrainChunk>();
		private List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();


		public void Start()
		{
			chunkSize = MapGenerator.mapChunkSize - 1;
			chunksVisibleInViewDist = Mathf.RoundToInt(maxViewDist / chunkSize);
		}


		public void Update()
		{
			viewerPosition = new Vector2(viewer.position.x, viewer.position.y);
			UpdateVisibleChunks();
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
					Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
					if (terrainChunkDic.TryGetValue(viewedChunkCoord, out TerrainChunk chunk))
					{
						chunk.UpdateTerrainChunk();
						if (chunk.IsVisible())
							terrainChunksVisibleLastUpdate.Add(chunk);
					}
					else
					{
						TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, chunkSize, mapChunkParent);
						terrainChunkDic.Add(viewedChunkCoord, newChunk);
						terrainChunksVisibleLastUpdate.Add(newChunk);
					}
				}
			}
		}

		public class TerrainChunk
		{
			private GameObject meshObject;
			private Vector2 position;
			private Bounds bounds;


			public TerrainChunk(Vector2 coords, int size, Transform parent)
			{
				position = coords * size;
				bounds = new Bounds(position, Vector2.one * size);
				Vector3 posV3 = new Vector3(position.x, position.y, 0);

				meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
				meshObject.transform.parent = parent;
				meshObject.transform.position = posV3;
				meshObject.transform.localScale = Vector3.one * size / 10f;
				SetVisible(false);
			}

			public void UpdateTerrainChunk()
			{
				float viewerDistFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
				bool visible = viewerDistFromNearestEdge <= maxViewDist;
				SetVisible(visible);
			}

			public void SetVisible(bool visible)
			{
				meshObject.SetActive(visible);
			}

			public bool IsVisible()
			{
				return meshObject.activeSelf;
			}
		}
	}
}