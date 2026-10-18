using Brix.Game.AI;
using Brix.Utils.Logging;
using Brix.Utils.Signals;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.NewUI.Compass;

public class CompassDriverCorruptron : CompassDriver
{
	private static BrixLogger logger = new BrixLogger(typeof(CompassDriverCorruptron).FullName);

	[SerializeField]
	private bool m_KillASAP;

	[SerializeField]
	protected CompassGrouper[] compassGroupers;

	protected override void ConnectToSignals()
	{
		UIGameObserver.enemyBricktrons.EnterCreation.Connect(OnEnterCreation).DisconnectWhen(Disconnected);
		UIGameObserver.enemyBricktrons.ExitCreation.Connect(OnExitCreation).DisconnectWhen(Disconnected);
		UIGameObserver.enemyBricktrons.EnterScreen.Connect(OnEnterScreen).DisconnectWhen(Disconnected);
		UIGameObserver.enemyBricktrons.ExitScreen.Connect(OnExitScreen).DisconnectWhen(Disconnected);
		UIGameObserver.enemyBricktrons.EnterFog.Connect(OnEnterFog).DisconnectWhen(Disconnected);
		UIGameObserver.enemyBricktrons.ExitFog.Connect(OnExitFog).DisconnectWhen(Disconnected);
	}

	protected virtual bool GetScreenFlag(Labor labor)
	{
		return UIGameObserver.enemyBricktrons.GetScreenFlag(labor);
	}

	protected virtual bool GetFogFlag(Labor labor)
	{
		return UIGameObserver.enemyBricktrons.GetFogFlag(labor);
	}

	private bool ValidateSignal(string signalName, Labor labor)
	{
		if (labor == null)
		{
			logger.Error(signalName + " signal is being called from a destroyed labor. Please sanitize at signal source.\nAborting.");
			return false;
		}
		if (labor.gameObject == null)
		{
			logger.Error(signalName + " signal is being called from a destroyed labor.gameObject. Please sanitize at signal source.\nAborting.");
			return false;
		}
		return true;
	}

	private void OnEnterCreation(Labor labor)
	{
		if (ValidateSignal("OnEnterCreation", labor))
		{
			AutoToggleCompass(labor);
		}
	}

	private void OnExitCreation(Labor labor)
	{
		if (ValidateSignal("OnExitCreation", labor))
		{
			KillCompass(labor);
		}
	}

	private void OnEnterScreen(Labor labor)
	{
		if (ValidateSignal("OnEnterScreen", labor))
		{
			AutoToggleCompass(labor);
		}
	}

	private void OnExitScreen(Labor labor)
	{
		if (ValidateSignal("OnExitScreen", labor))
		{
			AutoToggleCompass(labor);
		}
	}

	private void OnEnterFog(Labor labor)
	{
		if (ValidateSignal("OnEnterFog", labor))
		{
			AutoToggleCompass(labor);
		}
	}

	private void OnExitFog(Labor labor)
	{
		if (ValidateSignal("OnExitFog", labor))
		{
			AutoToggleCompass(labor);
		}
	}

	private void AutoToggleCompass(Labor labor)
	{
		AutoToggleCompass(labor, UIGameObserver.enemyBricktrons.GetScreenFlag(labor), UIGameObserver.enemyBricktrons.GetFogFlag(labor));
	}

	private void AutoToggleCompass(Labor labor, bool screenFlag, bool fogFlag)
	{
		if (screenFlag || fogFlag)
		{
			DisableCompass(labor);
		}
		else
		{
			EnableCompass(labor);
		}
	}

	private SoloCompass GetOrCreateCompass(Labor labor, out Sprite previous)
	{
		SoloCompass orCreateSoloCompass = dispenser.GetOrCreateSoloCompass(labor.gameObject, labor.Occupation.GetIcon64(), out previous);
		AddToGrouper(orCreateSoloCompass, orCreateSoloCompass.icon);
		return orCreateSoloCompass;
	}

	private void EnableCompass(Labor labor)
	{
		EnableCompass(GetOrCreateCompass(labor, out var _));
	}

	private void EnableCompass(SoloCompass soloCompass)
	{
		if (soloCompass != null)
		{
			soloCompass.CullingToggle(flag: true);
		}
	}

	private void DisableCompass(Labor labor)
	{
		Sprite previous;
		SoloCompass orCreateCompass = GetOrCreateCompass(labor, out previous);
		orCreateCompass.CullingToggle(flag: false);
		if (m_KillASAP)
		{
			RemoveFromGrouper(orCreateCompass, previous);
			KillCompass(labor);
		}
	}

	private void KillCompass(Labor labor)
	{
		RemoveFromGrouper(GetOrCreateCompass(labor, out var previous), previous);
		dispenser.DiscardSoloCompass(labor.gameObject, forceDestroy: true);
	}

	private void AddToGrouper(SoloCompass soloCompass, Sprite icon)
	{
		compassGroupers.Slinq().FirstOrNone((CompassGrouper g, Sprite i) => g.icon == i, icon).ForEach(delegate(CompassGrouper g, SoloCompass c)
		{
			g.Add(c);
		}, soloCompass);
	}

	private void RemoveFromGrouper(SoloCompass soloCompass, Sprite icon)
	{
		compassGroupers.Slinq().FirstOrNone((CompassGrouper g, Sprite i) => g.icon == i, icon).ForEach(delegate(CompassGrouper g, SoloCompass c)
		{
			g.Remove(c);
		}, soloCompass);
	}
}
