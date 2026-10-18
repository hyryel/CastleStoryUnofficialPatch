using System;
using System.Collections.Generic;
using Brix.Core;
using Brix.Engine;

namespace Brix.Game.Components.Volumes;

[Serializable]
public class IndexedVolumetricProperties : SerializableDictionary<XYZ, long>
{
	public IndexedVolumetricProperties()
	{
	}

	public IndexedVolumetricProperties(IEnumerable<KeyValuePair<XYZ, long>> ivs)
	{
		foreach (KeyValuePair<XYZ, long> iv in ivs)
		{
			Add(iv.Key, iv.Value);
		}
	}

	public IndexedVolumetricProperties(IEnumerable<KeyValuePair<XYZ, VPropertyMask>> ivs)
	{
		foreach (KeyValuePair<XYZ, VPropertyMask> iv in ivs)
		{
			Add(iv.Key, (long)iv.Value);
		}
	}

	public Dictionary<XYZ, VPropertyMask> ToVolumeDictionary()
	{
		Dictionary<XYZ, VPropertyMask> dictionary = new Dictionary<XYZ, VPropertyMask>();
		using Dictionary<XYZ, long>.Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<XYZ, long> current = enumerator.Current;
			dictionary.Add(current.Key, (VPropertyMask)current.Value);
		}
		return dictionary;
	}
}
