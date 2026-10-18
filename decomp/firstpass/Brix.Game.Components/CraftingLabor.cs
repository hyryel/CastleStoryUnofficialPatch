using System.Collections.Generic;
using Brix.Game.AI;
using Motus.Behavior;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components;

public class CraftingLabor : BaseLabor
{
	[SerializeField]
	private CraftingState state;

	public CraftingInstructions instructions;

	private const int MAX_ORDERS = 8;

	private ulong _recipesHash;

	private readonly List<RecipeInfo> _localRecipes = new List<RecipeInfo>();

	private Recipe _currentRecipe;

	public List<RecipeInfo> QueuedRecipes
	{
		get
		{
			if (_recipesHash != state.Hash)
			{
				_recipesHash = state.Hash;
				state.GetRecipe(_localRecipes);
			}
			return _localRecipes;
		}
	}

	public RecipeInfo FirstRecipe => QueuedRecipes.Slinq().FirstOrDefault();

	public bool LoopRecipe
	{
		get
		{
			return state.IsLoop;
		}
		set
		{
			state.IsLoop = value;
		}
	}

	public Recipe CurrentRecipe
	{
		get
		{
			return _currentRecipe;
		}
		private set
		{
			if (value != _currentRecipe)
			{
				Recipe.Dispose(_currentRecipe);
				_currentRecipe = value;
			}
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		AddOrSetSequence((StaticNode)Node.Sequence.Do((CraftingLabor aCraftingLabor) => aCraftingLabor.ProcessCurrentWorkSequence(), this), 1, "CurrentWork");
		AddOrSetSequence((StaticNode)Node.Sequence.Do((CraftingLabor aCraftingLabor) => aCraftingLabor.ProcessQueue(), this), 10, "Queue");
		AddOrSetSequence((StaticNode)Node.Sequence.Do((CraftingLabor aCraftingLabor) => aCraftingLabor.ProcessPendingWorkSequence(), this), 1, "PendingWork");
	}

	public void Init(CraftingInstructions instructions)
	{
		this.instructions = instructions;
	}

	protected StaticNode ProcessQueue()
	{
		return Node.Selector.If((CraftingLabor aCraftingLabor) => aCraftingLabor.CurrentWork == null && aCraftingLabor.FirstRecipe != null, this).Do(delegate(CraftingLabor aCraftingLabor)
		{
			aCraftingLabor.PopRecipe();
		}, this).Do(delegate(CraftingLabor aCraftingLabor)
		{
			aCraftingLabor.instructions.CookPackage(aCraftingLabor.CurrentRecipe).Send(aCraftingLabor);
		}, this);
	}

	private void PopRecipe()
	{
		CurrentRecipe = FirstRecipe.GetRecipe();
	}

	public void QueueRecipe(RecipeInfo recipe, int qty = 1)
	{
		for (int i = 0; i < qty; i++)
		{
			if (QueueCount() >= 8)
			{
				break;
			}
			QueuedRecipes.Add(recipe);
		}
		state.SetRecipes(QueuedRecipes);
	}

	public void ProduceRecipe(RecipeInfo recipe)
	{
		ClearOrderQueue();
		QueuedRecipes.Add(recipe);
		LoopRecipe = true;
		state.SetRecipes(QueuedRecipes);
	}

	private void RemoveRecipe(RecipeInfo recipe)
	{
		QueuedRecipes.Remove(recipe);
		state.SetRecipes(QueuedRecipes);
	}

	public void RemoveRecipeAt(int i)
	{
		if (i == 0)
		{
			base.ClearOrderQueue();
			CurrentRecipe = null;
		}
		QueuedRecipes.RemoveAt(i);
		state.SetRecipes(QueuedRecipes);
	}

	public override void ClearOrderQueue()
	{
		base.ClearOrderQueue();
		QueuedRecipes.Clear();
		state.SetRecipes(QueuedRecipes);
		CurrentRecipe = null;
		LoopRecipe = false;
	}

	public void FinishRecipe(Recipe recipe)
	{
		recipe.Finish(base.recepteur);
		RemoveRecipe(recipe.Info);
		if (LoopRecipe)
		{
			QueueRecipe(recipe.Info);
		}
		CurrentRecipe = null;
	}

	public int QueueCount()
	{
		return QueuedRecipes.Count;
	}
}
