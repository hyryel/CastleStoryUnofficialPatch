using System.Collections.Generic;
using Brix.External.Factories;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Semantique;
using Brix.Game.Storage;
using Brix.Utils.Signals;
using Smooth.Slinq;
using UnityEngine;

public class CraftingInventory
{
	private Recepteur recepteur;

	private Description totalRequiredIngredients = new Description();

	private Description ingredientsToFetch = new Description();

	private Description toStore = new Description();

	private Description resources = new Description();

	private HashSet<Factory.AssetKey> uniqueFetcheable = new HashSet<Factory.AssetKey>();

	private HashSet<Factory.AssetKey> uniqueStorable = new HashSet<Factory.AssetKey>();

	private HashSet<Factory.AssetKey> uniqueResources = new HashSet<Factory.AssetKey>();

	private bool needsHeat;

	private readonly int _resourceCap;

	private List<RecipeInfo> uniqueRecipe = new List<RecipeInfo>();

	private Description content => recepteur.ContentDescription;

	public CraftingInventory(Recepteur recepteur)
	{
		this.recepteur = recepteur;
		this.recepteur.InventoryChanged.Connect(delegate
		{
			RefreshAmounts();
		}).DisconnectWhen(recepteur.OnReleaseOrDestroySignal);
		_resourceCap = (from a in recepteur.BaseCapacity.Adjectifs
			where a is Ressource
			select a.quantifiable.valeur).Max();
	}

	public void SetRecipes(List<RecipeInfo> recipes)
	{
		totalRequiredIngredients.Clear();
		foreach (RecipeInfo recipe in recipes)
		{
			recipe.AddTo(totalRequiredIngredients);
		}
		needsHeat = recipes.Slinq().Any((RecipeInfo r) => r.heat > 0);
		RefreshAmounts();
	}

	public void SetRecipe(RecipeInfo recipe)
	{
		uniqueRecipe.Clear();
		uniqueRecipe.Add(recipe);
		SetRecipes(uniqueRecipe);
	}

	public void Clear()
	{
		uniqueRecipe.Clear();
		SetRecipes(uniqueRecipe);
	}

	private void RefreshAmounts()
	{
		ingredientsToFetch = (totalRequiredIngredients.CapAt(_resourceCap) - content).RemoveZeros();
		toStore.Clear();
		toStore.AddAllFrom(content);
		if (needsHeat)
		{
			toStore.Remove(Adjectif.orangeCrystal);
		}
		if (totalRequiredIngredients.Any())
		{
			toStore.ExceptWith(totalRequiredIngredients).RemoveZeros().RessourceOnly();
		}
		resources.Clear();
		resources.AddAllFrom(content);
		if (totalRequiredIngredients.Any())
		{
			resources.ExceptWith(toStore).RemoveZeros().RessourceOnly();
		}
		uniqueFetcheable.Clear();
		uniqueStorable.Clear();
		uniqueResources.Clear();
		ingredientsToFetch.UniqueRepresentatives(ref uniqueFetcheable);
		toStore.UniqueRepresentatives(ref uniqueStorable);
		resources.UniqueRepresentatives(ref uniqueResources);
	}

	public bool ItemToPickup(out GameObject go)
	{
		return recepteur.FirstObjectOf(uniqueStorable, out go);
	}

	public bool HasProductsOfType(Factory.AssetKey asset)
	{
		return uniqueStorable.Contains(asset) && recepteur.Has(asset);
	}

	public bool ResourceToRemove(out GameObject go)
	{
		return recepteur.FirstObjectOf(uniqueResources, out go);
	}

	public Description StuffToFecth()
	{
		return ingredientsToFetch;
	}

	public bool RequiresMoreRessources()
	{
		return uniqueFetcheable.Count > 0;
	}

	public bool CanBeFulfilledBy(CompoundRecepteur compound)
	{
		return compound.HasRecepteursWithOneOf(uniqueFetcheable);
	}

	public bool NeedsToStore()
	{
		return uniqueStorable.Count > 0;
	}

	public bool CanStoreIn(CompoundRecepteur compound)
	{
		return compound.RecepteursWithSpaceForOneOfSlinq(uniqueStorable).Any((Recepteur r, Recepteur inventory) => r != inventory, recepteur);
	}
}
