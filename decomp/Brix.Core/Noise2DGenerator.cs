using UnityEngine;

internal abstract class Noise2DGenerator : NoiseGenerator, INoise2DGenerator
{
	public abstract float Noise(Vector2 v);

	public float Noise(float x, float y)
	{
		return Noise(new Vector2(x, y));
	}
}
