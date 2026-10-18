using Brix.Engine;

namespace Brix.Game.Components.Volumes.Predicates;

public class VoxelInversePredicate : VoxelPredicateComponent
{
	public VoxelPredicateComponent predicate;

	public override bool HasProperty(XYZ p)
	{
		return !predicate.HasProperty(p);
	}
}
