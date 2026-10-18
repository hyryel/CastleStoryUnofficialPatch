using Brix.Game.Components;

namespace Brix.Game.AI;

public static class IOperableExtender
{
	public static bool Hire(IOperable<Labor> operable, Labor labor)
	{
		labor.AssignToIdle();
		labor.Operable = operable;
		if (operable.Operator != labor)
		{
			operable.Fire(operable.Operator);
		}
		operable.Operator = labor;
		return true;
	}

	public static void Fire(IOperable<Labor> operable, Labor labor)
	{
		if (!(operable.Operator != labor) && !labor.IsNullOrReleased())
		{
			labor.Operable = null;
			operable.Operator = null;
		}
	}

	public static string String(this IOperable<Labor> op)
	{
		return (op == null) ? null : ((!(op.gameObject != null)) ? null : op.gameObject.name);
	}
}
