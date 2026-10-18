using Brix.Engine;
using Brix.Game.Components.Volumes;

namespace Brix.Game.Components;

internal class MagicMineValidator : DefaultWEOValidator
{
	public override bool IsObstructed(ref ValidationData vData)
	{
		foreach (XYZ @case in vData.Volume.GetCases(VPropertyMask.occupy | VPropertyMask.fantome))
		{
			if (!VUtil.IsObstructed(@case, Obstructions, vData.Faction, vData.Volume))
			{
				return false;
			}
		}
		return true;
	}
}
