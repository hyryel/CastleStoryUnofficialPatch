using System;
using Brix.External.Signals;
using UnityEngine;

namespace Brix.Game.Names;

public class NameDispenserGO : MonoBehaviour
{
	public enum Dispensers
	{
		Bricktrons,
		Corruptrons,
		Biftrons,
		Minitrons
	}

	public Dispensers dispenserSelection;

	private INameDispenser dispenser;

	public string namePath;

	public static NameDispenserGO GetDispenserGO(Dispensers disp)
	{
		if (disp == Dispensers.Bricktrons)
		{
			return GameObject.Find("NameDispenserBricktron").GetComponent<NameDispenserGO>();
		}
		throw new Exception("Undefined Name Dispenser");
	}

	private void OnDestroy()
	{
		if (dispenser != null)
		{
			dispenser.Destroy();
			dispenser = null;
		}
	}

	private INameDispenser GetDispenser(Dispensers disp)
	{
		if (disp == Dispensers.Bricktrons)
		{
			return new NameDispenserBricktron(namePath);
		}
		throw new Exception("No NameGenerator Defined");
	}

	private INameDispenser Dispenser()
	{
		if (dispenser == null)
		{
			dispenser = GetDispenser(dispenserSelection);
		}
		return dispenser;
	}

	public string GetName()
	{
		return Dispenser().GetName();
	}

	public Signal<string> GetSignal()
	{
		return Dispenser().GetNewNameSignal();
	}
}
