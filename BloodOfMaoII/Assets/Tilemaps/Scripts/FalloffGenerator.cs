using UnityEngine;

public static class FalloffGenerator
{
	public static float[,] GenerateFalloffMap(int size)
	{
		float a = Random.Range(.5f, 6);
		float b = Random.Range(.5f, 6);
		Debug.Log("a: " + a + " b: " + b);

		float[,] map = new float[size, size];
		for (int i = size/4; i < size; ++i)
			for (int j = size/4; j < size; ++j)
			{
				float x = i / (float)size * 2 - 1;
				float y = j / (float)size * 2 - 1;
				float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
				map[i, j] = Evaluate(value, a, b);

			}

		return map;
	}

	/// <summary>
	/// setting a and b to 1 will create a small island in center.
	/// Default value by Sebastian Lague were a = 3, b = 2.2.
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	static float Evaluate(float value, float a, float b)
	{
		return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
	}
}
