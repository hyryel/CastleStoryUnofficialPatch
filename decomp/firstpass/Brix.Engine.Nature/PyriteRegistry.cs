using System.Collections.Generic;
using Brix.Components;
using UnityEngine;

namespace Brix.Engine.Nature;

public class PyriteRegistry : BrixSingleton<PyriteRegistry>
{
	private PyriteRegistryImpl _internalRegistry = new PyriteRegistryImpl();

	public Texture2D GrowthNoiseTex;

	public int GrowthNoiseTexRepeats = 4;

	public const float HealingPerSecond = 5f;

	public static void AddAllMineralPositionsToList(ref List<XYZ> list)
	{
		BrixSingleton<PyriteRegistry>.Instance._internalRegistry.AddAllMineralPositionsToList(ref list);
	}

	public static bool HasMinerals(XYZ p)
	{
		return BrixSingleton<PyriteRegistry>.Instance._internalRegistry.HasMinerals(p);
	}

	public static MineralData GetMineral(XYZ p)
	{
		return BrixSingleton<PyriteRegistry>.Instance._internalRegistry.GetMineral(p);
	}

	public static void Add(XYZ position, float growth)
	{
		BrixSingleton<PyriteRegistry>.Instance._internalRegistry.Add(position, growth);
	}

	public static void Remove(XYZ position)
	{
		BrixSingleton<PyriteRegistry>.Instance._internalRegistry.Remove(position);
	}

	public static bool AddOrGrow(XYZ position, float growth)
	{
		return BrixSingleton<PyriteRegistry>.Instance._internalRegistry.AddOrGrow(position, growth);
	}

	public static bool RemoveOrUngrow(XYZ position, float ungrowth)
	{
		return BrixSingleton<PyriteRegistry>.Instance._internalRegistry.RemoveOrUngrow(position, ungrowth);
	}

	public static void TranslateAllMinerals(XYZ delta)
	{
		BrixSingleton<PyriteRegistry>.Instance._internalRegistry.TranslateAllMinerals(delta);
	}

	public static MineralSerializedData[] GetSerializedMinerals()
	{
		return BrixSingleton<PyriteRegistry>.Instance._internalRegistry.GetSerializedMinerals();
	}

	public static void BuildMineralsFromSerialized(MineralSerializedData[] serializedMinerals)
	{
		BrixSingleton<PyriteRegistry>.Instance._internalRegistry.BuildMineralsFromSerialized(serializedMinerals);
	}

	public static void DamageAt(XYZ p, float amount)
	{
		BrixSingleton<PyriteRegistry>.Instance._internalRegistry.DamageAt(p, amount);
	}

	public static float GetMaxHp(XYZ p)
	{
		return BrixSingleton<PyriteRegistry>.Instance._internalRegistry.GetMaxHp(p);
	}

	public static void BreakAt(XYZ p)
	{
		BrixSingleton<PyriteRegistry>.Instance._internalRegistry.BreakAt(p);
	}

	public static float GetDamage(XYZ p)
	{
		return BrixSingleton<PyriteRegistry>.Instance._internalRegistry.GetDamage(p);
	}

	public static float GetCurrentHp(XYZ p)
	{
		return BrixSingleton<PyriteRegistry>.Instance._internalRegistry.GetCurrentHp(p);
	}

	public static void Harvest(XYZ p)
	{
		BrixSingleton<PyriteRegistry>.Instance._internalRegistry.Harvest(p);
	}

	private void Update()
	{
		BrixSingleton<PyriteRegistry>.Instance._internalRegistry.RegenerateMinerals(Time.deltaTime * 5f);
	}
}
