using Brix.Game.Components;
using UnityEngine;

namespace Brix.Input;

public class CompoundPlacementOptions : IPickerComponentOptions
{
	public BlueprintCompoundComponent Compound;

	public bool Instant;

	public CompoundPlacementOptions(GameObject compound)
	{
		Compound = compound.GetComponent<BlueprintCompoundComponent>();
	}
}
