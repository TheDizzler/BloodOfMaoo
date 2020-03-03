using UnityEngine;

namespace AtomosZ.BoMII.Terrain.Generators
{
	public static class Noise
	{
		public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed,
			float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
		{
			System.Random rng = new System.Random(seed);
			Vector2[] octaveOffsets = new Vector2[octaves];
			for (int i = 0; i < octaves; ++i)
			{
				float offsetX = rng.Next(-100000, 100000) + offset.x;
				float offsetY = rng.Next(-100000, 100000) + offset.y;
				octaveOffsets[i] = new Vector2(offsetX, offsetY);
			}
			// create an empty noise map with the mapDepth and mapWidth coordinates
			float[,] noiseMap = new float[mapWidth, mapHeight];

			if (scale <= 0)
				scale = .0001f;

			float maxNoiseHeight = float.MinValue;
			float minNoiseHeight = float.MaxValue;

			float halfWidth = mapWidth * .5f;
			float halfHeight = mapHeight * .5f;

			for (int y = 0; y < mapHeight; ++y)
			{
				for (int x = 0; x < mapWidth; ++x)
				{
					float amplitude = 1;
					float frequency = 1;
					float noiseHeight = 0;
					for (int i = 0; i < octaves; ++i)
					{
						// calculate sample indices based on the coordinates and the scale
						float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
						float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

						// generate noise value using PerlinNoise
						float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

						noiseHeight += perlinValue * amplitude;

						amplitude *= persistance;
						frequency *= lacunarity;
					}

					if (noiseHeight > maxNoiseHeight)
						maxNoiseHeight = noiseHeight;
					if (noiseHeight < minNoiseHeight)
						minNoiseHeight = noiseHeight;
					noiseMap[x, y] = noiseHeight;
				}
			}

			for (int y = 0; y < mapHeight; ++y)
			{
				for (int x = 0; x < mapWidth; ++x)
				{
					noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
				}
			}

			return noiseMap;
		}
	}
}