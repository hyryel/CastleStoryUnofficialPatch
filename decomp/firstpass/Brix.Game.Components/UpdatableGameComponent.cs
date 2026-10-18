using System;
using System.Reflection;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Game.Components;

public abstract class UpdatableGameComponent : GameComponent, IUpdatableComponent
{
	private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;

	protected static Type UpdateBaseType = typeof(UpdatableGameComponent).GetMethod("BrixUpdate", BindingFlags.Instance | BindingFlags.NonPublic).DeclaringType;

	protected static Type FixedUpdateBaseType = typeof(UpdatableGameComponent).GetMethod("BrixFixedUpdate", BindingFlags.Instance | BindingFlags.NonPublic).DeclaringType;

	protected static Type LateUpdateBaseType = typeof(UpdatableGameComponent).GetMethod("BrixLateUpdate", BindingFlags.Instance | BindingFlags.NonPublic).DeclaringType;

	protected static Type SlowUpdateBaseType = typeof(UpdatableGameComponent).GetMethod("BrixSlowUpdate", BindingFlags.Instance | BindingFlags.NonPublic).DeclaringType;

	protected GameClock.Connection UpdateActionConnection = GameClock.Connection.Empty;

	protected GameClock.Connection FixedUpdateActionConnection = GameClock.Connection.Empty;

	protected GameClock.Connection LateUpdateActionConnection = GameClock.Connection.Empty;

	protected GameClock.Connection SlowUpdateActionConnection = GameClock.Connection.Empty;

	public bool ErrorHappened;

	public bool StopOnError = true;

	Component IUpdatableComponent.Component => this;

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

	protected abstract void ProfiledBrixUpdate();

	protected abstract void ProfiledBrixFixedUpdate();

	protected abstract void ProfiledBrixLateUpdate();

	protected abstract void ProfiledBrixSlowUpdate();

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
	}

	protected virtual void OnFixedUpdateError(Exception e)
	{
	}

	protected virtual void OnLateUpdateError(Exception e)
	{
	}

	protected virtual void OnSlowUpdateError(Exception e)
	{
	}
}
public abstract class UpdatableGameComponent<T> : UpdatableGameComponent where T : UpdatableGameComponent<T>
{
	private static BrixLogger logger = new BrixLogger(typeof(UpdatableGameComponent<T>).FullName);

	protected static readonly UpdatableProfiler<UpdatableGameComponent, T> Updatable = new UpdatableProfiler<UpdatableGameComponent, T>();

	protected virtual bool HasUpdate => Updatable.HasUpdate;

	protected virtual bool HasFixedUpdate => Updatable.HasFixedUpdate;

	protected virtual bool HasLateUpdate => Updatable.HasLateUpdate;

	protected virtual bool HasSlowUpdate => Updatable.HasSlowUpdate;

	protected override void ProfiledBrixUpdate()
	{
		BrixUpdate();
	}

	protected override void ProfiledBrixFixedUpdate()
	{
		BrixFixedUpdate();
	}

	protected override void ProfiledBrixLateUpdate()
	{
		BrixLateUpdate();
	}

	protected override void ProfiledBrixSlowUpdate()
	{
		BrixSlowUpdate();
	}

	protected override void OnUpdateError(Exception e)
	{
		logger.ErrorFormat(e, "UpdatableGameComponent.Update error: {0} for {1}", GetType(), base.Name);
	}

	protected override void OnFixedUpdateError(Exception e)
	{
		if (!ErrorHappened || !StopOnError)
		{
			logger.ErrorFormat(e, "UpdatableGameComponent.FixedUpdate error: {0} for {1}", GetType(), base.Name);
			ErrorHappened = true;
		}
		if (StopOnError && !this.IsNullOrReleased())
		{
			base.enabled = false;
		}
	}

	protected override void OnLateUpdateError(Exception e)
	{
		logger.ErrorFormat(e, "UpdatableGameComponent.LateUpdate error: {0} for {1}", GetType(), base.Name);
	}

	protected override void OnSlowUpdateError(Exception e)
	{
		logger.ErrorFormat(e, "UpdatableGameComponent.SlowUpdate error: {0} for {1}", GetType(), base.Name);
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
}
