using UnityEngine;

namespace AtomosZ.BoMII.Terrain.Generators
{
	public static class Noise
	{
		public enum NormalizeMode { Local, Global };

		public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed,
			float scale, int octaves, float persistance, float lacunarity, Vector2 offset,
			NormalizeMode normalizeMode)
		{
			System.Random rng = new System.Random(seed);

			float maxPossibleHeight = 0;
			float amplitude = 1;
			float frequency = 1;

			Vector2[] octaveOffsets = new Vector2[octaves];
			for (int i = 0; i < octaves; ++i)
			{
				float offsetX = rng.Next(-100000, 100000) + offset.x;
				float offsetY = rng.Next(-100000, 100000) - offset.y;
				octaveOffsets[i] = new Vector2(offsetX, offsetY);
				maxPossibleHeight += amplitude;
				amplitude *= persistance;
			}
			// create an empty noise map with the mapDepth and mapWidth coordinates
			float[,] noiseMap = new float[mapWidth, mapHeight];

			if (scale <= 0)
				scale = .0001f;

			float maxLocalNoiseHeight = float.MinValue;
			float minLocalNoiseHeight = float.MaxValue;

			float halfWidth = mapWidth * .5f;
			float halfHeight = mapHeight * .5f;

			for (int y = 0; y < mapHeight; ++y)
			{
				for (int x = 0; x < mapWidth; ++x)
				{
					amplitude = 1;
					frequency = 1;
					float noiseHeight = 0;
					for (int i = 0; i < octaves; ++i)
					{
						// calculate sample indices based on the coordinates and the scale
						float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
						float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

						// generate noise value using PerlinNoise
						float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

						noiseHeight += perlinValue * amplitude;

						amplitude *= persistance;
						frequency *= lacunarity;
					}

					if (noiseHeight > maxLocalNoiseHeight)
						maxLocalNoiseHeight = noiseHeight;
					if (noiseHeight < minLocalNoiseHeight)
						minLocalNoiseHeight = noiseHeight;
					noiseMap[x, y] = noiseHeight;
				}
			}

			Vector2 center = new Vector2(mapWidth / 2, mapHeight / 2);

			for (int y = 0; y < mapHeight; ++y)
			{
				for (int x = 0; x < mapWidth; ++x)
				{
					//if (normalizeMode == NormalizeMode.Local)
					//	noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
					//else
					//{
					//	float normalizedHeight = (noiseMap[x, y] + 1) / (2 * maxPossibleHeight);
					//	noiseMap[x, y] = normalizedHeight;
					//}

					float inverseLerp = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
					float normalizedHeight = (noiseMap[x, y] + 1) / (2 * maxPossibleHeight);

					float distFromCenter = Vector2.Distance(new Vector2(x, y), center);
					float t;
					if (distFromCenter > EndlessTerrain.DMax)
						t = 1;
					else if (distFromCenter < EndlessTerrain.DMin)
						t = 0;
					else
						t = Mathf.Lerp(1, 0, distFromCenter/ center.x);
					//float t = Mathf.Min(distFromCenter / Vector2.Distance(Vector2.zero, center), EndlessTerrain.MaxTRatio);

					float internormalized = Mathf.Lerp(inverseLerp, normalizedHeight, t);
					noiseMap[x, y] = internormalized;
					// z= x^2 / a^2 + y^2/ b^2
					// where a and b are constants that dictate the level of curvature in the xz and yz planes respectively
					// if a == b it is a circular paraboloid
					//float parabolicConstant = 2;
					//float z = (x * x + y * y) / (2 * parabolicConstant * parabolicConstant);
					//float internormalized = Mathf.Lerp(normalizedHeight, inverseLerp, z);
					//noiseMap[x,y] = internormalized;

				}
			}

			return noiseMap;
		}
	}
}