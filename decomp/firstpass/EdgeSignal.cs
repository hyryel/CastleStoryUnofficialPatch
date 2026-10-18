using System;
using Brix.External.Signals;
using Brix.Utils.Signals;
using UnityEngine;

public class EdgeSignal<T> : IReadOnlyEdgeSignal<T>
{
	public Signal<T> start = new Signal<T>();

	public Signal<T> on = new Signal<T>();

	public Signal<T> end = new Signal<T>();

	public IReadOnlySignal<T> Start => start;

	public IReadOnlySignal<T> On => on;

	public IReadOnlySignal<T> End => end;

	public void Connect(Action<T> _start, Action<T> _on, Action<T> _end, IReadOnlySignal<GameObject> until)
	{
		if (_start != null)
		{
			start.Connect(_start).DisconnectWhen(until);
		}
		if (_on != null)
		{
			on.Connect(_on).DisconnectWhen(until);
		}
		if (_end != null)
		{
			end.Connect(_end).DisconnectWhen(until);
		}
	}

	public void Connect<T2>(Action<T, T2> _start, Action<T, T2> _on, Action<T, T2> _end, T2 arg, IReadOnlySignal<GameObject> until)
	{
		if (_start != null)
		{
			start.Connect(_start, arg).DisconnectWhen(until);
		}
		if (_on != null)
		{
			on.Connect(_on, arg).DisconnectWhen(until);
		}
		if (_end != null)
		{
			end.Connect(_end, arg).DisconnectWhen(until);
		}
	}

	public void RelayTo(EdgeSignal<T> signals, IReadOnlySignal<GameObject> until)
	{
		Connect(delegate(T h)
		{
			signals.start.Invoke(h);
		}, delegate(T h)
		{
			signals.on.Invoke(h);
		}, delegate(T h)
		{
			signals.end.Invoke(h);
		}, until);
	}

	public void RelayTo<T2>(EdgeSignal<T2> signals, T2 arg, IReadOnlySignal<GameObject> until)
	{
		Connect(delegate
		{
			signals.start.Invoke(arg);
		}, delegate
		{
			signals.on.Invoke(arg);
		}, delegate
		{
			signals.end.Invoke(arg);
		}, until);
	}

	public void InvokeEdge(bool first, bool second, T value)
	{
		((!first && second) ? start : ((first && second) ? on : ((!first || second) ? null : end)))?.Invoke(value);
	}

	public void Clear()
	{
		start.Clear();
		on.Clear();
		end.Clear();
	}
}
