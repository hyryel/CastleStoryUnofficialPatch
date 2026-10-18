using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.External.Factories;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Semantique;
using Brix.Utils;
using MoonSharp.Interpreter;
using Smooth.Algebraics;
using Smooth.Slinq;

namespace Brix.Lua;

public static class LuaExtenders
{
	public static DynValue ToDynTable(this Description desc, Script script, int emptyCount)
	{
		DynValue result = DynValue.NewTable(script);
		HashSet<Factory.AssetKey> hashset = CollectionPool.RequestHashSet<Factory.AssetKey>();
		foreach (Ressource item in desc.GetRessources().NoLeak())
		{
			if (item.valeur != 0)
			{
				DynValue key = DynValue.FromObject(script, LuaCrafting.ToLuaResource(item));
				result.Table.Set(key, DynValue.NewNumber(item.valeur));
			}
		}
		hashset.Dispose();
		result.Table.Set(LuaCrafting.LuaResource.None, DynValue.NewNumber(emptyCount));
		return result;
	}

	public static DynValue ToDynValue(this List<RecipeInfo> recipes, Script script)
	{
		Dictionary<AdjectiveKey, int> dictionary = CollectionPool.RequestDictionary<AdjectiveKey, int>();
		Dictionary<AdjectiveKey, int> dictionary2 = CollectionPool.RequestDictionary<AdjectiveKey, int>();
		foreach (RecipeInfo recipe in recipes)
		{
			foreach (Tuple<AdjectiveKey, int> item in recipe.Ingredients().Slinq().NoLeak())
			{
				AddTo(dictionary, item._1, item._2);
			}
			if (recipe.heat > 0)
			{
				AddTo(dictionary, Adjectif.orangeCrystal.GetKey(), recipe.heat);
			}
			Ressource ressource = Apparence.PrincipaleOf(Factory.Peek(recipe.Creates));
			AddTo(dictionary2, ressource.GetKey(), recipe.QtyCreated);
		}
		DynValue value = DictToDyn(dictionary, script);
		DynValue value2 = DictToDyn(dictionary2, script);
		dictionary.Dispose();
		dictionary2.Dispose();
		DynValue result = DynValue.NewTable(script);
		result.Table.Set("input", value);
		result.Table.Set("output", value2);
		return result;
	}

	private static DynValue DictToDyn(Dictionary<AdjectiveKey, int> dict, Script script)
	{
		DynValue result = DynValue.NewTable(script);
		foreach (KeyValuePair<AdjectiveKey, int> item in dict)
		{
			DynValue key = DynValue.FromObject(script, LuaCrafting.ToLuaResource(item.Key));
			result.Table.Set(key, DynValue.NewNumber(item.Value));
		}
		return result;
	}

	public static void AddTo<T>(Dictionary<T, int> dict, T index, int value)
	{
		int valueOrCreate = dict.GetValueOrCreate(index);
		dict[index] = valueOrCreate + value;
	}
}
