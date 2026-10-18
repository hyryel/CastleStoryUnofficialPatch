using Motus.Behavior;

namespace Brix.Game.AI.InstructionType;

public class ActorInstruction<T> where T : Actor
{
	public T Actor;

	public ActorInstruction(T actor)
	{
		Actor = actor;
	}

	public virtual Node GetNode(T actor)
	{
		return actor.BasicAction();
	}
}
