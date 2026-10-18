using System;
using Brix.Utils.Closures;

namespace Motus.Behavior;

public static class Sharer
{
	public class ShareNode : PooledNode<ShareNode>
	{
		internal Node _sharedNode;

		~ShareNode()
		{
			Release();
			GC.ReRegisterForFinalize(this);
		}

		public override NodeRun Spawn()
		{
			return ShareNodeRun.Request(this);
		}

		public override NodeRun SpawnWithArg(object arg)
		{
			return Spawn();
		}

		public static ShareNode Request(Node sharedNode)
		{
			ShareNode shareNode = PooledNode<ShareNode>.Request();
			shareNode._sharedNode = sharedNode;
			return shareNode;
		}

		protected override void Clear()
		{
			if (_sharedNode != null)
			{
				_sharedNode.Dispose();
			}
		}

		public override void Dispose()
		{
		}

		public void Release()
		{
			base.Dispose();
		}
	}

	public class ShareNodeRun : PooledNodeRun<ShareNodeRun>
	{
		private NodeRun _nodeRun;

		public static NodeRun Request(ShareNode n)
		{
			ShareNodeRun shareNodeRun = PooledNodeRun<ShareNodeRun>.Request(n.Label, n.Context, n.Finalizer.Delegate);
			shareNodeRun._nodeRun = n._sharedNode.Spawn();
			return shareNodeRun;
		}

		protected override void Init()
		{
			_nodeRun.Start();
		}

		protected override void Tick()
		{
			_nodeRun.Run();
			base.State = _nodeRun.State;
			Exception = _nodeRun.Exception;
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
		private ShareNode _shareNode;

		public ShareNode Root => _shareNode;

		public Builder(Node node)
		{
			_shareNode = ShareNode.Request(node);
		}

		public Builder Finally(Action a)
		{
			_shareNode.Finalizer = a.Make().Pooled2();
			return this;
		}

		public Builder Finally<P1>(Action<P1> a, P1 p1)
		{
			_shareNode.Finalizer = a.Make().Close(p1).Pooled2();
			return this;
		}

		public Builder Finally<P1, P2>(Action<P1, P2> a, P1 p1, P2 p2)
		{
			_shareNode.Finalizer = a.Make().Close(p2).Close(p1)
				.Pooled2();
			return this;
		}

		public Builder Finally<P1, P2, P3>(Action<P1, P2, P3> a, P1 p1, P2 p2, P3 p3)
		{
			_shareNode.Finalizer = a.Make().Close(p3).Close(p2)
				.Close(p1)
				.Pooled2();
			return this;
		}

		public Builder Finally<P1, P2, P3, P4>(Action<P1, P2, P3, P4> a, P1 p1, P2 p2, P3 p3, P4 p4)
		{
			_shareNode.Finalizer = a.Make().Close(p4).Close(p3)
				.Close(p2)
				.Close(p1)
				.Pooled2();
			return this;
		}

		public static implicit operator StaticNode(Builder m)
		{
			return m._shareNode;
		}

		public static implicit operator ShareNode(Builder m)
		{
			return m._shareNode;
		}

		public Builder Label(string label)
		{
			_shareNode.Label = label;
			return this;
		}
	}
}
