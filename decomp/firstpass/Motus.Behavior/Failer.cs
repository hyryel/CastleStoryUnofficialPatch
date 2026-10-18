using Brix.Utils;

namespace Motus.Behavior;

public static class Failer
{
	internal class _Node : PooledNode<_Node>
	{
		internal FailException _e;

		public override NodeRun Spawn()
		{
			return FailNodeRun.Request(this);
		}

		public override NodeRun SpawnWithArg(object arg)
		{
			return Spawn();
		}

		protected override void Clear()
		{
			if (_e is IPoolable)
			{
				(_e as IPoolable).Dispose();
			}
			_e = null;
		}

		public static _Node Request(FailException e)
		{
			_Node node = PooledNode<_Node>.Request();
			node._e = e;
			return node;
		}
	}

	internal class FailNodeRun : PooledNodeRun<FailNodeRun>
	{
		private FailException _e;

		public static FailNodeRun Request(_Node n)
		{
			FailNodeRun failNodeRun = PooledNodeRun<FailNodeRun>.Request(n.Label, n.Context, n.Finalizer.Delegate);
			failNodeRun._e = n._e;
			return failNodeRun;
		}

		protected override void Init()
		{
			base.State = NodeState.Fail;
			Exception = _e;
		}

		protected override void Clear()
		{
			_e = null;
		}
	}

	internal static _Node Make(FailException e)
	{
		return _Node.Request(e);
	}
}
