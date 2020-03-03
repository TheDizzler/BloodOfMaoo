using UnityEditor;
using UnityEngine;


namespace AtomosZ.BoMII.Terrain.Editors
{
	[CustomEditor(typeof(TerrainMaster))]
	public class MapGeneratorEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			TerrainMaster mapGen = (TerrainMaster)target;
			if (GUILayout.Button("Generate"))
				mapGen.GenerateMap();
			if (GUILayout.Button("Clear Tile Map"))
				mapGen.tilemap.ClearAllTiles();

			if (DrawDefaultInspector() && mapGen.autoUpdate)
			{
				mapGen.GenerateMap();
			}

			
		}
	}
}