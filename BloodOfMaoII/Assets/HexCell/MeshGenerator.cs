using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static AtomosZ.BoMII.Terrain.HexTools;

namespace AtomosZ.BoMII.Terrain
{
	public class MeshGenerator : MonoBehaviour
	{
		HexGrid hexGrid;
		//Tilemap tilemap;

		public void GenerateMesh(Tilemap tm, Vector3Int[] tiles, float tileWidth, float tileHeight)
		{
			hexGrid = new HexGrid(tm, tiles, tileWidth, tileHeight);
			//foreach (Vector3Int tile in tiles)
			//{
			//	TriangulateHex(tile);
			//}
		}

		private void TriangulateHex(Vector3Int tile)
		{


		}


		public class HexGrid
		{
			public Hex[] hexes;

			public HexGrid(Tilemap tilemap, Vector3Int[] tiles, float tileWidth, float tileHeight)
			{
				Dictionary<Vector3Int, ControlNode[]> controlNodeDict = new Dictionary<Vector3Int, ControlNode[]>();

				foreach (Vector3Int tile in tiles)
				{
					bool active = false;
					Vector3 worldpos = tilemap.CellToWorld(tile);
					Vector3 topLeft = worldpos - Vector3.right * tileWidth * .25f + Vector3.up * tileHeight * .5f;
					Vector3 left = worldpos - Vector3.right * tileWidth * .5f;
					
					ControlNode[] controlNodes = new ControlNode[2];
					controlNodes[0] = new ControlNode(
						topLeft, active, tileWidth, tileHeight);
					controlNodes[1] = new ControlNode(
						left, active, tileWidth, tileHeight);
					controlNodeDict[tile] = controlNodes;
				}

				hexes = new Hex[tiles.Length];
				for (int i = 0; i < tiles.Length; ++i)
				{
					Vector3Int coords = tiles[i];
					// guaranteed to exist
					ControlNode topLeft = controlNodeDict[coords][0];
					ControlNode left = controlNodeDict[coords][1];

					// may not exist, depending on location on tile
					ControlNode bottomLeft = null;
					ControlNode right = null;
					ControlNode bottomRight = null;
					ControlNode topRight = null;


					Vector3Int[] surrounding = HexTools.GetSurroundingTilesOffset(coords);
					if (controlNodeDict.TryGetValue(surrounding[(int)Cardinality.S], out ControlNode[] sNodes))
					{
						bottomLeft = sNodes[0];
					}

					if (controlNodeDict.TryGetValue(surrounding[(int)Cardinality.SE], out ControlNode[] seNodes))
					{
						right = seNodes[0];
						bottomRight = seNodes[1];
					}

					if (controlNodeDict.TryGetValue(surrounding[(int)Cardinality.NE], out ControlNode[] neNodes))
					{
						topRight = neNodes[1];
					}

					hexes[i] = new Hex(topLeft, left, bottomLeft, bottomRight, right, topRight);
				}
			}
		}

		public class Hex
		{
			public ControlNode nw, ne, e, se, sw, w;
			public Node centerTop, rightTop, rightBottom, centerBottom, leftBottom, leftTop;
			public int configuration;


			public Hex(ControlNode nw, ControlNode w, ControlNode sw, ControlNode se, ControlNode e, ControlNode ne)
			{
				this.nw = nw;
				this.ne = ne;
				this.e = e;
				this.se = se;
				this.sw = sw;
				this.w = w;

				centerTop = nw.right;
				leftBottom = w.belowRight;
				leftTop = w.aboveRight;

				if (ne != null)
					rightTop = ne.belowRight;
				if (se != null)
					rightBottom = se.aboveRight;
				if (sw != null)
					centerBottom = sw.right;


				//if (topLeft.active)
				//	configuration += 8;
				//if (topRight.active)
				//	configuration += 4;
				//if (bottomRight.active)
				//	configuration += 2;
				//if (bottomLeft.active)
				//	configuration += 1;

			}

			public void OnDrawGizmos()
			{
				Gizmos.color = nw.active ? Color.black : Color.white;
				Gizmos.DrawCube(nw.position, Vector3.one * .4f);

				//if (ne != null)
				//{
				//	Gizmos.color = ne.active ? Color.black : Color.white;
				//	Gizmos.DrawCube(ne.position, Vector3.one * .4f);
				//}

				//if (e != null)
				//{
				//	Gizmos.color = e.active ? Color.black : Color.white;
				//	Gizmos.DrawCube(e.position, Vector3.one * .4f);
				//}

				//if (se != null)
				//{
				//	Gizmos.color = se.active ? Color.black : Color.white;
				//	Gizmos.DrawCube(se.position, Vector3.one * .4f);
				//}

				//if (sw != null)
				//{
				//	Gizmos.color = sw.active ? Color.black : Color.white;
				//	Gizmos.DrawCube(sw.position, Vector3.one * .4f);
				//}

				Gizmos.color = w.active ? Color.black : Color.white;
				Gizmos.DrawCube(w.position, Vector3.one * .4f);

				Gizmos.color = Color.grey;
				Gizmos.DrawCube(centerTop.position, Vector3.one * .15f);
				//if (ne != null)
				//	Gizmos.DrawCube(rightTop.position, Vector3.one * .15f);
				//if (se != null)
				//	Gizmos.DrawCube(rightBottom.position, Vector3.one * .15f);
				//if (sw != null)
				//	Gizmos.DrawCube(centerBottom.position, Vector3.one * .15f);
				Gizmos.DrawCube(leftBottom.position, Vector3.one * .15f);
				Gizmos.DrawCube(leftTop.position, Vector3.one * .15f);
			}
		}

		/// <summary>
		/// Point halfway between corners
		/// </summary>
		public class Node
		{
			public Vector3 position;
			public int vertexIndex = -1;

			public Node(Vector3 position)
			{
				this.position = position;
			}
		}

		/// <summary>
		/// Corner of hex.
		/// </summary>
		public class ControlNode : Node
		{
			public bool active;
			public Node aboveRight, right, belowRight;

			public ControlNode(Vector3 pos, bool active, float hexWidth, float hexHeight) : base(pos)
			{
				this.active = active;
				aboveRight = new Node(pos + Vector3.right * hexWidth * .125f + Vector3.up * hexHeight * .25f);
				right = new Node(pos + Vector3.right * hexWidth * .25f);
				belowRight = new Node(pos + Vector3.right * hexWidth * .125f - Vector3.up * hexHeight * .25f);
			}
		}

		void OnDrawGizmos()
		{
			if (hexGrid != null)
			{
				for (int x = 0; x < hexGrid.hexes.Length; ++x)
				{
					hexGrid.hexes[x].OnDrawGizmos();

				}
			}
		}
	}
}