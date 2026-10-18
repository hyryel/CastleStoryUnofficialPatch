using Brix.Engine;
using Brix.Legacy;

namespace Brix.Game.Components.Volumes.Predicates;

public class VoxelIsTree : VoxelPredicateComponent
{
	public override bool HasProperty(XYZ p)
	{
		return TreeRegistry.Instance.HasTree(p);
	}
}
