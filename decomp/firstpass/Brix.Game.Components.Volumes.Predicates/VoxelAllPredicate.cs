using Brix.Engine;
using Smooth.Slinq;

namespace Brix.Game.Components.Volumes.Predicates;

public class VoxelAllPredicate : VoxelPredicateComponent
{
	public VoxelPredicateComponent[] predicates;

	public override bool HasProperty(XYZ p)
	{
		return predicates.Slinq().All((VoxelPredicateComponent predicate) => predicate.HasProperty(p));
	}
}
