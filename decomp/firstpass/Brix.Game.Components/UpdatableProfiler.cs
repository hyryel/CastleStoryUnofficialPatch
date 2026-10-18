using System;
using System.Reflection;

namespace Brix.Game.Components;

public abstract class UpdatableProfiler
{
	private class DefaultUpdatableProfiler : UpdatableProfiler
	{
	}

	public static readonly UpdatableProfiler Default = new DefaultUpdatableProfiler();

	public bool HasUpdate;

	public bool HasFixedUpdate;

	public bool HasLateUpdate;

	public bool HasSlowUpdate;

	public string UpdateProfile;

	public string FixedUpdateProfile;

	public string LateUpdateProfile;

	public string SlowUpdateProfile;
}
public class UpdatableProfiler<TBase, TDerived> : UpdatableProfiler
{
	private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;

	public readonly Type UpdateType = typeof(TDerived).GetMethod("BrixUpdate", BindingFlags.Instance | BindingFlags.NonPublic).DeclaringType;

	public readonly Type FixedUpdateType = typeof(TDerived).GetMethod("BrixFixedUpdate", BindingFlags.Instance | BindingFlags.NonPublic).DeclaringType;

	public readonly Type LateUpdateType = typeof(TDerived).GetMethod("BrixLateUpdate", BindingFlags.Instance | BindingFlags.NonPublic).DeclaringType;

	public readonly Type SlowUpdateType = typeof(TDerived).GetMethod("BrixSlowUpdate", BindingFlags.Instance | BindingFlags.NonPublic).DeclaringType;

	public UpdatableProfiler()
	{
		HasUpdate = UpdateType != typeof(TBase);
		HasFixedUpdate = FixedUpdateType != typeof(TBase);
		HasLateUpdate = LateUpdateType != typeof(TBase);
		HasSlowUpdate = SlowUpdateType != typeof(TBase);
		UpdateProfile = $"BrixUpdate.{UpdateType.Name}";
		FixedUpdateProfile = $"BrixFixedUpdate.{FixedUpdateType.Name}";
		LateUpdateProfile = $"BrixLateUpdate.{LateUpdateType.Name}";
		SlowUpdateProfile = $"BrixSlowUpdate.{SlowUpdateType.Name}";
	}
}
