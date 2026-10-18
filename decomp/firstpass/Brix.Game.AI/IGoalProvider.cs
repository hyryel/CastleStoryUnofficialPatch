using System.Collections.Generic;
using Brix.External.Factories;

namespace Brix.Game.AI;

public interface IGoalProvider
{
	void GetAll(ICollection<Goal> goals);

	bool HasNonStorable();

	void UniqueItemsGiven(ref HashSet<Factory.AssetKey> keys);

	void UniqueItemsNeeded(ref HashSet<Factory.AssetKey> keys);

	int GoalCount();
}
