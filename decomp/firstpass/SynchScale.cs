using Brix.Components;
using Brix.External.Signals;
using Brix.Utils.Signals;
using UnityEngine;

public class SynchScale : BrixComponent
{
	[SerializeField]
	private Transform master;

	[SerializeField]
	private Transform slave;

	[SerializeField]
	private float ratio;

	[SerializeField]
	private bool inverse;

	public SynchScale With(Transform c, float ratio, bool inverse)
	{
		master = c;
		this.ratio = ratio;
		this.inverse = inverse;
		return this;
	}

	public SynchScale Of(Transform c)
	{
		slave = c;
		return this;
	}

	public SynchScale OfThis()
	{
		slave = base.transform;
		return this;
	}

	public SynchScale OfParent()
	{
		slave = base.transform.parent;
		return this;
	}

	public void When<T>(params Signal<T>[] signals)
	{
		for (int i = 0; i < signals.Length; i++)
		{
			When(signals[i]);
		}
	}

	public void Now()
	{
		if (slave != null && master != null)
		{
			slave.localScale = ratio * ((!inverse) ? master.localScale : new Vector3(1f / master.localScale.x, 1f / master.localScale.y, 1f / master.localScale.z));
		}
	}

	public void When<T>(Signal<T> signal)
	{
		Signal pureSignal = new Signal();
		signal.Connect(delegate
		{
			pureSignal.Invoke();
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
		pureSignal.Connect(delegate
		{
			Now();
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
	}

	public void When(Signal signal)
	{
		signal.Connect(delegate
		{
			Now();
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
	}
}
