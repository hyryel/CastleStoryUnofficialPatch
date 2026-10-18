using System;
using Brix.Network;
using Brix.Utils.Logging;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components;

public abstract class UpdatableNetworkGameComponent : NetworkGameComponent, IUpdatableComponent
{
	private static BrixLogger logger = new BrixLogger(typeof(UpdatableNetworkGameComponent).FullName);

	protected GameClock.Connection UpdateActionConnection = GameClock.Connection.Empty;

	protected GameClock.Connection FixedUpdateActionConnection = GameClock.Connection.Empty;

	protected GameClock.Connection LateUpdateActionConnection = GameClock.Connection.Empty;

	protected GameClock.Connection SlowUpdateActionConnection = GameClock.Connection.Empty;

	Component IUpdatableComponent.Component => this;

	protected virtual UpdatableProfiler Updatable => UpdatableProfiler.Default;

	protected virtual bool HasUpdate => Updatable.HasUpdate;

	protected virtual bool HasFixedUpdate => Updatable.HasFixedUpdate;

	protected virtual bool HasLateUpdate => Updatable.HasLateUpdate;

	protected virtual bool HasSlowUpdate => Updatable.HasSlowUpdate;

	protected virtual float SlowUpdateRate => 0.2f;

	protected virtual bool IsUpdatedWithUnscaledTime => false;

	void IUpdatableComponent.BrixUpdate()
	{
		BrixUpdate();
	}

	void IUpdatableComponent.BrixFixedUpdate()
	{
		BrixFixedUpdate();
	}

	void IUpdatableComponent.BrixLateUpdate()
	{
		BrixLateUpdate();
	}

	void IUpdatableComponent.BrixSlowUpdate()
	{
		BrixSlowUpdate();
	}

	void IUpdatableComponent.OnUpdateError(Exception e)
	{
		OnUpdateError(e);
	}

	void IUpdatableComponent.OnFixedUpdateError(Exception e)
	{
		OnFixedUpdateError(e);
	}

	void IUpdatableComponent.OnLateUpdateError(Exception e)
	{
		OnLateUpdateError(e);
	}

	void IUpdatableComponent.OnSlowUpdateError(Exception e)
	{
		OnSlowUpdateError(e);
	}

	protected virtual void BrixUpdate()
	{
	}

	protected virtual void BrixFixedUpdate()
	{
	}

	protected virtual void BrixLateUpdate()
	{
	}

	protected virtual void BrixSlowUpdate()
	{
	}

	protected virtual void OnUpdateError(Exception e)
	{
		logger.WarnFormat(e, "Error: {0}, {1}", GetType(), base.Name);
	}

	protected virtual void OnFixedUpdateError(Exception e)
	{
		logger.WarnFormat(e, "Error: {0}, {1}", GetType(), base.Name);
	}

	protected virtual void OnLateUpdateError(Exception e)
	{
		logger.WarnFormat(e, "Error: {0}, {1}", GetType(), base.Name);
	}

	protected virtual void OnSlowUpdateError(Exception e)
	{
		logger.WarnFormat(e, "Error: {0}, {1}", GetType(), base.Name);
	}

	private void ProfiledBrixUpdate()
	{
		BrixUpdate();
	}

	private void ProfiledBrixFixedUpdate()
	{
		BrixFixedUpdate();
	}

	private void ProfiledBrixLateUpdate()
	{
		BrixLateUpdate();
	}

	private void ProfiledBrixSlowUpdate()
	{
		BrixSlowUpdate();
	}

	protected virtual void OnEnable()
	{
		_OnEnable();
	}

	protected virtual void OnDisable()
	{
		_OnDisable();
	}

	private void _OnEnable()
	{
		if (base.Started)
		{
			if (HasUpdate)
			{
				UpdateActionConnection = GameClock.AddOnUpdate(this);
			}
			if (HasFixedUpdate)
			{
				FixedUpdateActionConnection = GameClock.AddOnFixedUpdate(this);
			}
			if (HasLateUpdate)
			{
				LateUpdateActionConnection = GameClock.AddOnLateUpdate(this);
			}
			if (HasSlowUpdate)
			{
				SlowUpdateActionConnection = GameClock.AddOnSlowUpdate(this, SlowUpdateRate, IsUpdatedWithUnscaledTime);
			}
		}
	}

	private void _OnDisable()
	{
		if (base.Started)
		{
			UpdateActionConnection.Disconnect();
			FixedUpdateActionConnection.Disconnect();
			LateUpdateActionConnection.Disconnect();
			SlowUpdateActionConnection.Disconnect();
		}
	}

	protected override void Start()
	{
		base.Start();
		_OnEnable();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
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
