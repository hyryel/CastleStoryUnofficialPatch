using Brix.Engine.Nature;

namespace Brix.Engine;

public class MegaVoxelIronPrinter : MegaVoxelMineralPrinter
{
	protected override MineralData GetMineral(XYZ p)
	{
		return IronRegistry.GetMineral(p);
	}
}
