using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.External.Factories;
using Brix.Game.AI;
using Brix.UI.Icons;
using Brix.Utils;
using Smooth.Algebraics;
using Smooth.Slinq;

namespace Brix.Game.Components;

public class RecipeInfo
{
	public readonly int id;

	protected readonly Description ingredients;

	public readonly int heat;

	public readonly int work;

	public readonly Factory.AssetKey Creates;

	public readonly int QtyCreated = 1;

	public readonly bool DropOnCompletion;

	public readonly IconKey icon;

	public readonly IconKey drawing;

	public RecipeInfo(int id, Description ingredients, int heat, int work, Factory.AssetKey assetKey, int qteCreated, bool dropOnCompletion, IconKey icon, IconKey drawing)
	{
		this.id = id;
		this.ingredients = ingredients;
		this.heat = heat;
		this.work = work;
		Creates = assetKey;
		QtyCreated = qteCreated;
		DropOnCompletion = dropOnCompletion;
		this.icon = icon;
		this.drawing = drawing;
	}

	public void AddTo(Description desc)
	{
		desc.AddAllFrom(ingredients);
	}

	public IEnumerable<Tuple<AdjectiveKey, int>> Ingredients()
	{
		return ingredients.Adjectifs.Select((Adjectif a) => Tuple.Create(a.GetKey(), a.quantifiable.valeur)).NoLeak();
	}

	public Recipe GetRecipe()
	{
		Recipe recipe = ObjectPool<Recipe>.Reserve();
		recipe.Set(this, heat, work, finished: false, ingredients.Clone());
		return recipe;
	}
}
