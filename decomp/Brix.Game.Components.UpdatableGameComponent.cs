using System;
using System.Reflection;
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
You are not using the latest version of the tool, please update.
Latest version is '11.1.0.9782' (yours is '9.1.0.7988')
