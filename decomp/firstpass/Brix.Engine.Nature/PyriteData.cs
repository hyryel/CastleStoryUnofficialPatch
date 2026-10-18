using Brix.Components;
using UnityEngine;

namespace Brix.Engine.Nature;

public class PyriteData : MineralData
{
	public override NatureType Type => NatureType.pyrite;

	public PyriteData(float growth, int variation, XYZ rootPos)
		: base(growth, variation, rootPos)
	{
	}

	public override Texture2D GetGrowthNoiseTex()
	{
		return BrixSingleton<PyriteRegistry>.Instance.GrowthNoiseTex;
	}

	public override int GetGrowthNoiseTexRepeats()
	{
		return BrixSingleton<PyriteRegistry>.Instance.GrowthNoiseTexRepeats;
	}
}
