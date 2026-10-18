using Brix.Engine;
using Smooth.Slinq;

namespace Brix.Game.Components.Volumes.Predicates;

public class VoxelAnyPredicate : VoxelPredicateComponent
{
	public VoxelPredicateComponent[] predicates;

	public override bool HasProperty(XYZ p)
	{
		return predicates.Slinq().Any((VoxelPredicateComponent predicate) => predicate.HasProperty(p));
	}
}
