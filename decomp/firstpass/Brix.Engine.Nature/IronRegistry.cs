using System.Collections.Generic;
using Brix.Components;
using UnityEngine;

namespace Brix.Engine.Nature;

public class IronRegistry : BrixSingleton<IronRegistry>
{
	private IronRegistryImpl _internalRegistry = new IronRegistryImpl();

	public Texture2D GrowthNoiseTex;

	public int GrowthNoiseTexRepeats = 4;

	public const float HealingPerSecond = 5f;

	public static void AddAllMineralPositionsToList(ref List<XYZ> list)
	{
		BrixSingleton<IronRegistry>.Instance._internalRegistry.AddAllMineralPositionsToList(ref list);
	}

	public static bool HasMinerals(XYZ p)
	{
		return BrixSingleton<IronRegistry>.Instance._internalRegistry.HasMinerals(p);
	}

	public static MineralData GetMineral(XYZ p)
	{
		return BrixSingleton<IronRegistry>.Instance._internalRegistry.GetMineral(p);
	}

	public static void Add(XYZ position, float growth)
	{
		BrixSingleton<IronRegistry>.Instance._internalRegistry.Add(position, growth);
	}

	public static void Remove(XYZ position)
	{
		BrixSingleton<IronRegistry>.Instance._internalRegistry.Remove(position);
	}

	public static bool AddOrGrow(XYZ position, float growth)
	{
		return BrixSingleton<IronRegistry>.Instance._internalRegistry.AddOrGrow(position, growth);
	}

	public static bool RemoveOrUngrow(XYZ position, float ungrowth)
	{
		return BrixSingleton<IronRegistry>.Instance._internalRegistry.RemoveOrUngrow(position, ungrowth);
	}

	public static void TranslateAllMinerals(XYZ delta)
	{
		BrixSingleton<IronRegistry>.Instance._internalRegistry.TranslateAllMinerals(delta);
	}

	public static MineralSerializedData[] GetSerializedMinerals()
	{
		return BrixSingleton<IronRegistry>.Instance._internalRegistry.GetSerializedMinerals();
	}

	public static void BuildMineralsFromSerialized(MineralSerializedData[] serializedMinerals)
	{
		BrixSingleton<IronRegistry>.Instance._internalRegistry.BuildMineralsFromSerialized(serializedMinerals);
	}

	public static void DamageAt(XYZ p, float amount)
	{
		BrixSingleton<IronRegistry>.Instance._internalRegistry.DamageAt(p, amount);
	}

	public static void BreakAt(XYZ p)
	{
		BrixSingleton<IronRegistry>.Instance._internalRegistry.BreakAt(p);
	}

	public static float GetDamage(XYZ p)
	{
		return BrixSingleton<IronRegistry>.Instance._internalRegistry.GetDamage(p);
	}

	public static float GetMaxHp(XYZ p)
	{
		return BrixSingleton<IronRegistry>.Instance._internalRegistry.GetMaxHp(p);
	}

	public static float GetCurrentHp(XYZ p)
	{
		return BrixSingleton<IronRegistry>.Instance._internalRegistry.GetCurrentHp(p);
	}

	public static void Harvest(XYZ p)
	{
		BrixSingleton<IronRegistry>.Instance._internalRegistry.Harvest(p);
	}

	private void Update()
	{
		BrixSingleton<IronRegistry>.Instance._internalRegistry.RegenerateMinerals(Time.deltaTime * 5f);
	}
}
