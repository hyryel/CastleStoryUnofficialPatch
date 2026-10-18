using UnityEngine;

namespace Brix.Engine;

public struct BlockDataStruct
{
	public XYZ P;

	public Quaternion R;

	public float Dmg;

	public int type;

	public BlockDataStruct(XYZ p, Quaternion r, float dmg, int type)
	{
		P = p;
		R = r;
		Dmg = dmg;
		this.type = type;
	}
}
