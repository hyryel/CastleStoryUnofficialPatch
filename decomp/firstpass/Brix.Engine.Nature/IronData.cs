using Brix.Components;
using UnityEngine;

namespace Brix.Engine.Nature;

public class IronData : MineralData
{
	public override NatureType Type => NatureType.iron;

	public IronData(float growth, int variation, XYZ rootPos)
		: base(growth, variation, rootPos)
	{
	}

	public override Texture2D GetGrowthNoiseTex()
	{
		return BrixSingleton<IronRegistry>.Instance.GrowthNoiseTex;
	}

	public override int GetGrowthNoiseTexRepeats()
	{
		return BrixSingleton<IronRegistry>.Instance.GrowthNoiseTexRepeats;
	}
}
