using Brix.Legacy;
using Brix.UI.Icons;
using Brix.Utils.Signals;
using UnityEngine;

namespace Brix.NewUI.Compass;

public class CompassDriverCrystal : CompassDriver
{
	[SerializeField]
	private bool m_KillASAP;

	private Sprite Icon64 => IconKeys._Wisp_Yellow.Get64();

	protected override void ConnectToSignals()
	{
		UIGameObserver.crystals.EnterCreation.Connect(OnEnterCreation).DisconnectWhen(Disconnected);
		UIGameObserver.crystals.ExitCreation.Connect(OnExitCreation).DisconnectWhen(Disconnected);
		UIGameObserver.crystals.AddFirefly.Connect(OnChangeFireflyCount).DisconnectWhen(Disconnected);
		UIGameObserver.crystals.RemoveFirefly.Connect(OnChangeFireflyCount).DisconnectWhen(Disconnected);
		UIGameObserver.crystals.EnterScreen.Connect(OnEnterScreen).DisconnectWhen(Disconnected);
		UIGameObserver.crystals.ExitScreen.Connect(OnExitScreen).DisconnectWhen(Disconnected);
	}

	private void OnEnterCreation(SpawnerCristal crystal)
	{
		AutoToggleCompass(crystal);
	}

	private void OnExitCreation(SpawnerCristal crystal)
	{
		AutoToggleCompass(crystal);
	}

	private void OnEnterScreen(SpawnerCristal crystal)
	{
		AutoToggleCompass(crystal, screenFlag: true);
	}

	private void OnExitScreen(SpawnerCristal crystal)
	{
		AutoToggleCompass(crystal, screenFlag: false);
	}

	private void OnChangeFireflyCount(SpawnerCristal crystal)
	{
		AutoToggleCompass(crystal);
	}

	private void AutoToggleCompass(SpawnerCristal crystal)
	{
		AutoToggleCompass(crystal, UIGameObserver.crystals.GetScreenFlag(crystal));
	}

	private void AutoToggleCompass(SpawnerCristal crystal, bool screenFlag)
	{
		if (!crystal.FireflyNest.isHome)
		{
			return;
		}
		SoloCompass orCreateCompass = GetOrCreateCompass(crystal);
		if (!(orCreateCompass == null))
		{
			orCreateCompass.count = UIGameObserver.crystals.GetFireflyCount(crystal);
			if (orCreateCompass.count == 0 || screenFlag)
			{
				DisableCompass(crystal);
			}
			else
			{
				EnableCompass(orCreateCompass);
			}
		}
	}

	private SoloCompass GetOrCreateCompass(SpawnerCristal crystal)
	{
		Sprite previous;
		return dispenser.GetOrCreateSoloCompass(crystal.gameObject, Icon64, out previous);
	}

	private void EnableCompass(SoloCompass soloCompass)
	{
		if (soloCompass != null)
		{
			soloCompass.CullingToggle(flag: true);
		}
	}

	private void DisableCompass(SpawnerCristal crystal)
	{
		if (m_KillASAP)
		{
			KillCompass(crystal);
		}
		else
		{
			dispenser.DiscardSoloCompass(crystal.gameObject);
		}
	}

	private void KillCompass(SpawnerCristal crystal)
	{
		dispenser.DiscardSoloCompass(crystal.gameObject, forceDestroy: true);
	}
}
