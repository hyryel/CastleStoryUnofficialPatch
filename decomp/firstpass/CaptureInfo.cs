using Brix.Game;
using UnityEngine;

internal struct CaptureInfo
{
	public int CapturePower;

	public CanCapture.CaptureActions AllowedCaptureActions;

	public Faction Faction;

	public bool hasLeader;

	public bool IsMixed;

	public CaptureInfo(int c, Faction f, bool hasLeader, bool isMixed, CanCapture.CaptureActions allowedCaptureActions)
	{
		CapturePower = c;
		Faction = f;
		this.hasLeader = hasLeader;
		IsMixed = isMixed;
		AllowedCaptureActions = allowedCaptureActions;
	}

	private void Set(int cp, Faction f, bool ispl, CanCapture.CaptureActions allowedCaptureActions)
	{
		CapturePower = cp;
		Faction = f;
		hasLeader = ispl;
		IsMixed = false;
		AllowedCaptureActions = allowedCaptureActions;
	}

	public void Reset(Faction faction, int basePower = 0)
	{
		CapturePower = basePower;
		hasLeader = false;
		Faction = faction;
		IsMixed = false;
	}

	public static CaptureInfo From(GameObject go)
	{
		CaptureInfo info = default(CaptureInfo);
		if (ValuesFromCanCapture(go, ref info))
		{
		}
		return info;
	}

	private static bool ValuesFromCanCapture(GameObject go, ref CaptureInfo info)
	{
		CanCapture component = go.GetComponent<CanCapture>();
		if (component == null)
		{
			return false;
		}
		info.Set(component.CapturePower, component.faction, ispl: false, component.AllowedCaptureActions);
		return true;
	}

	public bool Defends(CaptureClock clock)
	{
		if (!AllowedCaptureActions.HasFlag(CanCapture.CaptureActions.Defend) || CapturePower <= 0)
		{
			return false;
		}
		if ((bool)Faction && Affiliation.IsAllied(clock.gameObject, Faction.gameObject))
		{
			return true;
		}
		if ((bool)Faction && Affiliation.IsEnemy(clock.CurrentCapturers.gameObject, Faction.gameObject))
		{
			return true;
		}
		return false;
	}

	public bool ThereIs()
	{
		return CapturePower != 0;
	}

	public bool Attacks(CaptureClock clock)
	{
		if ((AllowedCaptureActions.HasFlag(CanCapture.CaptureActions.Attack) || (AllowedCaptureActions.HasFlag(CanCapture.CaptureActions.Neutralize) && clock.faction != Faction.DefaultNeutral)) && CapturePower > 0)
		{
			if ((bool)Faction && Affiliation.IsEnemyOrNeutral(clock.gameObject, Faction.gameObject))
			{
				return true;
			}
			if ((bool)Faction && Affiliation.IsAllied(clock.CurrentCapturers, Faction.gameObject))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsStrongerThan(CaptureInfo c)
	{
		return CapturePower > c.CapturePower;
	}

	public CaptureInfo Plus(CaptureInfo c)
	{
		return new CaptureInfo
		{
			CapturePower = CapturePower + c.CapturePower,
			IsMixed = (c.IsMixed | (c.Faction != Faction && (bool)Faction)),
			Faction = (Faction ?? c.Faction),
			hasLeader = (hasLeader | c.hasLeader),
			AllowedCaptureActions = c.AllowedCaptureActions
		};
	}
}
