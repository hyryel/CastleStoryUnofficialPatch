using Brix.Engine.Nature;

namespace Brix.Engine;

public class MegaVoxelPyritePrinter : MegaVoxelMineralPrinter
{
	protected override MineralData GetMineral(XYZ p)
	{
		return PyriteRegistry.GetMineral(p);
	}
}
