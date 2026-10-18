using Brix.Utils.Logging;
using Motus.Behavior;

namespace Brix.Game.AI;

public class Sequence
{
	private static BrixLogger logger = new BrixLogger(typeof(Sequence).FullName);

	private readonly string sequenceName;

	private readonly string delayName;

	private StaticNode node = Node.Do(delegate
	{
		logger.Error("Running uninitialized sequence. I should be initialized T_T");
	});

	private StaticNode delay = Node.Empty;

	public Sequence(string name)
	{
		sequenceName = name;
		delayName = "Delay - " + name;
	}

	public string GetName()
	{
		return sequenceName;
	}

	public void Set(StaticNode node, StaticNode delay)
	{
		this.node = node;
		this.delay = delay;
	}

	private StaticNode GetNode()
	{
		return node;
	}

	private StaticNode GetDelay()
	{
		return delay;
	}

	public Node Create()
	{
		return Node.Sequence.Do(GetNode()).Label(sequenceName).Do(GetDelay())
			.Label(delayName)
			.Loop()
			.ToNode;
	}
}
