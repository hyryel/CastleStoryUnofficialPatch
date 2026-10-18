namespace Motus.Behavior;

internal static class ForeverNode
{
	public class Node : StaticNode
	{
		public static Node Instance = new Node();

		private Node()
		{
			Label = "ForeverNode";
		}

		public override void Dispose()
		{
		}

		public override NodeRun SpawnWithArg(object arg)
		{
			return Spawn();
		}

		public override NodeRun Spawn()
		{
			return ForeverNodeRun.Instance;
		}
	}

	public class ForeverNodeRun : NodeRun
	{
		public static ForeverNodeRun Instance = new ForeverNodeRun("ForeverNode");

		private ForeverNodeRun(string label)
			: base(label)
		{
			base.State = NodeState.Running;
		}

		protected override void Init()
		{
			base.State = NodeState.Running;
		}
	}

	public static Node _Node = Node.Instance;

	public static ForeverNodeRun _NodeRun = ForeverNodeRun.Instance;
}
