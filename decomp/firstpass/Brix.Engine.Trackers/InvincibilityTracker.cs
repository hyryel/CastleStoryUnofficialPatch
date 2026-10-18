using System.Collections.Generic;
using Brix.Components;

namespace Brix.Engine.Trackers;

public class InvincibilityTracker : BrixSingleton<InvincibilityTracker>
{
	private readonly HashSet<XYZ> _invincibleVoxels = new HashSet<XYZ>();

	public static void SetInvincible(XYZ position)
	{
		BrixSingleton<InvincibilityTracker>.Instance._invincibleVoxels.Add(position);
	}

	public static void ClearInvincible(XYZ position)
	{
		BrixSingleton<InvincibilityTracker>.Instance._invincibleVoxels.Remove(position);
	}

	public static bool IsInvincible(XYZ position)
	{
		return BrixSingleton<InvincibilityTracker>.Instance._invincibleVoxels.Contains(position);
	}
}
