using UnityEngine;

internal class WhiteNoise3DGenerator : Noise3DGenerator
{
	public override float Noise(Vector3 v)
	{
		return Random.value;
	}
}
