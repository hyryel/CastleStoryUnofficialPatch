using UnityEngine;

internal class ConstantNoise3DGenerator : Noise3DGenerator
{
	public float value;

	public ConstantNoise3DGenerator(float value)
	{
		this.value = value;
	}

	public override float Noise(Vector3 v)
	{
		return value;
	}
}
