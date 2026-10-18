using System;

namespace Brix.Game.Components.CatapultSpace;

public class CatapultShootStatus
{
	private bool wantsToFire;

	private Func<CatapultShootStatus> onFire;

	private unsafe static readonly CatapultShootStatus holdFire = new CatapultShootStatus
	{
		wantsToFire = false,
		onFire = new Func<CatapultShootStatus>(null, (nint)(delegate*<CatapultShootStatus>)(&CatapultShootStatus.get_HoldFire))
	};

	private unsafe static readonly CatapultShootStatus fireOnce = new CatapultShootStatus
	{
		wantsToFire = true,
		onFire = new Func<CatapultShootStatus>(null, (nint)(delegate*<CatapultShootStatus>)(&CatapultShootStatus.get_HoldFire))
	};

	private unsafe static readonly CatapultShootStatus fireAtWill = new CatapultShootStatus
	{
		wantsToFire = true,
		onFire = new Func<CatapultShootStatus>(null, (nint)(delegate*<CatapultShootStatus>)(&CatapultShootStatus.get_FireAtWill))
	};

	public bool WantsToFire => wantsToFire;

	public CatapultShootStatus OnFire => onFire();

	public static CatapultShootStatus HoldFire => holdFire;

	public static CatapultShootStatus FireOnce => fireOnce;

	public static CatapultShootStatus FireAtWill => fireAtWill;
}
