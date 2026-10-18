using System;
using Brix.Common;
using UnityEngine;

namespace Motus.Behavior;

public class TimeCondition : PoolableObject<TimeCondition>, ICondition, IDisposable
{
	public float Delay;

	private float WaitUntil;

	public static TimeCondition Request(float delay)
	{
		TimeCondition timeCondition = PoolableObject<TimeCondition>.Request();
		timeCondition.Delay = delay;
		return timeCondition;
	}

	public void Init()
	{
		WaitUntil = Time.time + Delay;
	}

	public bool Test()
	{
		return Time.time > WaitUntil;
	}

	protected override void ResetObject()
	{
		WaitUntil = float.MaxValue;
	}

	public void Dispose()
	{
		ReleaseToPool();
	}
}
