using System.Collections.Generic;
using System.Linq;
using Brix.Components;
using Brix.External.Factories;
using Brix.Legacy;
using Brix.Util;
using Brix.Utils;
using Newtonsoft.Json;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;

namespace Brix.Engine.Nature;

[JsonObject(MemberSerialization.OptIn)]
public class PlantRegistry : BrixSingleton<PlantRegistry>
{
	private readonly Dictionary<XYZ, PlantData> _data = new Dictionary<XYZ, PlantData>();

	private readonly Dictionary<XYZ, HashSet<XYZ>> _mvPositions = new Dictionary<XYZ, HashSet<XYZ>>();

	public static XYZ MvSize = XYZ.one * 16;

	public const int GarnottesPerUnit = 2;

	[JsonProperty]
	public PlantSerializedData[] SerializedData
	{
		get
		{
			return _data.Select(ExtractData).ToArray();
		}
		set
		{
			Clear();
			value?.Slinq().ForEach(delegate(PlantSerializedData entry)
			{
				Add(ExtractPosition(entry), entry.t, entry.s);
			});
		}
	}

	public static bool IsPlant(XYZ p)
	{
		return BrixSingleton<PlantRegistry>.Instance._data.ContainsKey(p);
	}

	public static PlantData PlantAt(XYZ p)
	{
		return BrixSingleton<PlantRegistry>.Instance._data.GetValueOrDefault(p);
	}

	public static void Add(XYZ p, int plantType, float growth = 1f)
	{
		if (!NatureRegistry.HasNatureAt(p))
		{
			PlantData plantData = PlantData.Randomized(p, plantType);
			plantData.Growth = growth;
			BrixSingleton<PlantRegistry>.Instance._data.Add(p, plantData);
			BrixSingleton<PlantRegistry>.Instance._mvPositions.GetValueOrCreate(XYZ.Div(p, MvSize)).Add(p);
			GameSignals.Invoke(GameSignals.PlantChanged, p);
		}
	}

	public static void Remove(XYZ p)
	{
		if (BrixSingleton<PlantRegistry>.Instance._data.ContainsKey(p))
		{
			BrixSingleton<PlantRegistry>.Instance._data.Remove(p);
			HashSet<XYZ> valueOrDefault = BrixSingleton<PlantRegistry>.Instance._mvPositions.GetValueOrDefault(XYZ.Div(p, MvSize));
			if (valueOrDefault != null && BrixSingleton<PlantRegistry>.Instance._mvPositions.Remove(p) && BrixSingleton<PlantRegistry>.Instance._mvPositions.Count == 0)
			{
				BrixSingleton<PlantRegistry>.Instance._mvPositions.Remove(XYZ.Div(p, MvSize));
			}
			GameSignals.Invoke(GameSignals.PlantChanged, p);
		}
	}

	public static bool AddOrGrow(XYZ p, int plantType, float growth = 1f)
	{
		bool result = false;
		if (!BrixSingleton<PlantRegistry>.Instance._data.TryGetValue(p, out var value))
		{
			Add(p, plantType, growth);
			result = true;
		}
		else
		{
			float growth2 = value.Growth;
			value.Growth += growth;
			if (value.Growth != growth2)
			{
				result = true;
			}
			GameSignals.Invoke(GameSignals.PlantChanged, p);
		}
		return result;
	}

	public static bool RemoveOrUngrow(XYZ p, float ungrowth = 1f)
	{
		if (!BrixSingleton<PlantRegistry>.Instance._data.TryGetValue(p, out var value))
		{
			return false;
		}
		value.Growth -= ungrowth;
		if (value.Growth <= 0f)
		{
			Remove(p);
		}
		else
		{
			GameSignals.Invoke(GameSignals.PlantChanged, p);
		}
		return true;
	}

	public static void TranslateAllPlants(XYZ delta)
	{
		List<XYZ> list = BrixSingleton<PlantRegistry>.Instance._data.Keys.ToList();
		Dictionary<XYZ, PlantData> dictionary = BrixSingleton<PlantRegistry>.Instance._data.ToDictionary((KeyValuePair<XYZ, PlantData> kv) => kv.Key + delta, (KeyValuePair<XYZ, PlantData> kv) => kv.Value);
		foreach (XYZ item in list)
		{
			Remove(item);
		}
		foreach (KeyValuePair<XYZ, PlantData> item2 in dictionary)
		{
			Add(item2.Key, item2.Value.PlantType, item2.Value.Growth);
		}
	}

	public static Vector3 ExactPositionOfPlantAt(XYZ position)
	{
		if (!IsPlant(position))
		{
			return Vector3.zero;
		}
		return PlantAt(position).Displacement + position.ToVector3();
	}

	private static PlantSerializedData ExtractData(KeyValuePair<XYZ, PlantData> kv)
	{
		XYZ key = kv.Key;
		float growth = kv.Value.Growth;
		int plantType = kv.Value.PlantType;
		return new PlantSerializedData
		{
			x = key.x,
			y = key.y,
			z = key.z,
			s = growth,
			t = plantType
		};
	}

	private static XYZ ExtractPosition(PlantSerializedData entry)
	{
		return new XYZ(entry.x, entry.y, entry.z);
	}

	public static void Break(PlantData plant)
	{
		if (plant != null)
		{
			Remove(plant.RootPos);
			SpawnGarnottes(plant);
		}
	}

	public static void BreakAt(XYZ p)
	{
		PlantData valueOrDefault = BrixSingleton<PlantRegistry>.Instance._data.GetValueOrDefault(p);
		Break(valueOrDefault);
	}

	protected static void SpawnGarnottes(PlantData plant)
	{
		Garnotte.Nouvelle(plant.RootPos.ToVector3() + new Vector3(0f, 0.5f, 0f), 2, RawResources.RawFiber);
	}

	public Slinq<XYZ, PredicateContext<XYZ, SelectSlinqContext<XYZ, HashSetSlinqContext<XYZ>, XYZ, IEnumerableContext<XYZ>, Dictionary<XYZ, HashSet<XYZ>>>, int>> PlantsPositionsInRange(XYZ center, XYZ extents)
	{
		XYZ min = XYZ.Div(center - extents, MvSize);
		XYZ max = XYZ.Div(center + extents + MvSize - XYZ.one, MvSize);
		return Calcule.VolumeBetween(min, max).Slinq().SelectMany((XYZ q, Dictionary<XYZ, HashSet<XYZ>> mvs) => _mvPositions.GetValueOrDefault(q, Empty.HashSet<XYZ>()).Slinq(), _mvPositions)
			.Where((XYZ p, int d2) => XYZ.SqrMagnitude(p - center) <= d2, XYZ.SqrMagnitude(extents));
	}

	[ContextMenu("Fill Map")]
	private void FillMap()
	{
		foreach (XYZ topVoxelPositionsFromLayer in Voxel.Engine.GetTopVoxelPositionsFromLayers())
		{
			if (Voxel.GetBlockInfo(topVoxelPositionsFromLayer).ID == 2)
			{
				AddOrGrow(topVoxelPositionsFromLayer, 0);
			}
		}
	}

	[ContextMenu("Clear Now")]
	private void Clear()
	{
		XYZ[] array = _data.Keys.ToArray();
		_data.Clear();
		_mvPositions.Clear();
		XYZ[] array2 = array;
		foreach (XYZ t in array2)
		{
			GameSignals.Invoke(GameSignals.PlantChanged, t);
		}
	}
}
