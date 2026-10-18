using Brix.External.Signals;
using UnityEngine;

namespace Brix.Utils.Listeners;

public abstract class GameObjectListener : MonoBehaviour
{
	private Signal _signal = new Signal();

	public IReadOnlySignal Signal => _signal;

	protected void Invoke()
	{
		_signal.Invoke();
	}
}
public abstract class GameObjectListener<T1> : MonoBehaviour
{
	private Signal<T1> _signal = new Signal<T1>();

	public IReadOnlySignal<T1> Signal => _signal;

	protected void Invoke(T1 a)
	{
		_signal.Invoke(a);
	}
}
public abstract class GameObjectListener<T1, T2> : MonoBehaviour
{
	private Signal<T1, T2> _signal = new Signal<T1, T2>();

	public IReadOnlySignal<T1, T2> Signal => _signal;

	protected void Invoke(T1 a, T2 b)
	{
		_signal.Invoke(a, b);
	}
}
