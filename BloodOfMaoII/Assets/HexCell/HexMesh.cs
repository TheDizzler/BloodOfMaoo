using System.Collections.Generic;
using AtomosZ.BoMII.Terrain;
using UnityEngine;
using UnityEngine.Tilemaps;
using static AtomosZ.BoMII.Terrain.HexTools;

public class HexMesh : MonoBehaviour
{
	public Tilemap tilemap;
	public TerrainTile testTile;
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
		//TerrainTile newTile = Instantiate(testTile);
		//tilemap.SetTile(Vector3Int.zero, newTile);
		//tilemap.SetTile(HexTools.GetAdjacentTileOffset(Vector3Int.zero, Cardinality.NE), newTile);
		//tilemap.SetTile(HexTools.GetAdjacentTileOffset(Vector3Int.zero, Cardinality.SE), newTile);

		List<TerrainTile> tts = new List<TerrainTile>()
		{
			GetTile(Vector3Int.zero),
			GetTile(HexTools.GetAdjacentTileOffset(Vector3Int.zero, Cardinality.NE)),
			//GetTile(HexTools.GetAdjacentTileOffset(Vector3Int.zero, Cardinality.SE)),
		};
		MeshIt(new List<Vector3Int>() { Vector3Int.zero, HexTools.GetAdjacentTileOffset(Vector3Int.zero, Cardinality.NE), HexTools.GetAdjacentTileOffset(Vector3Int.zero, Cardinality.SE) });
	}


	public TerrainTile GetTile(Vector3Int offsetGridCoords)
	{
		return tilemap.GetTile<TerrainTile>(offsetGridCoords);
	}

	//private void MeshIt(List<TerrainTile> tiles)
	private void MeshIt(List<Vector3Int> tiles)
	{
		Vector3 toTopLeft = -Vector3.right * .25f * hexWidth + Vector3.up * .5f * hexHeight;
		Vector3 toTopRight = Vector3.right * .25f * hexWidth + Vector3.up * .5f * hexHeight;
		Vector3 toRight = Vector3.right * .5f * hexWidth;

		Dictionary<Vector3, int> vertexCodex = new Dictionary<Vector3, int>();
		int[] triangles = new int[tiles.Count * 12];
		int vertexIndex = 0;
		int triangleIndex = 0;

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


			//Vector3[] vertices = new Vector3[]
			//{
			//	topLeftCorner, topRightCorner, rightCorner,
			//	bottomRightCorner, bottomLeftCorner, leftCorner
			//};


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
		foreach (var kvp in vertexCodex)
		{
			vertices[kvp.Value] = kvp.Key;
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
