using System;

namespace Brix.Input;

[Serializable]
public struct DampVector
{
	public float Limit;

	public float DampThreshold;

	public float BounceThreshold;
}
