using System;
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
	[Range(0, 100)]
	public int mapRadius;
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

		var tiles = HexTools.GetSpiral(Vector3Int.zero, mapRadius);
		TileBase[] tileArray = new TileBase[tiles.Count];


		for (int index = 0; index < tiles.Count; index++)
		{
			tileArray[index] = testTile;
		}

		tilemap.SetTiles(tiles.ToArray(), tileArray);

		if (mapRadius == 0)
			mapRadius = 1;
		float[,] noiseMap = Noise.GenerateNoiseMap(4 * mapRadius * 2, 3 * mapRadius * 2, noiseSettings, Vector2.zero);
		MeshIt(tiles.ToArray(), noiseMap);
	}


	public TerrainTile GetTile(Vector3Int offsetGridCoords)
	{
		return tilemap.GetTile<TerrainTile>(offsetGridCoords);
	}


	public enum IndexOrder
	{
		TopLeft, TopRight, Right, BottomRight, BottomLeft, Left
	}

	
	private void MeshIt(Vector3Int[] tiles, float[,] noiseMap)
	{
		Vector3 toTopLeft = -Vector3.right * .25f * hexWidth + Vector3.forward * .5f * hexHeight;
		Vector3 toTopRight = Vector3.right * .25f * hexWidth + Vector3.forward * .5f * hexHeight;
		Vector3 toRight = Vector3.right * .5f * hexWidth;

		Dictionary<Vector3Int, TileMeshData> tileVertexCodex = new Dictionary<Vector3Int, TileMeshData>();
		int[] triangleIndices = new int[tiles.Length * 12];
		int vertexIndex = 0;
		int triangleIndex = 0;
		float minX = float.MaxValue;
		float maxX = float.MinValue;
		float minY = float.MaxValue;
		float maxY = float.MinValue;

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
			minY = Mathf.Min(minY, bottomRightCorner.z);
			maxY = Mathf.Max(maxY, topRightCorner.z);


			TileMeshData tmd = new TileMeshData();
			tmd.verticeIndices = new Tuple<Vector3, int>[6];
			// look for shared vertices
			Vector3Int[] surroundingTiles = HexTools.GetSurroundingTilesOffset(tile);

			if (tileVertexCodex.TryGetValue(surroundingTiles[(int)Cardinality.N], out TileMeshData sharedData))
			{
				tmd.verticeIndices[(int)IndexOrder.TopLeft] = new Tuple<Vector3, int>(topLeftCorner, sharedData.verticeIndices[(int)IndexOrder.BottomLeft].Item2);
				tmd.verticeIndices[(int)IndexOrder.TopRight] = new Tuple<Vector3, int>(topRightCorner, sharedData.verticeIndices[(int)IndexOrder.BottomRight].Item2);
			}

			if (tileVertexCodex.TryGetValue(surroundingTiles[(int)Cardinality.NE], out sharedData))
			{
				tmd.verticeIndices[(int)IndexOrder.TopRight] = new Tuple<Vector3, int>(topRightCorner, sharedData.verticeIndices[(int)IndexOrder.Left].Item2);
				tmd.verticeIndices[(int)IndexOrder.Right] = new Tuple<Vector3, int>(rightCorner, sharedData.verticeIndices[(int)IndexOrder.BottomLeft].Item2);
			}

			if (tileVertexCodex.TryGetValue(surroundingTiles[(int)Cardinality.SE], out sharedData))
			{
				tmd.verticeIndices[(int)IndexOrder.Right] = new Tuple<Vector3, int>(rightCorner, sharedData.verticeIndices[(int)IndexOrder.TopLeft].Item2);
				tmd.verticeIndices[(int)IndexOrder.BottomRight] = new Tuple<Vector3, int>(bottomRightCorner, sharedData.verticeIndices[(int)IndexOrder.Left].Item2);
			}

			if (tileVertexCodex.TryGetValue(surroundingTiles[(int)Cardinality.S], out sharedData))
			{

				tmd.verticeIndices[(int)IndexOrder.BottomRight] = new Tuple<Vector3, int>(bottomRightCorner, sharedData.verticeIndices[(int)IndexOrder.TopRight].Item2);
				tmd.verticeIndices[(int)IndexOrder.BottomLeft] = new Tuple<Vector3, int>(bottomLeftCorner, sharedData.verticeIndices[(int)IndexOrder.TopLeft].Item2);
			}

			if (tileVertexCodex.TryGetValue(surroundingTiles[(int)Cardinality.SW], out sharedData))
			{
				tmd.verticeIndices[(int)IndexOrder.BottomLeft] = new Tuple<Vector3, int>(bottomLeftCorner, sharedData.verticeIndices[(int)IndexOrder.Right].Item2);
				tmd.verticeIndices[(int)IndexOrder.Left] = new Tuple<Vector3, int>(leftCorner, sharedData.verticeIndices[(int)IndexOrder.TopRight].Item2);
			}

			if (tileVertexCodex.TryGetValue(surroundingTiles[(int)Cardinality.NW], out sharedData))
			{
				tmd.verticeIndices[(int)IndexOrder.Left] = new Tuple<Vector3, int>(leftCorner, sharedData.verticeIndices[(int)IndexOrder.BottomRight].Item2);
				tmd.verticeIndices[(int)IndexOrder.TopLeft] = new Tuple<Vector3, int>(topLeftCorner, sharedData.verticeIndices[(int)IndexOrder.Right].Item2);
			}


			if (tmd.verticeIndices[(int)IndexOrder.TopLeft] == null)
				tmd.verticeIndices[(int)IndexOrder.TopLeft] = new Tuple<Vector3, int>(topLeftCorner, vertexIndex++);
			if (tmd.verticeIndices[(int)IndexOrder.TopRight] == null)
				tmd.verticeIndices[(int)IndexOrder.TopRight] = new Tuple<Vector3, int>(topRightCorner, vertexIndex++);
			if (tmd.verticeIndices[(int)IndexOrder.Right] == null)
				tmd.verticeIndices[(int)IndexOrder.Right] = new Tuple<Vector3, int>(rightCorner, vertexIndex++);
			if (tmd.verticeIndices[(int)IndexOrder.BottomRight] == null)
				tmd.verticeIndices[(int)IndexOrder.BottomRight] = new Tuple<Vector3, int>(bottomRightCorner, vertexIndex++);
			if (tmd.verticeIndices[(int)IndexOrder.BottomLeft] == null)
				tmd.verticeIndices[(int)IndexOrder.BottomLeft] = new Tuple<Vector3, int>(bottomLeftCorner, vertexIndex++);
			if (tmd.verticeIndices[(int)IndexOrder.Left] == null)
				tmd.verticeIndices[(int)IndexOrder.Left] = new Tuple<Vector3, int>(leftCorner, vertexIndex++);


			tileVertexCodex[tile] = tmd;


			triangleIndices[triangleIndex++] = tmd.verticeIndices[(int)IndexOrder.TopLeft].Item2;
			triangleIndices[triangleIndex++] = tmd.verticeIndices[(int)IndexOrder.TopRight].Item2;
			triangleIndices[triangleIndex++] = tmd.verticeIndices[(int)IndexOrder.BottomRight].Item2;
			triangleIndices[triangleIndex++] = tmd.verticeIndices[(int)IndexOrder.TopRight].Item2;
			triangleIndices[triangleIndex++] = tmd.verticeIndices[(int)IndexOrder.Right].Item2;
			triangleIndices[triangleIndex++] = tmd.verticeIndices[(int)IndexOrder.BottomRight].Item2;
			triangleIndices[triangleIndex++] = tmd.verticeIndices[(int)IndexOrder.BottomRight].Item2;
			triangleIndices[triangleIndex++] = tmd.verticeIndices[(int)IndexOrder.BottomLeft].Item2;
			triangleIndices[triangleIndex++] = tmd.verticeIndices[(int)IndexOrder.TopLeft].Item2;
			triangleIndices[triangleIndex++] = tmd.verticeIndices[(int)IndexOrder.BottomLeft].Item2;
			triangleIndices[triangleIndex++] = tmd.verticeIndices[(int)IndexOrder.Left].Item2;
			triangleIndices[triangleIndex++] = tmd.verticeIndices[(int)IndexOrder.TopLeft].Item2;
		}

		Vector3[] vertices = new Vector3[vertexIndex];

		int mapX = noiseMap.GetLength(0);
		int mapY = noiseMap.GetLength(1);
		int i = 0;
		foreach (var kvp in tileVertexCodex)
		{
			TileMeshData tmd = kvp.Value;
			foreach (Tuple<Vector3, int> verts in tmd.verticeIndices)
			{
				if (vertices[verts.Item2] != Vector3.zero) 
					continue;// If the tilemap is positioned at 0,0 there should never be a vertex there

				Vector3 pos = verts.Item1;
				float percentX = Mathf.InverseLerp(minX, maxX, pos.x);
				float percentY = Mathf.InverseLerp(minY, maxY, pos.z);
				int x = Mathf.RoundToInt(percentX * (mapX - 1));
				int y = Mathf.RoundToInt(percentY * (mapY - 1));
				vertices[verts.Item2] = pos + new Vector3(0, noiseMap[x, y] * 5, 0);
				++i;
			}
		}

		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = triangleIndices;
		mesh.RecalculateBounds();

		GetComponent<MeshFilter>().sharedMesh = mesh;

		Debug.Log("i: " + i + " vertices: " + vertices.Length + " triangles: " + (triangleIndices.Length / 3));
	}


	public struct TileMeshData
	{
		//public Vector3[] vertices;
		//public int[] indices;
		//public Dictionary<Vector3, int> vertexCodex;
		public Tuple<Vector3, int>[] verticeIndices;
		//public Tuple<Vector3, int> topLeft;



		//public Tuple<Vector3, int> bottomLeft;
	}
}
