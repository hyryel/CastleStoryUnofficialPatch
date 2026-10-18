using System;
using Brix.Components;
using Brix.External.Signals;
using UnityEngine;

namespace Brix.Engine.Lifecycle;

public class SingletonProvider : BrixSingleton<SingletonProvider>
{
	public class Inner<TOwner, TData> where TData : class, new()
	{
		public static TData Data;

		public static Action ClearAction = delegate
		{
			Data = (TData)null;
		};
	}

	public Signal ClearSignal = new Signal();

	protected override void Awake()
	{
		base.Awake();
		base.OnReleaseOrDestroySignal.Connect(delegate(GameObject go, SingletonProvider self)
		{
			self.ClearSignal.Invoke();
		}, this);
	}

	public void Clear()
	{
		ClearSignal.Invoke();
	}

	public static TData Get<TData>() where TData : class, new()
	{
		return Inner<SingletonProvider, TData>.Data ?? Create<SingletonProvider, TData>();
	}

	public static TData Get<TOwner, TData>() where TData : class, new()
	{
		return Inner<TOwner, TData>.Data ?? Create<TOwner, TData>();
	}

	private static TData Create<TOwner, TData>() where TData : class, new()
	{
		BrixSingleton<SingletonProvider>.Instance.ClearSignal.ConnectOnce(Inner<TOwner, TData>.ClearAction);
		return Inner<TOwner, TData>.Data = new TData();
	}
}
