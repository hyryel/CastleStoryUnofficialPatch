using Brix.Utils.Logging;
using Motus.Behavior;

namespace Brix.Game.AI.InstructionType;

public class ObjectActor : Actor
{
	private static BrixLogger logger = new BrixLogger(typeof(ObjectActor).FullName);

	public Node GetWorkedOn(CharacterActor character)
	{
		return (StaticNode)Node.Sequence.Do((CharacterActor c) => c.MoveTo(Location.Of(base.transform.position)), character).Do(delegate
		{
			logger.Info("Work work work");
		});
	}
}
