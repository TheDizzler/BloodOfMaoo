using AtomosZ.BoMII.Terrain.Generators;
using UnityEditor;
using UnityEngine;


namespace AtomosZ.BoMII.Terrain.Editors
{
	[CustomEditor(typeof(MapGenerator))]
	public class MapGeneratorEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			MapGenerator mapGen = (MapGenerator)target;
			if (GUILayout.Button("Generate"))
				mapGen.DrawMapInEditor();


			if (DrawDefaultInspector() && mapGen.autoUpdate)
			{
				mapGen.DrawMapInEditor();
			}
		}
	}

	[CustomEditor(typeof(TerrainMaster))]
	public class TerrainMasterEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			TerrainMaster tm = (TerrainMaster)target;
			if (GUILayout.Button("Clear Tile Map"))
				tm.tilemap.ClearAllTiles();
			DrawDefaultInspector();
		}
	}
}