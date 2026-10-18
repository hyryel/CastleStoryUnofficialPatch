using UnityEngine;

internal abstract class Noise3DGenerator : NoiseGenerator, INoise3DGenerator
{
	public abstract float Noise(Vector3 v);

	public float Noise(float x, float y, float z)
	{
		return Noise(new Vector3(x, y, z));
	}
}
