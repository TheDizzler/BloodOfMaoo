using UnityEditor;
using UnityEngine;

namespace AtomosZ.BoMII.Terrain.Generation
{
	[CustomEditor(typeof(HexMapGenerator))]
	public class HexMapGeneratorEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			HexMapGenerator mapGen = (HexMapGenerator)target;

			if (GUILayout.Button("Generate") || DrawDefaultInspector())
			{
				mapGen.GenerateMap();
			}

			//if (GUILayout.Button("Next Step"))
			//{
			//	mapGen.SmoothMap(Vector3Int.zero);
			//	SceneView.RepaintAll();
			//}

			if (GUILayout.Button("Clear Tile Map"))
				mapGen.ClearMap();

			if (GUILayout.Button("Expand"))
			{
				mapGen.RevealArea(new Vector3Int(40, -50, 0), 50);
			}
		}
	}
}