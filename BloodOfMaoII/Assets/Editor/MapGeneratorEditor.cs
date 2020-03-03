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
				mapGen.GenerateMap();


			if (DrawDefaultInspector() && mapGen.autoUpdate)
			{
				mapGen.GenerateMap();
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