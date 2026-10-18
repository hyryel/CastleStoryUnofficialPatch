using System;
using Brix.Utils.Closures;

namespace Motus.Behavior;

public abstract class Shielder
{
	public class ShieldNode : PooledNode<ShieldNode>
	{
		internal Node _shieldedNode;

		public override NodeRun Spawn()
		{
			return ShieldNodeRun.Request(this);
		}

		public override NodeRun SpawnWithArg(object arg)
		{
			return Spawn();
		}

		public static ShieldNode Request(Node sharedNode)
		{
			ShieldNode shieldNode = PooledNode<ShieldNode>.Request();
			shieldNode._shieldedNode = sharedNode;
			return shieldNode;
		}

		protected override void Clear()
		{
			if (_shieldedNode != null)
			{
				_shieldedNode.Dispose();
			}
		}

		public void Release()
		{
			Dispose();
		}
	}

	public class ShieldNodeRun : PooledNodeRun<ShieldNodeRun>
	{
		private NodeRun _nodeRun;

		public static NodeRun Request(ShieldNode n)
		{
			ShieldNodeRun shieldNodeRun = PooledNodeRun<ShieldNodeRun>.Request(n.Label, n.Context, n.Finalizer.Delegate);
			shieldNodeRun._nodeRun = n._shieldedNode.Spawn();
			return shieldNodeRun;
		}

		protected override void Init()
		{
			_nodeRun.Start();
		}

		protected override void Tick()
		{
			_nodeRun.Run();
			if (_nodeRun.State != NodeState.Running)
			{
				base.State = NodeState.Done;
			}
		}

		protected override void Terminate()
		{
			_nodeRun.Halt();
		}

		protected override void Clear()
		{
			if (_nodeRun != null)
			{
				_nodeRun.Dispose();
				_nodeRun = null;
			}
		}
	}

	public struct Builder
	{
		private ShieldNode _shieldNode;

		internal Node Root => _shieldNode;

		public Builder(Node node)
		{
			_shieldNode = ShieldNode.Request(node);
		}

		public Builder Finally(Action a)
		{
			_shieldNode.Finalizer = a.Make().Pooled2();
			return this;
		}

		public Builder Finally<P1>(Action<P1> a, P1 p1)
		{
			_shieldNode.Finalizer = a.Make().Close(p1).Pooled2();
			return this;
		}

		public Builder Finally<P1, P2>(Action<P1, P2> a, P1 p1, P2 p2)
		{
			_shieldNode.Finalizer = a.Make().Close(p2).Close(p1)
				.Pooled2();
			return this;
		}

		public Builder Finally<P1, P2, P3>(Action<P1, P2, P3> a, P1 p1, P2 p2, P3 p3)
		{
			_shieldNode.Finalizer = a.Make().Close(p3).Close(p2)
				.Close(p1)
				.Pooled2();
			return this;
		}

		public Builder Finally<P1, P2, P3, P4>(Action<P1, P2, P3, P4> a, P1 p1, P2 p2, P3 p3, P4 p4)
		{
			_shieldNode.Finalizer = a.Make().Close(p4).Close(p3)
				.Close(p2)
				.Close(p1)
				.Pooled2();
			return this;
		}

		public static implicit operator StaticNode(Builder m)
		{
			return m._shieldNode;
		}

		public Builder Label(string label)
		{
			_shieldNode.Label = label;
			return this;
		}
	}
}
