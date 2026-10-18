using Brix.External.Factories;
using Brix.Game.AI;
using Brix.UI.Icons;

namespace Brix.Game.Components;

public class RecipeMaker
{
	private Description Ingredients = new Description();

	public int Heat;

	public int Work;

	public Factory.AssetKey Creates;

	public int QteCreated = 1;

	public IconKey Icon = IconKeys.Missing;

	public IconKey Drawing = IconKeys.Missing;

	public string Sound = string.Empty;

	public bool DropOnCompletion;

	public RecipeMaker With(Ressource resource, int qty)
	{
		Adjectif adjectif = resource.Clone();
		adjectif.quantifiable.valeur = qty;
		Ingredients.Add(adjectif);
		return this;
	}

	public RecipeInfo Make()
	{
		int nextID = CookBook.GetNextID();
		RecipeInfo recipeInfo = new RecipeInfo(nextID, Ingredients.Clone(), Heat, Work, Creates, QteCreated, DropOnCompletion, Icon, Drawing);
		CookBook.Register(recipeInfo);
		return recipeInfo;
	}
}
