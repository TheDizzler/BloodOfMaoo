using System.Collections.Generic;
using UnityEngine;

public static class FalloffGenerator
{
	public enum FalloffSide
	{
		None,
		Top, Bottom, Left, Right,
		TopLeft, TopRight, BottomRight, BottomLeft
	}

	public static float[,] GenerateIslandFalloffMap(int mapSize, float a, float b, bool circularIsland)
	{
		float halfMapSize = mapSize * .5f;
		float[,] map = new float[mapSize, mapSize];
		if (circularIsland)
		{
			for (int i = 0; i < mapSize; ++i)
			{
				for (int j = 0; j < mapSize; ++j)
				{
					float distanFromCenter = Vector2.Distance(new Vector2(halfMapSize, halfMapSize), new Vector2(i, j));
					float value = Mathf.Lerp(0, 1, distanFromCenter / halfMapSize);
					//map[i, j] = Evaluate(value, a, b);
					float circValue = Evaluate(value, a, b);

					float x = i / (float)mapSize * 2 - 1;
					float y = j / (float)mapSize * 2 - 1;
					value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
					float sqrValue = Evaluate(value, a, b);
					float diagonal = Vector2.Distance(new Vector2(0, 0), new Vector2(mapSize, mapSize));
					float dist = Vector2.Distance(new Vector2(i, j), new Vector2(0, 0));
					map[i, j] = Mathf.Lerp(circValue, sqrValue, dist / diagonal);

				}
			}
		}
		else
		{
			for (int i = 0; i < mapSize; ++i)
			{
				for (int j = 0; j < mapSize; ++j)
				{
					float x = i / (float)mapSize * 2 - 1;
					float y = j / (float)mapSize * 2 - 1;
					float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
					map[i, j] = Evaluate(value, a, b);
				}
			}
		}

		return map;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="mapSize"></param>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <param name="falloffSides"></param>
	/// <returns></returns>
	public static float[,] GenerateContinentFalloffMap(int mapSize, float a, float b, List<FalloffSide> falloffSides)
	{
		float halfMapSize = mapSize * .5f;
		float quarterMapSize = mapSize * .75f;

		float[,] map = new float[mapSize, mapSize];
		for (int i = 0; i < mapSize; ++i)
			for (int j = 0; j < mapSize; ++j)
			{
				//float x = i / (float)mapSize * 2 - 2.5f;
				//float y = j / (float)mapSize * 2 - 2.5f;
				//float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));

				float evaluated = 0;
				if (falloffSides.Contains(FalloffSide.Top))
				{
					//if (j >= i && j > mapSize - i)
					{
						//float x = i / (float)mapSize * 2 - 1;
						//float y = j / (float)mapSize * 2 - 1;
						//float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
						//float distance = Vector2.Distance(new Vector2(halfMapSize, halfMapSize), new Vector2(i, j));
						//float fade = Mathf.Lerp(1, 0, distance / (mapSize * mapSize));
						//evaluated = Evaluate(value, a, b) * fade;
					}


					//float distFromCenter = Vector2.Distance(new Vector2(halfMapSize, 2 * mapSize), new Vector2(i, 2 * j));
					//float value = Mathf.Lerp(1, 0, .25f * distFromCenter * distFromCenter / (halfMapSize * halfMapSize));
					//evaluated = Evaluate(value, a, b);
					float distFromCenter = Vector2.Distance(new Vector2(mapSize, 4 * mapSize), new Vector2(2 * i, 4 * j));
					float value = Mathf.Lerp(1, 0, .25f * distFromCenter * distFromCenter / (halfMapSize * halfMapSize));
					evaluated = Mathf.Max(Evaluate(value, a, b), evaluated);
				}

				if (falloffSides.Contains(FalloffSide.Bottom))
				{
					//if (j <= i && (j < mapSize - i))
					{
						//float x = i / (float)mapSize * 2 - 1;
						//float y = j / (float)mapSize * 2 - 1;
						//float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
						//evaluated = Evaluate(value, a, b);
					}

					//if (j < 1 && j <= i && (j < mapSize -i))
					//{
					//	evaluated = 1;
					//}
					//else
					{
						//float distFromCenter = Vector2.Distance(new Vector2(halfMapSize, 0), new Vector2(i, 4 * j));
						//float value = Mathf.Lerp(1, 0, .25f * distFromCenter * distFromCenter / (halfMapSize * halfMapSize));
						float distFromCenter = Vector2.Distance(new Vector2(mapSize, 0), new Vector2(2 * i, 4 * j));
						float value = Mathf.Lerp(1, 0, .25f * distFromCenter * distFromCenter / (halfMapSize * halfMapSize));
						evaluated = Mathf.Max(Evaluate(value, a, b), evaluated);
					}
				}

				if (falloffSides.Contains(FalloffSide.Left))
				{
					//if (j < i && j >= mapSize - i)
					{
						//float x = i / (float)mapSize * 2 - 1;
						//float y = j / (float)mapSize * 2 - 1;
						//float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
						//evaluated = Evaluate(value, a, b);
						float distFromCenter = Vector2.Distance(new Vector2(2f * mapSize, halfMapSize), new Vector2(2f * i, j));
						float value = Mathf.Lerp(1, 0, .25f * distFromCenter * distFromCenter / (halfMapSize * halfMapSize));
						evaluated = Mathf.Max(Evaluate(value, a, b), evaluated);
					}
				}

				if (falloffSides.Contains(FalloffSide.Right))
				{
					//if (j > i && j <= mapSize - i)
					//{
					//	float x = i / (float)mapSize * 2 - 1;
					//	float y = j / (float)mapSize * 2 - 1;
					//	float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
					//	evaluated = Evaluate(value, a, b);
					//}

					float distFromCenter = Vector2.Distance(new Vector2(0, halfMapSize), new Vector2(2f * i, j));
					float value = Mathf.Lerp(1, 0, .25f * distFromCenter * distFromCenter / (halfMapSize * halfMapSize));
					evaluated = Mathf.Max(Evaluate(value, a, b), evaluated);
				}


				if (falloffSides.Contains(FalloffSide.TopLeft))
				{
					//if (j > halfMapSize && i > halfMapSize)
					{
						//float x = i / (float)mapSize;
						//float y = j / (float)mapSize - 1f;
						//float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));

						//float distance = Vector2.Distance(new Vector2(mapSize, mapSize), new Vector2(i, j));
						//float fade = Mathf.Lerp(1, 0, distance * distance / (10 * mapSize));
						float distanFromCenter = Vector2.Distance(new Vector2(mapSize, mapSize), new Vector2(i, j));
						float value = Mathf.Lerp(1, 0, distanFromCenter * distanFromCenter / (halfMapSize * halfMapSize));
						evaluated = Mathf.Max(Evaluate(value, a, b), evaluated);
					}
				}

				if (falloffSides.Contains(FalloffSide.TopRight))
				{
					if (j > halfMapSize && i < halfMapSize)
					{
						//float x = i / (float)mapSize /** 1.5f*/ - 1f;
						//float y = j / (float)mapSize  /**2 */- 1f;
						//float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));

						//float distance = Vector2.Distance(new Vector2(0, mapSize), new Vector2(i, j));
						//float fade = Mathf.Lerp(1, 0, distance * distance / (10 * mapSize));
						//evaluated = Evaluate(value, a, b) * fade;
						float distanFromCenter = Vector2.Distance(new Vector2(0, mapSize), new Vector2(i, j));
						float value = Mathf.Lerp(1, 0, distanFromCenter * distanFromCenter / (halfMapSize * halfMapSize));
						evaluated = Mathf.Max(Evaluate(value, a, b), evaluated);
					}
				}

				if (falloffSides.Contains(FalloffSide.BottomRight))
				{
					//if (j < halfMapSize && i < halfMapSize)
					//{
					//	float x = i / (float)mapSize - 1f;
					//	float y = j / (float)mapSize;
					//	float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));

					//	float distance = Vector2.Distance(Vector2.zero, new Vector2(i, j));
					//	float fade = Mathf.Lerp(1, 0, distance * distance / (10 * mapSize));
					//	evaluated = Evaluate(value, a, b) * fade;
					//}

					float distanFromCenter = Vector2.Distance(new Vector2(0, 0), new Vector2(i, j));
					float value = Mathf.Lerp(1, 0, distanFromCenter * distanFromCenter / (halfMapSize * halfMapSize));
					evaluated = Mathf.Max(Evaluate(value, a, b), evaluated);
				}

				if (falloffSides.Contains(FalloffSide.BottomLeft))
				{
					//if (j < halfMapSize && i > halfMapSize)
					{
						//float x = i / (float)mapSize;
						//float y = j / (float)mapSize;
						//float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));

						//float distance = Vector2.Distance(new Vector2(mapSize, 0), new Vector2(i, j));
						//float fade = Mathf.Lerp(1, 0, distance * distance / (10 * mapSize));
						//evaluated = Evaluate(value, a, b) * fade;
						float distanFromCenter = Vector2.Distance(new Vector2(mapSize, 0), new Vector2(i, j));
						float value = Mathf.Lerp(1, 0, distanFromCenter * distanFromCenter / (halfMapSize * halfMapSize));
						evaluated = Mathf.Max(Evaluate(value, a, b), evaluated);
					}
				}

				map[i, j] = evaluated;
			}

		return map;
	}

	/// <summary>
	/// Value is subtracted from height map. Value of 1 = lowest possible terrain, i.e deep water,
	/// Value = 0 no change to height. 1 = equals white, 0 = black.
	/// Setting a and b to 1 will create a small island in center.
	/// Default value by Sebastian Lague were a = 3, b = 2.2.
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	static float Evaluate(float value, float a, float b)
	{
		return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
	}
}
