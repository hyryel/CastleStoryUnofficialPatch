using System.Collections.Generic;
using Brix.Engine;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Collections;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components.Volumes;

public static class VolumeDataGenerator
{
	public static List<XYZ> GetValueOrCreate(this List<XYZ>[] lists, int index)
	{
		return lists[index] ?? (lists[index] = CollectionPool.RequestList<XYZ>());
	}

	public static Dictionary<XYZ, VPropertyMask> ToIndexedVolumetricSettings(this VolumeDataComponent component, Quaternion discreteRotation)
	{
		Dictionary<XYZ, VPropertyMask> dictionary = component.IndexedVolumetricProperties.ToIndexedVolumetricSettings(discreteRotation);
		RemoveDeadEntries(dictionary);
		return dictionary;
	}

	public static Dictionary<XYZ, VPropertyMask> ToIndexedVolumetricSettings(this IEnumerable<KeyValuePair<XYZ, VPropertyMask>> indexedVSettings)
	{
		return new IndexedVolumetricProperties(indexedVSettings).ToVolumeDictionary();
	}

	public static Dictionary<XYZ, VPropertyMask> ToIndexedVolumetricSettings(this IEnumerable<KeyValuePair<XYZ, VPropertyMask>> indexedVSettings, Quaternion rotation)
	{
		return new IndexedVolumetricProperties(RotateVSettings(indexedVSettings, rotation)).ToVolumeDictionary();
	}

	public static Dictionary<XYZ, VPropertyMask> ToIndexedVolumetricSettings(this IEnumerable<KeyValuePair<XYZ, long>> indexedVSettings)
	{
		return new IndexedVolumetricProperties(indexedVSettings).ToVolumeDictionary();
	}

	public static Dictionary<XYZ, VPropertyMask> ToIndexedVolumetricSettings(this IEnumerable<KeyValuePair<XYZ, long>> indexedVSettings, Quaternion rotation)
	{
		return new IndexedVolumetricProperties(RotateVSettings(indexedVSettings, rotation)).ToVolumeDictionary();
	}

	public static List<XYZ>[] ToVolumetricSettingsIndices(this Dictionary<XYZ, VPropertyMask> indexedVSettings)
	{
		List<XYZ>[] array = new List<XYZ>[64];
		foreach (KeyValuePair<XYZ, VPropertyMask> indexedVSetting in indexedVSettings)
		{
			foreach (int item in indexedVSetting.Value.Bits())
			{
				array.GetValueOrCreate(item).Add(indexedVSetting.Key);
			}
		}
		array.Slinq().NotNull().ForEach(delegate(List<XYZ> list)
		{
			list.Sort();
		});
		return array;
	}

	public static List<XYZ>[] ToVolumetricSettingsIndices(this Dictionary<XYZ, long> indexedVSettings)
	{
		List<XYZ>[] array = new List<XYZ>[64];
		foreach (KeyValuePair<XYZ, long> indexedVSetting in indexedVSettings)
		{
			foreach (int item in indexedVSetting.Value.Bits())
			{
				array.GetValueOrCreate(item).Add(indexedVSetting.Key);
			}
		}
		array.Slinq().NotNull().ForEach(delegate(List<XYZ> list)
		{
			list.Sort();
		});
		return array;
	}

	public static List<XYZ>[] ToVolumetricSettingsIndices(this List<XYZ>[] source)
	{
		List<XYZ>[] array = new List<XYZ>[64];
		for (int i = 0; i < source.Length; i++)
		{
			if (source[i] != null)
			{
				source[i].Slinq().AddTo(array.GetValueOrCreate(i));
			}
		}
		return array;
	}

	public static void RemoveProperties(this Dictionary<XYZ, VPropertyMask> ivs, VPropertyMask properties)
	{
		foreach (XYZ item in (from kvp in ivs.Slinq()
			select kvp.Key).ToList())
		{
			ivs[item] &= ~properties;
		}
		RemoveDeadEntries(ivs);
	}

	public static void RemoveProperties(this List<XYZ>[] vsi, VPropertyMask p)
	{
		foreach (int item in p.Bits())
		{
			List<XYZ> list = vsi[item];
			if (list != null)
			{
				list.Dispose();
				vsi[item] = null;
			}
		}
	}

	public static void RemoveProperties(this Dictionary<XYZ, VPropertyMask> ivs, VProperty p)
	{
		ivs.RemoveProperties(p.ToMask());
	}

	public static void RemoveProperties(this List<XYZ>[] vsi, VProperty p)
	{
		vsi.RemoveProperties(p.ToMask());
	}

	public static void KeepProperties(this Dictionary<XYZ, VPropertyMask> ivs, VPropertyMask p)
	{
		foreach (XYZ item in (from kvp in ivs.Slinq()
			select kvp.Key).ToList())
		{
			ivs[item] &= p;
		}
		RemoveDeadEntries(ivs);
	}

	public static void KeepProperties(this List<XYZ>[] vsi, VPropertyMask p)
	{
		for (int i = 0; i < vsi.Length; i++)
		{
			if (!p.HasFlag((VPropertyMask)(1 << i)))
			{
				if (vsi[i] != null)
				{
					vsi[i].Dispose();
				}
				vsi[i] = null;
			}
		}
	}

	public static void KeepProperties(this Dictionary<XYZ, VPropertyMask> ivs, VProperty p)
	{
		ivs.KeepProperties(p.ToMask());
	}

	public static void KeepProperties(this List<XYZ>[] vsi, VProperty p)
	{
		vsi.KeepProperties(p.ToMask());
	}

	public static void CopyIndices(this Dictionary<XYZ, VPropertyMask> ivs, VProperty source, VProperty to)
	{
		foreach (XYZ item in (from kvp in ivs.Slinq()
			where kvp.Value.HasFlag(source.ToMask())
			select kvp.Key).ToList())
		{
			ivs[item] |= to.ToMask();
		}
	}

	public static void CopyIndices(this List<XYZ>[] vsi, VProperty source, VProperty to)
	{
		List<XYZ> list = vsi[(int)source];
		if (list != null)
		{
			List<XYZ> valueOrCreate = vsi.GetValueOrCreate((int)to);
			list.Slinq().Where((XYZ i, List<XYZ> _listOut) => !_listOut.Contains(i), valueOrCreate).AddTo(valueOrCreate);
		}
	}

	private static void RemoveDeadEntries(Dictionary<XYZ, VPropertyMask> ivs)
	{
		List<XYZ> list = (from kv in ivs.Slinq()
			where kv.Value == ~VPropertyMask.All
			select kv.Key).ToPooledList();
		list.Slinq().ForEach(delegate(XYZ xyz, Dictionary<XYZ, VPropertyMask> _ivs)
		{
			_ivs.Remove(xyz);
		}, ivs);
		list.Dispose();
	}

	private static void RemoveDeadEntries(List<XYZ>[] vPropertiesIndices)
	{
		for (int i = 0; i < vPropertiesIndices.Length; i++)
		{
			if (vPropertiesIndices[i] != null && vPropertiesIndices[i].Count == 0)
			{
				vPropertiesIndices[i].Dispose();
				vPropertiesIndices[i] = null;
			}
		}
	}

	private static IEnumerable<KeyValuePair<XYZ, VPropertyMask>> RotateVSettings(IEnumerable<KeyValuePair<XYZ, VPropertyMask>> baseVSettings, Quaternion rotation)
	{
		foreach (KeyValuePair<XYZ, VPropertyMask> kv in baseVSettings)
		{
			yield return new KeyValuePair<XYZ, VPropertyMask>(RotateIndex(kv.Key, rotation), kv.Value);
		}
	}

	private static IEnumerable<KeyValuePair<XYZ, VPropertyMask>> RotateVSettings(IEnumerable<KeyValuePair<XYZ, long>> baseVSettings, Quaternion rotation)
	{
		foreach (KeyValuePair<XYZ, long> kv in baseVSettings)
		{
			yield return new KeyValuePair<XYZ, VPropertyMask>(RotateIndex(kv.Key, rotation), (VPropertyMask)kv.Value);
		}
	}

	private static XYZ RotateIndex(XYZ index, Quaternion rotation)
	{
		Vector3 vecteur = rotation * index.ToVector3();
		vecteur = Calcule.RoundVecteur(vecteur);
		return XYZ.FromVector3(vecteur);
	}

	private static void _ExceptWith<T>(this HashSet<T> source, HashSet<T> exclusion)
	{
		foreach (T item in exclusion)
		{
			source.Remove(item);
		}
	}

	private static void _UnionWith<T>(this HashSet<T> source, HashSet<T> newItems)
	{
		foreach (T newItem in newItems)
		{
			source.Add(newItem);
		}
	}
}
