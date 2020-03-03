using UnityEngine;
using UnityEngine.Tilemaps;

namespace AtomosZ.BoMII.Terrain.Generators
{
	public class MapGenerator : MonoBehaviour
	{
		public enum DrawMode { NoiseMap, ColorMap, Mesh, HexGrid };

		public const int mapChunkSize = 241;
		
		public DrawMode drawMode;
		public bool autoUpdate = true;

		[Range(0, 6)]
		[SerializeField] private int levelOfDetail = 1;
		// noise map gen related variables
		//[SerializeField] private int mapChunkSize, mapChunkSize;
		[SerializeField] private float noiseScale = 1;
		[Range(1, 126)]
		[SerializeField] private int octaves = 1;
		[Range(0, 1)]
		[SerializeField] private float persistance = 1;
		[SerializeField] private float lacunarity = 1;
		[SerializeField] private int seed = 1;
		[SerializeField] private Vector2 offset = new Vector2();
		[SerializeField] private float meshHeighMultiplier = 1;
		[SerializeField] private AnimationCurve heightMapCurve = null;
		[SerializeField] private TerrainType[] regions = null;


		public void GenerateMap()
		{
			// calculate the offsets based on the tile position
			float[,] noiseMap = Noise.GenerateNoiseMap(
				mapChunkSize, mapChunkSize, seed, noiseScale,
				octaves, persistance, lacunarity, offset);

			//tilemap.ClearAllTiles();
			//int halfMapWidth = mapChunkSize / 2;
			//int halfMapHeight = mapChunkSize / 2;

			Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
			for (int y = 0; y < mapChunkSize; ++y)
			{
				for (int x = 0; x < mapChunkSize; ++x)
				{
					float currentHeight = noiseMap[x, y];
					for (int i = 0; i < regions.Length; ++i)
					{
						if (currentHeight <= regions[i].height)
						{
							colorMap[y * mapChunkSize + x] = regions[i].color;
							//tilemap.SetTile(new Vector3Int( y - halfMapHeight, x - halfMapWidth, 0), terrainTiles[i]);
							break;
						}
					}
				}
			}

			MapDisplay display = GetComponent<MapDisplay>();
			switch (drawMode)
			{
				case DrawMode.NoiseMap:
					display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
					break;
				case DrawMode.ColorMap:
					display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
					break;
				case DrawMode.Mesh:
					display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeighMultiplier, heightMapCurve, levelOfDetail),
						TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
					break;
				case DrawMode.HexGrid:

					break;
			}
		}


		public void OnValidate()
		{
			if (lacunarity < 1)
				lacunarity = 1;
		}

	}
}