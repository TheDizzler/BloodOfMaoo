using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtomosZ.BoMII.Terrain
{
	/// <summary>
	/// Special thanks to Amit Patel for https://www.redblobgames.com/grids/hexagons/
	/// Notes:
	///		Unity uses odd-q.
	///		Due to the overwhelming use of cube coordinates in these functions
	///			(I believe there more effecient as well?) it may be prudent to
	///			store the cube coordinates in the tiles, instead of converting
	///			every time needed.
	/// </summary>
	public static class HexTools
	{
		/// <summary>
		/// Circles counter-clockwise from N to NE.
		/// </summary>
		public enum Cardinality { N, NW, SW, S, SE, NE };

		/// <summary>
		/// There are times when cube_lerp will return a point that's exactly on 
		/// the side between two hexes. Then cube_round will push it one way or 
		/// the other. The lines will look better if it's always pushed in the same 
		/// direction. You can do this by adding an "epsilon" hex Cube(1e-6, 2e-6, -3e-6) 
		/// to one or both of the endpoints before starting the loop. This will 
		/// "nudge" the line in one direction to avoid landing on side boundaries.
		/// </summary>
		public static readonly Vector3 epsilonCube = new Vector3(1E-6f, 2E-6f, -3E-6f);

		public static readonly Dictionary<Cardinality, Vector3Int> cubeDirections
			= new Dictionary<Cardinality, Vector3Int>()
		{
			{Cardinality.N,  new Vector3Int( 0,  1, -1) },
			{Cardinality.NW, new Vector3Int(-1,  1,  0) },
			{Cardinality.SW, new Vector3Int(-1,  0,  1) },
			{Cardinality.S,  new Vector3Int( 0, -1,  1) },
			{Cardinality.SE, new Vector3Int( 1, -1,  0) },
			{Cardinality.NE, new Vector3Int( 1,  0, -1) },
		};


		/// <summary>
		/// Takes in offset coords and returns in offset cords.
		/// </summary>
		/// <param name="center"></param>
		/// <param name="radius"></param>
		/// <returns></returns>
		public static List<Vector3Int> GetRing(Vector3Int center, int radius)
		{
			List<Vector3Int> tiles = new List<Vector3Int>();
			if (radius == 0)
			{
				tiles.Add(center);
				return tiles;
			}

			
			Vector3Int tile = OffsetToCube(center) + cubeDirections[Cardinality.SE] * radius;

			for (int i = 0; i < 6; ++i)
			{
				for (int j = 0; j < radius; ++j)
				{
					tiles.Add(CubeToOffset(tile));
					tile = GetCubeNeighbour(tile, (Cardinality)i);
				}
			}

			return tiles;
		}

		private static Vector3Int GetCubeNeighbour(Vector3Int cubeCoord, Cardinality cardinality)
		{
			return cubeCoord + cubeDirections[cardinality];
		}

		public static List<Vector3Int> GetSpiral(Vector3Int center, int radius)
		{
			List<Vector3Int> tiles = new List<Vector3Int>();
			for (int i = 0; i <= radius; ++i)
				tiles.AddRange(GetRing(center, i));

			return tiles;
		}

		/// <summary>
		/// Returns a "circle" of tiles that are within the range and center specified.
		/// </summary>
		/// <param name="center"></param>
		/// <param name="range"></param>
		/// <returns></returns>
		public static List<Vector3Int> GetTilesInRange(Vector3Int center, int range)
		{
			List<Vector3Int> tiles = new List<Vector3Int>();
			for (int x = -range; x <= range; ++x)
			{
				for (int y = Math.Max(-range, -x - range); y <= Math.Min(range, -x + range); ++y)
				{
					int z = -x - y;
					tiles.Add(CubeToOffset(OffsetToCube(center) + new Vector3Int(x, y, z)));
				}
			}

			return tiles;
		}

		/// <summary>
		/// Gets all hexes in-between two offset coordinate hexes.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public static List<Vector3Int> GetLine(Vector3Int from, Vector3Int to)
		{
			List<Vector3Int> line = new List<Vector3Int>();
			int n = HexTools.DistanceInTiles(from, to);

			Vector3 fromCube = OffsetToCube(from) + epsilonCube;
			Vector3 toCube = OffsetToCube(to) + epsilonCube;

			for (int i = 0; i <= n; ++i)
			{
				Vector3Int crossedTile = CubeToOffset(
					CubeRound(
						CubeLerp(fromCube, toCube, (1.0f / n) * i)));
				line.Add(crossedTile);
			}

			return line;
		}


		/// <summary>
		/// Sometimes we'll end up with a floating-point cube coordinate (x, y, z), 
		/// and we'll want to know which hex it should be in. This comes up in line drawing and pixel to hex.
		/// 
		/// Returns Vector3Int in Cube coordinates.
		/// </summary>
		/// <param name="cubeCoords"></param>
		/// <returns>in cube coordinates</returns>
		private static Vector3Int CubeRound(Vector3 cubeCoords)
		{
			int rx = Mathf.RoundToInt(cubeCoords.x);
			int ry = Mathf.RoundToInt(cubeCoords.y);
			int rz = Mathf.RoundToInt(cubeCoords.z);

			float x_diff = Mathf.Abs(rx - cubeCoords.x);
			float y_diff = Mathf.Abs(ry - cubeCoords.y);
			float z_diff = Mathf.Abs(rz - cubeCoords.z);

			if (x_diff > y_diff && x_diff > z_diff)
				rx = -ry - rz;
			else if (y_diff > z_diff)
				ry = -rx - rz;
			else
				rz = -rx - ry;

			return new Vector3Int(rx, ry, rz);
		}

		private static Vector3 CubeLerp(Vector3 cubeCoordsA, Vector3 cubeCoordsB, float t)
		{
			return new Vector3(
				Mathf.Lerp(cubeCoordsA.x, cubeCoordsB.x, t),
				Mathf.Lerp(cubeCoordsA.y, cubeCoordsB.y, t),
				Mathf.Lerp(cubeCoordsA.z, cubeCoordsB.z, t));
		}

		/// <summary>
		/// Takes two offset coordinate hexes, converts them to cube coordinate, then retrieves the distance.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static int DistanceInTiles(Vector3Int a, Vector3Int b)
		{
			var ac = OffsetToCube(a);
			var bc = OffsetToCube(b);
			return CubeDistance(ac, bc);
		}


		/// <summary>
		/// Converts odd-q offset hex grid coordinates to cube coordinates.
		/// </summary>
		/// <param name="offsetCoords"></param>
		/// <returns></returns>
		public static Vector3Int OffsetToCube(Vector3Int offsetCoords) // 
		{
			int x = offsetCoords.y;
			int z = offsetCoords.x - (offsetCoords.y - (offsetCoords.y & 1)) / 2;
			int y = -x - z;
			return new Vector3Int(x, y, z);
		}

		/// <summary>
		/// Converts hex coords from cube to odd-q offset.
		/// </summary>
		/// <param name="cubeCoords"></param>
		/// <returns></returns>
		private static Vector3Int CubeToOffset(Vector3Int cubeCoords)
		{
			int col = cubeCoords.x;
			int row = cubeCoords.z + (cubeCoords.x - (cubeCoords.x & 1)) / 2;
			return new Vector3Int(row, col, 0);
		}


		/// <summary>
		/// Finds distance in hexes between two Cube Coordinate hexes.
		/// </summary>
		/// <param name="cubeVectorA">MUST BE IN CUBE COORDINATE</param>
		/// <param name="cubeVectorB">MUST BE IN CUBE COORDINAT</param>
		/// <returns></returns>
		private static int CubeDistance(Vector3Int cubeVectorA, Vector3Int cubeVectorB)
		{
			return (int)(
				(Math.Abs(cubeVectorA.x - cubeVectorB.x)
				+ Math.Abs(cubeVectorA.y - cubeVectorB.y)
				+ Math.Abs(cubeVectorA.z - cubeVectorB.z)) * .5f);
		}
	}
}