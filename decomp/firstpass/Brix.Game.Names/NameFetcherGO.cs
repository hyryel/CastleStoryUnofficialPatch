using UnityEngine;

namespace Brix.Game.Names;

public class NameFetcherGO : MonoBehaviour
{
	public NameDispenserGO.Dispensers dispenserSelection;

	private NameDispenserGO dispenser;

	private NameDispenserGO GetDispenser()
	{
		if (dispenser == null)
		{
			dispenser = NameDispenserGO.GetDispenserGO(NameDispenserGO.Dispensers.Bricktrons);
		}
		return dispenser;
	}

	public string GetName()
	{
		return GetDispenser().GetName();
	}
}
