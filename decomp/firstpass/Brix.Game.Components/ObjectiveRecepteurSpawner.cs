using Brix.External.Factories;
using Brix.Game.AI;
using Brix.Game.Semantique;
using Brix.Lua;
using Brix.Transactions;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.Game.Components;

internal class ObjectiveRecepteurSpawner : MonoBehaviour
{
	private Recepteur rec;

	[LuaDrivable("HasPumpkin")]
	private bool HasPumpkin
	{
		get
		{
			Pumpkin pumpkin = theRecepteur.ContentDescription.Get<Pumpkin>();
			return pumpkin != null;
		}
	}

	[LuaDrivable("HasCrystal")]
	private bool HasCrystal
	{
		get
		{
			CrystalObjective crystalObjective = theRecepteur.ContentDescription.Get<CrystalObjective>();
			return crystalObjective != null;
		}
	}

	private Recepteur theRecepteur
	{
		get
		{
			if (rec == null)
			{
				rec = GetComponent<Recepteur>();
			}
			return rec;
		}
	}

	[LuaDrivable("SpawnPumpkin")]
	private static DynValue SpawnPumpkin(ScriptExecutionContext context, CallbackArguments args)
	{
		ObjectiveRecepteurSpawner objectiveRecepteurSpawner = args.AsComponent<ObjectiveRecepteurSpawner>(0, "SpawnPumpkin");
		GameObject item = Transactor.SpawnUNet(BuildResources.Pumpkin, Faction.DefaultNeutral, objectiveRecepteurSpawner.transform.position, Quaternion.identity);
		objectiveRecepteurSpawner.theRecepteur.AddObject(item);
		return DynValue.Void;
	}

	[LuaDrivable("SpawnCrystal")]
	private static DynValue SpawnCrystal(ScriptExecutionContext context, CallbackArguments args)
	{
		ObjectiveRecepteurSpawner objectiveRecepteurSpawner = args.AsComponent<ObjectiveRecepteurSpawner>(0, "SpawnCrystal");
		GameObject item = Transactor.SpawnUNet(BuildResources.Crystal, Faction.DefaultNeutral, objectiveRecepteurSpawner.transform.position, Quaternion.identity);
		objectiveRecepteurSpawner.theRecepteur.AddObject(item);
		return DynValue.Void;
	}
}
