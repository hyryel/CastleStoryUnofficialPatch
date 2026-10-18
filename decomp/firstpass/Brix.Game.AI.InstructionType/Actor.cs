using Brix.Game.Components;
using Motus.Behavior;

namespace Brix.Game.AI.InstructionType;

public class Actor : GameComponent
{
	public Node BasicAction()
	{
		return (StaticNode)Node.Sequence.Do(delegate
		{
		});
	}
}
