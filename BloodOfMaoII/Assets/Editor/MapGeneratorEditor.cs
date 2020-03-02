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

			if (DrawDefaultInspector() && mapGen.autoUpdate)
			{
				mapGen.GenerateMap();
			}
		}
	}
}