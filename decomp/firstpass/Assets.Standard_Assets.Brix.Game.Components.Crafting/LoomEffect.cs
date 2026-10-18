using Brix.External.Factories;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Semantique;
using UnityEngine;

namespace Assets.Standard_Assets.Brix.Game.Components.Crafting;

internal class LoomEffect : MonoBehaviour
{
	[SerializeField]
	private LoomAnimatorInterface loom;

	public void RefreshContent(Recepteur r, RecipeInfo current)
	{
		loom.ShowRope(DisplayMode(r, current, BuildResources.Rope));
		loom.ShowFabric(DisplayMode(r, current, BuildResources.Fabric));
		loom.FillAt(r.GetProportionUsed(Adjectif.plant));
	}

	public void Work(RecipeInfo current)
	{
		loom.SpinRope(current.Creates == BuildResources.Rope);
		loom.SpinFabric(current.Creates == BuildResources.Fabric);
	}

	public void StopWork()
	{
		loom.SpinFabric(value: false);
		loom.SpinRope(value: false);
	}

	private LoomAnimatorInterface.DisplayState DisplayMode(Recepteur recepteur, RecipeInfo recipe, Factory.AssetKey key)
	{
		if (recepteur.Has(key))
		{
			return LoomAnimatorInterface.DisplayState.Item;
		}
		if (recipe != null && recipe.Creates == key)
		{
			return LoomAnimatorInterface.DisplayState.Blueprint;
		}
		return LoomAnimatorInterface.DisplayState.None;
	}
}
