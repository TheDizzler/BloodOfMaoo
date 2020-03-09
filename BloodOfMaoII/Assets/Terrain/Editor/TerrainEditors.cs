using AtomosZ.BoMII.Terrain.Generators;
using UnityEditor;
using UnityEngine;
using static FalloffGenerator;

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

	[CustomEditor(typeof(TerrainChunk))]
	public class TerrainChunkEditor : Editor
	{
		private FalloffSide falloffSide;

		public override void OnInspectorGUI()
		{
			TerrainChunk tc = (TerrainChunk)target;

			falloffSide = (FalloffSide)EditorGUILayout.EnumPopup("Falloff Type", falloffSide);
			if (GUILayout.Button("Set Falloff Map"))
			{
				tc.SetFalloffMap(falloffSide);
			}
		}
	}
}