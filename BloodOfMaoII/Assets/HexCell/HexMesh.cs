using System.Collections.Generic;
using AtomosZ.BoMII.Terrain;
using AtomosZ.BoMII.Terrain.Generation;
using UnityEngine;
using UnityEngine.Tilemaps;
using static AtomosZ.BoMII.Terrain.HexTools;

public class HexMesh : MonoBehaviour
{
	public Tilemap tilemap;
	public TerrainTile testTile;
	public int mapTileWidth, mapTileHeight;
	public NoiseSettings noiseSettings;

	private float hexHeight;
	private float hexWidth;


	public void MakeMesh()
	{
		hexHeight = Vector3.Distance(
			tilemap.CellToWorld(HexTools.GetAdjacentTileOffset(Vector3Int.zero, Cardinality.N)),
			tilemap.CellToWorld(new Vector3Int(0, 0, 0)));

		float diagonalDist = Vector3.Distance(
			tilemap.CellToWorld(Vector3Int.zero),
			tilemap.CellToWorld(HexTools.GetAdjacentTileOffset(Vector3Int.zero, Cardinality.NE)));

		hexWidth = Mathf.Sqrt(diagonalDist * diagonalDist - Mathf.Pow(hexHeight * .5f, 2)) * 4 / 3;

		tilemap.ClearAllTiles();

		Vector3Int[] positions = new Vector3Int[mapTileWidth * mapTileHeight];
		TileBase[] tileArray = new TileBase[positions.Length];

		for (int index = 0; index < positions.Length; index++)
		{
			positions[index] = new Vector3Int(index % mapTileWidth, index / mapTileHeight, 0);
			tileArray[index] = testTile;
		}
		tilemap.SetTiles(positions, tileArray);

		float[,] noiseMap = Noise.GenerateNoiseMap(4 * mapTileWidth, 3 * mapTileHeight, noiseSettings, Vector2.zero);
		MeshIt(positions, noiseMap);
	}


	public TerrainTile GetTile(Vector3Int offsetGridCoords)
	{
		return tilemap.GetTile<TerrainTile>(offsetGridCoords);
	}

	//private void MeshIt(List<TerrainTile> tiles, float[,] noiseMap)
	private void MeshIt(Vector3Int[] tiles, float[,] noiseMap)
	{
		Vector3 toTopLeft = -Vector3.right * .25f * hexWidth + Vector3.up * .5f * hexHeight;
		Vector3 toTopRight = Vector3.right * .25f * hexWidth + Vector3.up * .5f * hexHeight;
		Vector3 toRight = Vector3.right * .5f * hexWidth;

		Dictionary<Vector3, int> vertexCodex = new Dictionary<Vector3, int>();
		int[] triangles = new int[tiles.Length * 12];
		int vertexIndex = 0;
		int triangleIndex = 0;
		float minX = float.MaxValue;
		float maxX = float.MinValue;
		float minY = float.MaxValue;
		float maxY = float.MinValue;

		//foreach (TerrainTile tile in tiles)
		foreach (Vector3Int tile in tiles)
		{
			Vector3 tileCenter = tilemap.GetCellCenterWorld(tile);

			Vector3 topLeftCorner = tileCenter + toTopLeft;
			Vector3 topRightCorner = tileCenter + toTopRight;
			Vector3 rightCorner = tileCenter + toRight;
			Vector3 bottomRightCorner = tileCenter - toTopLeft;
			Vector3 bottomLeftCorner = tileCenter - toTopRight;
			Vector3 leftCorner = tileCenter - toRight;

			minX = Mathf.Min(minX, leftCorner.x);
			maxX = Mathf.Max(maxX, rightCorner.x);
			minY = Mathf.Min(minY, bottomRightCorner.y);
			maxY = Mathf.Max(maxY, topRightCorner.y);

			if (!vertexCodex.TryGetValue(topLeftCorner, out int tlIndex))
			{
				tlIndex = vertexIndex++;
				vertexCodex[topLeftCorner] = tlIndex;
			}

			if (!vertexCodex.TryGetValue(topRightCorner, out int trIndex))
			{
				trIndex = vertexIndex++;
				vertexCodex[topRightCorner] = trIndex;
			}

			if (!vertexCodex.TryGetValue(rightCorner, out int rIndex))
			{
				rIndex = vertexIndex++;
				vertexCodex[rightCorner] = rIndex;
			}

			if (!vertexCodex.TryGetValue(bottomRightCorner, out int brIndex))
			{
				brIndex = vertexIndex++;
				vertexCodex[bottomRightCorner] = brIndex;
			}

			if (!vertexCodex.TryGetValue(bottomLeftCorner, out int blIndex))
			{
				blIndex = vertexIndex++;
				vertexCodex[bottomLeftCorner] = blIndex;
			}

			if (!vertexCodex.TryGetValue(leftCorner, out int lIndex))
			{
				lIndex = vertexIndex++;
				vertexCodex[leftCorner] = lIndex;
			}


			triangles[triangleIndex++] = tlIndex;
			triangles[triangleIndex++] = trIndex;
			triangles[triangleIndex++] = brIndex;
			triangles[triangleIndex++] = trIndex;
			triangles[triangleIndex++] = rIndex;
			triangles[triangleIndex++] = brIndex;
			triangles[triangleIndex++] = brIndex;
			triangles[triangleIndex++] = blIndex;
			triangles[triangleIndex++] = tlIndex;
			triangles[triangleIndex++] = blIndex;
			triangles[triangleIndex++] = lIndex;
			triangles[triangleIndex++] = tlIndex;
		}

		Vector3[] vertices = new Vector3[vertexCodex.Keys.Count];

		int mapX = noiseMap.GetLength(0);
		int mapY = noiseMap.GetLength(1);

		foreach (var kvp in vertexCodex)
		{
			Vector3 pos = kvp.Key;
			float percentX = Mathf.InverseLerp(minX, maxX, pos.x);
			float percentY = Mathf.InverseLerp(minY, maxY, pos.y);
			int x = Mathf.RoundToInt(percentX * (mapX - 1));
			int y = Mathf.RoundToInt(percentY * (mapY - 1));

			vertices[kvp.Value] = pos + new Vector3(0, 0, noiseMap[x, y] * 10);
		}

		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateBounds();

		GetComponent<MeshFilter>().sharedMesh = mesh;
	}


	public class MeshData
	{
		public Vector3[] vertices;
		public int[] triangles;

	}
}
