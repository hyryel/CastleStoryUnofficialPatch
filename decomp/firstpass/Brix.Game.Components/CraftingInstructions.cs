using System;
using Brix.Game.AI;
using Brix.Game.AI.InstructionType;
using Brix.Game.AI.Nodes;
using Brix.Game.Rules;
using Brix.Game.Semantique;
using Brix.UI.Icons;
using Motus.Behavior;

namespace Brix.Game.Components;

public class CraftingInstructions
{
	private readonly CraftingStation station;

	private readonly WorkInstruction<CraftingLabor, Recipe> Cook;

	private readonly LaborInstruction<Description> FindIngredients;

	private readonly LaborInstruction<Recipe> WorkOnRecipe;

	private readonly WorkNodeGenerator<CraftingStation> nodeGenerator;

	public CraftingInstructions(CraftingStation station)
	{
		this.station = station;
		Cook = new WorkInstruction<CraftingLabor, Recipe>("Cook", IconKeys.Block, (CraftingLabor labor, Recipe recipe) => CookRecipeNode(recipe));
		FindIngredients = new OperableInstruction<Description>("FindIngredients", IconKeys.Block, FindResource, station);
		WorkOnRecipe = new OperableInstruction<Recipe>("WorkOnRecipe", IconKeys.HammerGroup, (Labor labor, Recipe recipe) => GetWorkOnReceipeNode(labor), station);
		nodeGenerator = SetupNodeGenerator();
	}

	public OrderPackage<CraftingLabor, Recipe> CookPackage(Recipe recipe)
	{
		return new OrderPackage<CraftingLabor, Recipe>(Cook, recipe);
	}

	public OrderPackage<Labor> FindPackage()
	{
		return Order.Package.OfDescription(FindIngredients, station.StuffToFetch(), Labor.DecisionSource.Project);
	}

	public OrderPackage<Labor> WorkPackage(Recipe recipe)
	{
		return Order.Package.OfRecipe(WorkOnRecipe, recipe, Labor.DecisionSource.Project);
	}

	private Node FindResource(Labor labor, Description ingredients)
	{
		return (StaticNode)Node.Sequence.Do((Labor _labor, Description _ingredients) => _labor.FindAndPickUp(_ingredients), labor, ingredients).Do((Func<Labor, Recepteur, Node>)StoreNode.GoStoreIn, labor, station.recepteur).Label("GoStore");
	}

	private unsafe Node CookRecipeNode(Recipe recipe)
	{
		Node b = (StaticNode)Node.Selector.Label("CookRecipe selector").If(() => recipe.IsReadyToFinish(station.recepteur) && (recipe.DropOnCompletion || station.recepteur.HasRoomFor(recipe.Creates))).Label("If ready to produce")
			.Do(delegate
			{
				station.Finish(recipe);
			})
			.Label("Produce recipe")
			.Do(Node.Break())
			.Label("Break")
			.If(() => recipe.HasIngredientsIn(station.recepteur) && recipe.NeedsHeat && recipe.HeatFrom(station.recepteur))
			.Label("If needs heat")
			.Do((CraftingInstructions self) => self.Process(GeneralRules.BrimstoneBurnTimeMs), this)
			.Label("Burn")
			.Else()
			.Label("Else")
			.Do(new Func<StaticNode>(null, (nint)(delegate*<StaticNode>)(&Node.get_Empty)))
			.Label("Trigger loop");
		return (StaticNode)b.Loop().WithDelay(30L).Label("CookRecipe Loop");
	}

	private Node Process(int ms)
	{
		station.Process(ms);
		return Node.WaitFor.Milliseconds(ms);
	}

	private Node GetWorkOnReceipeNode(Labor labor)
	{
		return nodeGenerator.BuildNode(station, labor);
	}

	private WorkNodeGenerator<CraftingStation> SetupNodeGenerator()
	{
		WorkNodeGenerator<CraftingStation> workNodeGenerator = new WorkNodeGenerator<CraftingStation>();
		workNodeGenerator.LoopAction = true;
		workNodeGenerator.WorkAction = (CraftingStation furnace, Labor worker) => (StaticNode)Node.Sequence.Do((CraftingStation g) => Work(furnace), furnace);
		workNodeGenerator.Animation = station.WorkAnimation();
		workNodeGenerator.ActionWrappers.Add(WrapNode);
		workNodeGenerator.GetLocation = (CraftingStation furnace) => new OperableLocation(furnace);
		workNodeGenerator.WorkDoneCondition = WorkDone;
		workNodeGenerator.ReleaseWhen = ReleaseNodeWhen.Dont;
		return workNodeGenerator;
	}

	private bool WorkDone(CraftingStation station, Labor ignored)
	{
		return station.IsNullOrReleased() || station.CurrentRecipe == null || !station.CurrentRecipe.NeedsWork;
	}

	private Node WrapNode(Node node, CraftingStation station, Labor labor)
	{
		return (StaticNode)Node.Sequence.Do(delegate(CraftingStation s)
		{
			s.WorkStarted();
		}, station).Do(node as StaticNode).Finally(delegate(CraftingStation s)
		{
			if (!s.IsNullOrReleased())
			{
				s.WorkEnded();
			}
		}, station);
	}

	private Node Work(CraftingStation station)
	{
		station.CurrentRecipe.WorkOn();
		station.WorkContinued();
		return Node.Empty;
	}
}
