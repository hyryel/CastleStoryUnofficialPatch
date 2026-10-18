using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Utils;
using Brix.Utils.Collections;
using Brix.Utils.Logging;

namespace Brix.External.Signals;

public abstract class SignalBase<TAction> : IDisconnectableSignal, SignalInterface, IPoolable, IResetable, IDisposable where TAction : class, IDisposable
{
	public struct SignalInvokeListEnumerator : IEnumerator<TAction>, IDisposable, IEnumerator
	{
		private SignalBase<TAction> signal;

		private StructLinkedList<SignalConnectionInfo<TAction>> list;

		private StructLinkedListNode node;

		private IDisposable toDispose;

		public TAction Current { get; private set; }

		object IEnumerator.Current => Current;

		public SignalInvokeListEnumerator(SignalBase<TAction> signal)
		{
			this = default(SignalInvokeListEnumerator);
			this.signal = signal;
			signal._invokeFrame++;
			signal._invokedCount++;
			list = signal._actions;
			node = list.First;
			toDispose = Empty.Disposable;
		}

		public bool MoveNext()
		{
			if (!list.IsActive(node))
			{
				return false;
			}
			SignalConnectionInfo<TAction> value = list.GetValue(node);
			if (value.BirthFrame >= signal._invokeFrame)
			{
				return false;
			}
			toDispose.Dispose();
			toDispose = Empty.Disposable;
			Current = value.Action;
			StructLinkedListNode next = list.GetNext(node);
			if (value.AutoDisconnect)
			{
				list.Remove(node);
				toDispose = Current;
			}
			node = next;
			return true;
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}

		public void Dispose()
		{
			toDispose.Dispose();
			toDispose = Empty.Disposable;
		}
	}

	public struct SignalUnorderedInvokeListEnumerator : IEnumerator<TAction>, IDisposable, IEnumerator
	{
		private SignalBase<TAction> signal;

		private StructLinkedList<SignalConnectionInfo<TAction>>.UnorderedEnumerator e;

		private StructLinkedListNode node;

		private IDisposable toDispose;

		public TAction Current { get; private set; }

		object IEnumerator.Current => Current;

		public SignalUnorderedInvokeListEnumerator(SignalBase<TAction> signal)
		{
			this = default(SignalUnorderedInvokeListEnumerator);
			this.signal = signal;
			signal._invokeFrame++;
			signal._invokedCount++;
			e = signal._actions.GetUnorderedEnumerator();
			toDispose = Empty.Disposable;
		}

		public bool MoveNext()
		{
			if (!e.MoveNext())
			{
				return false;
			}
			SignalConnectionInfo<TAction> current = e.Current;
			if (current.BirthFrame >= signal._invokeFrame)
			{
				return false;
			}
			toDispose.Dispose();
			toDispose = Empty.Disposable;
			Current = current.Action;
			if (current.AutoDisconnect)
			{
				e.RemoveCurrentNode();
				toDispose = Current;
			}
			return true;
		}

		public void Reset()
		{
		}

		public void Dispose()
		{
			toDispose.Dispose();
			toDispose = Empty.Disposable;
		}
	}

	private static BrixLogger logger = new BrixLogger();

	private int _maximumConnections = 100;

	internal readonly StructLinkedList<SignalConnectionInfo<TAction>> _actions = new StructLinkedList<SignalConnectionInfo<TAction>>();

	private int _invokedCount = 0;

	private int _invokeFrame = 0;

	public int MaximumConnections
	{
		get
		{
			return _maximumConnections;
		}
		set
		{
			_maximumConnections = value;
		}
	}

	public int ConnectionCount => _actions.Count;

	public int InvokedCount => _invokedCount;

	public void Clear()
	{
		Reset();
		StructLinkedList<SignalConnectionInfo<TAction>>.UnorderedEnumerator unorderedEnumerator = _actions.GetUnorderedEnumerator();
		try
		{
			while (unorderedEnumerator.MoveNext())
			{
				unorderedEnumerator.Current.Action.Dispose();
			}
		}
		finally
		{
			unorderedEnumerator.Dispose();
		}
		_actions.Clear();
	}

	public void Reset()
	{
		_invokedCount = 0;
	}

	public SignalConnection Connect(TAction action)
	{
		return _Connect(action, autoDisconnect: false);
	}

	public SignalConnection ConnectOnce(TAction action)
	{
		return _Connect(action, autoDisconnect: true);
	}

	private SignalConnection _Connect(TAction action, bool autoDisconnect)
	{
		SignalConnectionInfo<TAction> value = new SignalConnectionInfo<TAction>(action, _invokeFrame, autoDisconnect);
		StructLinkedListNode node = _actions.AddLast(value);
		if (_actions.Count > MaximumConnections)
		{
			logger.WarnFormat("Signal.Connect: connection overflow (count={0}, max={1}).", ConnectionCount, MaximumConnections);
		}
		return new SignalConnection(this, node);
	}

	public bool IsConnected(SignalConnection handle)
	{
		return _actions.IsActive(handle.Node);
	}

	public bool Disconnect(SignalConnection handle)
	{
		if (_actions.IsActive(handle.Node))
		{
			_actions.GetValue(handle.Node).Action.Dispose();
			_actions.Remove(handle.Node);
			return true;
		}
		return false;
	}

	public virtual void Dispose()
	{
	}

	public virtual void ResetObject()
	{
		Clear();
		Reset();
	}
}
