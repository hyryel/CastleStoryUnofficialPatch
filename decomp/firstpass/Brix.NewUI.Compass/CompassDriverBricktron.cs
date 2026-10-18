using Brix.Game.AI;
using Brix.Utils.Logging;
using Brix.Utils.Signals;
using UnityEngine;

namespace Brix.NewUI.Compass;

public class CompassDriverBricktron : CompassDriver
{
	private static BrixLogger logger = new BrixLogger(typeof(CompassDriverBricktron).FullName);

	[SerializeField]
	protected CompassGrouper workerGrouper;

	[SerializeField]
	protected CompassGrouper knightGrouper;

	[SerializeField]
	protected CompassGrouper archerGrouper;

	protected override void ConnectToSignals()
	{
		UIGameObserver.bricktrons.EnterCreation.Connect(OnBricktronAdded).DisconnectWhen(Disconnected);
		UIGameObserver.bricktrons.ExitCreation.Connect(OnBricktronRemoved).DisconnectWhen(Disconnected);
		UIGameObserver.bricktrons.EnterScreen.Connect(OnBricktronEnteredScreen).DisconnectWhen(Disconnected);
		UIGameObserver.bricktrons.ExitScreen.Connect(OnBricktronExitedScreen).DisconnectWhen(Disconnected);
		UIGameObserver.bricktrons.EnterSelection.Connect(OnBricktronSelected).DisconnectWhen(Disconnected);
		UIGameObserver.bricktrons.ExitSelection.Connect(OnBricktronUnselected).DisconnectWhen(Disconnected);
	}

	private bool ValidateSignal(string signalName, Labor labor)
	{
		if (labor == null)
		{
			logger.ErrorFormat("{0} signal is being called from a destroyed labor. Please sanitize at signal source.\nAborting.", signalName);
			return false;
		}
		if (labor.gameObject == null)
		{
			logger.ErrorFormat("{0} signal is being called from a destroyed labor.gameObject. Please sanitize at signal source.\nAborting.", signalName);
			return false;
		}
		return true;
	}

	private void OnBricktronAdded(Labor labor)
	{
		if (ValidateSignal("OnBricktronAdded", labor))
		{
			if (!UIGameObserver.bricktrons.GetScreenFlag(labor) && UIGameObserver.bricktrons.GetSelectionFlag(labor))
			{
				EnableCompass(labor);
			}
			else
			{
				DisableCompass(labor);
			}
		}
	}

	private void OnBricktronRemoved(Labor labor)
	{
		if (ValidateSignal("OnBricktronRemoved", labor))
		{
			KillCompass(labor);
		}
	}

	private void OnBricktronSelected(Labor labor)
	{
		if (ValidateSignal("OnBricktronSelected", labor) && !UIGameObserver.bricktrons.GetScreenFlag(labor))
		{
			EnableCompass(labor);
		}
	}

	private void OnBricktronUnselected(Labor labor)
	{
		if (ValidateSignal("OnBricktronUnselected", labor))
		{
			DisableCompass(labor);
		}
	}

	private void OnBricktronEnteredScreen(Labor labor)
	{
		if (ValidateSignal("OnBricktronEnteredScreen", labor))
		{
			DisableCompass(labor);
		}
	}

	private void OnBricktronExitedScreen(Labor labor)
	{
		if (ValidateSignal("OnBricktronExitedScreen", labor) && UIGameObserver.bricktrons.GetSelectionFlag(labor))
		{
			EnableCompass(labor);
		}
	}

	private SoloCompass GetOrCreateCompass(Labor labor, out Sprite previous)
	{
		SoloCompass orCreateSoloCompass = dispenser.GetOrCreateSoloCompass(labor.gameObject, labor.Occupation.GetIcon64(), out previous);
		AddToGroup(orCreateSoloCompass, orCreateSoloCompass.icon);
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
		RemoveFromGroup(orCreateCompass, previous);
		KillCompass(labor);
	}

	private void KillCompass(Labor labor)
	{
		RemoveFromGroup(GetOrCreateCompass(labor, out var previous), previous);
		dispenser.DiscardSoloCompass(labor.gameObject, forceDestroy: true);
	}

	private void AddToGroup(SoloCompass soloCompass, Sprite icon)
	{
		if (workerGrouper.icon == icon)
		{
			workerGrouper.Add(soloCompass);
		}
		else if (knightGrouper.icon == icon)
		{
			knightGrouper.Add(soloCompass);
		}
		else if (archerGrouper.icon == icon)
		{
			archerGrouper.Add(soloCompass);
		}
	}

	private void RemoveFromGroup(SoloCompass soloCompass, Sprite icon)
	{
		if (workerGrouper.icon == icon)
		{
			workerGrouper.Remove(soloCompass);
		}
		else if (knightGrouper.icon == icon)
		{
			knightGrouper.Remove(soloCompass);
		}
		else if (archerGrouper.icon == icon)
		{
			archerGrouper.Remove(soloCompass);
		}
	}
}
