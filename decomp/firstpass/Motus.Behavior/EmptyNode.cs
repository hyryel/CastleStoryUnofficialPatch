namespace Motus.Behavior;

internal static class EmptyNode
{
	public class Node : StaticNode
	{
		public static Node Instance = new Node();

		private Node()
		{
			Label = "EmptyNode";
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
			return EmptyNodeRun.Instance;
		}
	}

	public class EmptyNodeRun : NodeRun
	{
		public static EmptyNodeRun Instance = new EmptyNodeRun("EmptyNode");

		private EmptyNodeRun(string label)
			: base(label)
		{
			base.State = NodeState.Done;
		}

		protected override void Init()
		{
			base.State = NodeState.Done;
		}
	}

	public static Node _Node = Node.Instance;

	public static EmptyNodeRun _NodeRun = EmptyNodeRun.Instance;
}
