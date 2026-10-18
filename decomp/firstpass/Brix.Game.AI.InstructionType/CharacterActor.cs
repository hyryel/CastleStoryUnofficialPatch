using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI.InstructionType;

public class CharacterActor : Actor
{
	public Node MoveTo(Location location)
	{
		return (StaticNode)Node.Sequence.Do((Transform t, Location l) => t.position = l.Position, base.transform, location);
	}
}
