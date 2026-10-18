using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI.InstructionType;

public class CharacterInstruction : ActorInstruction<CharacterActor>
{
	public CharacterInstruction(CharacterActor actor)
		: base(actor)
	{
	}

	public override Node GetNode(CharacterActor actor)
	{
		return actor.MoveTo(new SpaceLocation(Vector3.zero));
	}
}
