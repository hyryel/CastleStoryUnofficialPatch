using System;
using System.Collections.Generic;
using System.Diagnostics;
using Brix.External.Signals;
using Brix.Utils;
using Brix.Utils.Closures;
using UnityEngine;

namespace Motus.Behavior;

public static class ForEacher
{
	public class ForEachNode<T> : PooledNode<ForEachNode<T>>
	{
		internal List<Node> _nodes = new List<Node>();

		internal IClosure<IAction<List<T>>> _itemsRetriever;

		internal IClosure<IFunc<T, bool>> _itemsFilter;

		internal long _maxTicksPerRun = long.MaxValue;

		internal bool _stopAtFirstItem;

		public static ForEachNode<T> Request(IClosure<IAction<List<T>>> itemsRetriever)
		{
			ForEachNode<T> forEachNode = PooledNode<ForEachNode<T>>.Request();
			forEachNode._itemsRetriever = itemsRetriever;
			forEachNode._itemsFilter = Closure.Make((T t) => true).Pooled2();
			return forEachNode;
		}

		public override NodeRun Spawn()
		{
			return ForEachNodeRun<T>.Request(this);
		}

		public override NodeRun SpawnWithArg(object arg)
		{
			return Spawn();
		}

		public void AddNode(Node node)
		{
			_nodes.Add(node);
		}

		public void SetFilter(IClosure<IFunc<T, bool>> filterFunc)
		{
			_itemsFilter.Dispose();
			_itemsFilter = filterFunc;
		}

		protected override void Clear()
		{
			for (int num = _nodes.Count - 1; num >= 0; num--)
			{
				_nodes[num].Dispose();
			}
			_maxTicksPerRun = long.MaxValue;
			_nodes.Clear();
			_itemsRetriever.Dispose();
			_itemsFilter.Dispose();
		}
	}

	public class ForEachNodeRun<T> : PooledNodeRun<ForEachNodeRun<T>>
	{
		private NodeRun _currentNodeRun;

		private List<Node> _subNodes;

		private List<T> _items;

		private int _nodeIndex;

		private int _itemIndex;

		private bool _stopAtFirstItem;

		private long _processingTicksLeft;

		private long _maxTicksPerRun;

		private IAction<List<T>> _itemsRetriever;

		private IFunc<T, bool> _itemsFilter;

		private Stopwatch _stopWatch = new Stopwatch();

		public static NodeRun Request(ForEachNode<T> node)
		{
			ForEachNodeRun<T> forEachNodeRun = PooledNodeRun<ForEachNodeRun<T>>.Request(node.Label, node.Context, node.Finalizer.Delegate);
			forEachNodeRun._maxTicksPerRun = node._maxTicksPerRun;
			forEachNodeRun._subNodes = node._nodes;
			forEachNodeRun._itemsRetriever = node._itemsRetriever.Delegate;
			forEachNodeRun._itemsFilter = node._itemsFilter.Delegate;
			forEachNodeRun._stopAtFirstItem = node._stopAtFirstItem;
			return forEachNodeRun;
		}

		protected override void Init()
		{
			_processingTicksLeft = 0L;
			if (_subNodes.Count != 0)
			{
				_items = CollectionPool.RequestList<T>();
				_itemsRetriever.Invoke(_items);
				_itemIndex = -1;
				if (HasItemLeftToProcess())
				{
					_nodeIndex = 0;
					_currentNodeRun = SpawnCurrentNodeRun();
				}
				else
				{
					base.State = NodeState.Done;
				}
			}
			else
			{
				base.State = NodeState.Done;
			}
		}

		protected override void Tick()
		{
			CalculateAllowedTicks();
			if (HasProcessingTimeLeft())
			{
				NodeState nodeState = TickSequence();
				if (nodeState == NodeState.Fail && _currentNodeRun.Exception is LoopBreak)
				{
					base.State = NodeState.Done;
					return;
				}
				base.State = nodeState;
				Exception = _currentNodeRun.Exception;
			}
		}

		protected override void Terminate()
		{
			if (_currentNodeRun != null)
			{
				_currentNodeRun.Halt();
			}
			if (_items != null)
			{
				_items.Dispose();
				_items = null;
			}
		}

		protected override void Clear()
		{
			if (_currentNodeRun != null)
			{
				_currentNodeRun.Dispose();
				_currentNodeRun = null;
			}
		}

		private NodeState TickSequence()
		{
			StartTimeTracking();
			NodeState result = ProcessCurrentItem();
			while (ShouldMoveToNextItem() && HasItemLeftToProcess())
			{
				_nodeIndex = 0;
				_currentNodeRun.Dispose();
				_currentNodeRun = SpawnCurrentNodeRun();
				if (!HasProcessingTimeLeft())
				{
					result = NodeState.Running;
					break;
				}
				result = ProcessCurrentItem();
			}
			UpdateProcessingTicksLeft();
			StopTimeTracking();
			return result;
		}

		private NodeState ProcessCurrentItem()
		{
			while (_currentNodeRun.Run() == NodeState.Done && ++_nodeIndex < _subNodes.Count)
			{
				_currentNodeRun.Dispose();
				_currentNodeRun = SpawnCurrentNodeRun();
			}
			return _currentNodeRun.State;
		}

		private void StartTimeTracking()
		{
			_stopWatch.Start();
		}

		private void StopTimeTracking()
		{
			_stopWatch.Reset();
		}

		private void UpdateProcessingTicksLeft()
		{
			_processingTicksLeft -= _stopWatch.ElapsedTicks;
		}

		private void CalculateAllowedTicks()
		{
			if (_maxTicksPerRun != long.MaxValue)
			{
				_processingTicksLeft = Math.Min(_processingTicksLeft + _maxTicksPerRun, _maxTicksPerRun);
			}
			else
			{
				_processingTicksLeft = long.MaxValue;
			}
		}

		private bool HasProcessingTimeLeft()
		{
			return _processingTicksLeft >= _stopWatch.ElapsedTicks;
		}

		private bool HasItemLeftToProcess()
		{
			while (++_itemIndex < _items.Count)
			{
				if (_itemsFilter.Invoke(_items[_itemIndex]))
				{
					return true;
				}
			}
			return false;
		}

		private bool ShouldMoveToNextItem()
		{
			return (!_stopAtFirstItem) ? HasItemFinishedProcessing() : HasItemFailedProcessing();
		}

		private bool HasItemFinishedProcessing()
		{
			return _currentNodeRun.State == NodeState.Done || (_currentNodeRun.State == NodeState.Fail && _currentNodeRun.Exception is LoopContinue);
		}

		private bool HasItemFailedProcessing()
		{
			return _currentNodeRun.State == NodeState.Fail && !(_currentNodeRun.Exception is LoopBreak);
		}

		private NodeRun SpawnCurrentNodeRun()
		{
			if (_subNodes.Count != 0)
			{
				return _subNodes[_nodeIndex].SpawnWithArg(_items[_itemIndex]);
			}
			return EmptyNode._NodeRun;
		}
	}

	public struct Builder<TItem> : IBuilderBase, IParameteredSequenceBuilder, ISequenceBuilder
	{
		private readonly ForEachNode<TItem> _foreachNode;

		public StaticNode ToNode => _foreachNode;

		public Builder(IClosure<IAction<List<TItem>>> itemRetriever)
		{
			_foreachNode = ForEachNode<TItem>.Request(itemRetriever);
		}

		public static implicit operator StaticNode(Builder<TItem> m)
		{
			return m._foreachNode;
		}

		public Builder<TItem> StopAtFirstItemSuccess(bool stopOnSuccess = true)
		{
			_foreachNode._stopAtFirstItem = stopOnSuccess;
			return this;
		}

		void IBuilderBase.Label(string label, UnityEngine.Object context)
		{
			Node node = ((_foreachNode._nodes.Count <= 0) ? _foreachNode : _foreachNode._nodes[_foreachNode._nodes.Count - 1]);
			node.Label = label;
			node.Context = context;
		}

		public Builder<TItem> MaxTicksPerRun(long ticks)
		{
			_foreachNode._maxTicksPerRun = ticks;
			return this;
		}

		public Builder<TItem> Filter(Func<TItem, bool> filterFunc)
		{
			_foreachNode.SetFilter(filterFunc.Make().Pooled2());
			return this;
		}

		public Builder<TItem> Filter<P1>(Func<TItem, P1, bool> filterFunc, P1 p1)
		{
			_foreachNode.SetFilter(filterFunc.Make().Close(p1).Pooled2());
			return this;
		}

		public Builder<TItem> Filter<P1, P2>(Func<TItem, P1, P2, bool> filterFunc, P1 p1, P2 p2)
		{
			_foreachNode.SetFilter(filterFunc.Make().Close(p2).Close(p1)
				.Pooled2());
			return this;
		}

		public Builder<TItem> Filter<P1, P2, P3>(Func<TItem, P1, P2, P3, bool> filterFunc, P1 p1, P2 p2, P3 p3)
		{
			_foreachNode.SetFilter(filterFunc.Make().Close(p3).Close(p2)
				.Close(p1)
				.Pooled2());
			return this;
		}

		public Builder<TItem> Filter<P1, P2, P3, P4>(Func<TItem, P1, P2, P3, P4, bool> filterFunc, P1 p1, P2 p2, P3 p3, P4 p4)
		{
			_foreachNode.SetFilter(filterFunc.Make().Close(p4).Close(p3)
				.Close(p2)
				.Close(p1)
				.Pooled2());
			return this;
		}

		void ISequenceBuilder.AddNode(Node node)
		{
			_foreachNode.AddNode(node);
		}

		void IBuilderBase.SetFinalize(IClosure<IAction> a)
		{
			_foreachNode.Finalizer = a;
		}
	}
}
