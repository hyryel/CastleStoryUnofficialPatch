using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Util;
using Newtonsoft.Json;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Engine;

[JsonObject(MemberSerialization.OptOut)]
public class VoxelTerrainResources
{
	public int[] types = new int[0];

	public int[] amounts = new int[0];

	public const int MaxAmountPerVoxel = 10;

	public int MostCommonType => types[Calcule.ArgMax(amounts)];

	public int TotalAmount => amounts.Slinq().Sum();

	public int TotalVolume
	{
		get
		{
			int totalAmount = TotalAmount;
			return (totalAmount > 0) ? Calcule.RemapMinMax(totalAmount, 1, 10, 1, 4) : 0;
		}
	}

	public IEnumerable<KeyValuePair<int, int>> All => types.Select((int t, int i) => new KeyValuePair<int, int>(t, amounts[i]));

	public void Clear()
	{
		types = new int[0];
		amounts = new int[0];
	}

	public void Add(int type, int amount)
	{
		for (int i = 0; i < types.Length; i++)
		{
			if (types[i] == type)
			{
				amounts[i] = Mathf.Clamp(amounts[i] + amount, 0, 10);
				return;
			}
		}
		Array.Resize(ref types, types.Length + 1);
		Array.Resize(ref amounts, amounts.Length + 1);
		types[types.Length - 1] = type;
		amounts[amounts.Length - 1] = Mathf.Clamp(amount, 0, 10);
	}

	public void Convert(Dictionary<int, int> fromToDico, bool allowDuplicates = false)
	{
		if (allowDuplicates)
		{
			for (int i = 0; i < types.Length; i++)
			{
				if (fromToDico.TryGetValue(types[i], out var value))
				{
					types[i] = value;
				}
			}
			return;
		}
		KeyValuePair<int, int>[] array = All.ToArray();
		Clear();
		KeyValuePair<int, int>[] array2 = array;
		for (int j = 0; j < array2.Length; j++)
		{
			KeyValuePair<int, int> keyValuePair = array2[j];
			int key = keyValuePair.Key;
			if (!fromToDico.TryGetValue(key, out var value2))
			{
				value2 = key;
			}
			Add(value2, keyValuePair.Value);
		}
	}
}
