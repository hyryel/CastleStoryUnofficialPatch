using System;
using Brix.External.Signals;
using UnityEngine;

public interface IReadOnlyEdgeSignal<T>
{
	IReadOnlySignal<T> Start { get; }

	IReadOnlySignal<T> On { get; }

	IReadOnlySignal<T> End { get; }

	void Connect(Action<T> _start, Action<T> _on, Action<T> _end, IReadOnlySignal<GameObject> until);

	void Connect<T2>(Action<T, T2> _start, Action<T, T2> _on, Action<T, T2> _end, T2 arg, IReadOnlySignal<GameObject> until);

	void RelayTo(EdgeSignal<T> signals, IReadOnlySignal<GameObject> until);

	void RelayTo<T2>(EdgeSignal<T2> signals, T2 arg, IReadOnlySignal<GameObject> until);
}
