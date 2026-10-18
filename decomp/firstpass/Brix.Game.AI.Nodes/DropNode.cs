using System;
using Brix.Utils.Logging;
using Motus.Behavior;

namespace Brix.Game.AI.Nodes;

public static class DropNode
{
	private static BrixLogger logger = new BrixLogger(typeof(DropNode).FullName);

	public static StaticNode Drop(this Labor labor)
	{
		return Node.Sequence.Do(delegate(Labor l)
		{
			logger.DebugFormat("{0} lache son inventory.", l.name);
		}, labor).Do(delegate(Labor l)
		{
			DropInventory(l);
		}, labor);
	}

	private static void DropInventory(Labor labor)
	{
		labor.eater.Starve();
		labor.recepteur.DropAll();
	}

	public static Node GoDropAt(this Labor labor, Location location, bool canFail)
	{
		if (location == null)
		{
			return (StaticNode)Node.Sequence.Do(labor.Drop());
		}
		return (StaticNode)Node.Sequence.Do((Labor _labor, Location _loc, bool _b) => _labor.RunToward(_loc, precise: true, aggressive: false).ShieldOrNot(!_b), labor, location, canFail).Do((Func<Labor, Location, Node>)NavigationNodes.TurnToward, labor, location).Do((Func<Labor, StaticNode>)Drop, labor);
	}

	public static Node GoDropAtExactly(this Labor labor, Location location, bool canFail)
	{
		if (location == null)
		{
			return (StaticNode)Node.Sequence.Do(labor.Drop());
		}
		return (StaticNode)Node.Sequence.Do((Labor _labor, Location _loc, bool _b) => _labor.RunToExactly(_loc).ShieldOrNot(!_b), labor, location, canFail).Do((Func<Labor, Location, Node>)NavigationNodes.TurnToward, labor, location).Do((Func<Labor, StaticNode>)Drop, labor);
	}
}
