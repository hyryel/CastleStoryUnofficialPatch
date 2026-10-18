using Brix.Game.AI;
using UnityEngine;

namespace Brix.Input.ContextualOrders;

public class ContextualAction
{
	private ContextualActor actor;

	public ContextualAction(GameObject subject, ContextualActionGroupData groupData, object target, ActionType type, bool toQueue)
	{
		Labor component = subject.GetComponent<Labor>();
		if (component != null)
		{
			Occupation component2 = component.GetComponent<Occupation>();
			if (component2 != null)
			{
				if (component2.CurrentOccupation == Occupation.Type.Builder)
				{
					actor = new BuilderActor(component, groupData, type, toQueue);
				}
				else if (component2.CurrentOccupation == Occupation.Type.Knight)
				{
					actor = new KnightActor(component, groupData, type, toQueue);
				}
				else if (component2.CurrentOccupation == Occupation.Type.Archer)
				{
					actor = new ArcherActor(component, groupData, type, toQueue);
				}
				else if (component2.CurrentOccupation == Occupation.Type.Corruptron)
				{
					actor = new BuilderActor(component, groupData, type, toQueue);
				}
				else if (component2.CurrentOccupation == Occupation.Type.Biftron)
				{
					actor = new BuilderActor(component, groupData, type, toQueue);
				}
				else if (component2.CurrentOccupation == Occupation.Type.Minitron)
				{
					actor = new BuilderActor(component, groupData, type, toQueue);
				}
			}
		}
		if (actor != null)
		{
			actor.Prepare(target);
		}
	}

	public void Resolve()
	{
		actor.SendOrder();
	}
}
