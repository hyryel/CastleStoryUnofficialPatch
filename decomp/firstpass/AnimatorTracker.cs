using System.Collections.Generic;
using Brix.Game.Components;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Animator))]
public class AnimatorTracker : UpdatableNetworkGameComponent
{
	private static readonly UpdatableNetworkProfiler<AnimatorTracker> UpdatableProfiler = new UpdatableNetworkProfiler<AnimatorTracker>();

	private List<AnimatorStateProgress> States;

	protected override UpdatableProfiler Updatable => UpdatableProfiler;

	protected override bool HasFixedUpdate => NetworkServer.active;

	public void Add(AnimatorStateProgress s)
	{
		States.Add(s);
	}

	protected override void Awake()
	{
		base.Awake();
		States = new List<AnimatorStateProgress>();
	}

	protected override void BrixFixedUpdate()
	{
		States.ForEach(delegate(AnimatorStateProgress s)
		{
			s.Update();
		});
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}
}
