using System;
using UnityEngine;

namespace Brix.Engine.Nature;

[Serializable]
public struct BoulderSerializedData
{
	public XYZ[] positions;

	public Vector3 displayPos;

	public Vector3 offset;

	public float growth;

	public int variation;
}
