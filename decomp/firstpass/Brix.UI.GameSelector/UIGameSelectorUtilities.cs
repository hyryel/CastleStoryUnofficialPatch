using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.Components.Volumes;
using Brix.Utils;
using Brix.Utils.Collections;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.UI.GameSelector;

public static class UIGameSelectorUtilities
{
	private static Dictionary<XYZ, HashSet<IVolume>> VolumeMap = new Dictionary<XYZ, HashSet<IVolume>>();

	private static HashSet<XYZ> selectedCases = new HashSet<XYZ>();

	private static List<GameObject> toSelect = new List<GameObject>();

	private static int previousCount;

	private static void SelectAdjacentVolumes(SelectionType selectionType, VProperty property, int ntime)
	{
		selectedCases.Clear();
		toSelect.Clear();
		CollectionPool.Request<GameObject>(out List<GameObject> list);
		UIGameSelector.GetSelectedOfType(ref list, selectionType);
		foreach (GameObject item in list)
		{
			item.GetComponent<IVolume>().GetCases(property).Slinq()
				.AddTo(selectedCases);
		}
		list.Dispose();
		previousCount = 0;
		do
		{
			if (ntime > 0)
			{
				ntime--;
			}
			previousCount = toSelect.Count;
			UpdateCasesToSelect();
		}
		while (toSelect.Count > previousCount && ntime != 0);
		UIGameSelector.AddToSelection(toSelect);
	}

	private static void UpdateCasesToSelect()
	{
		List<XYZ> list = (from pos in VolumeMap.Keys.Slinq()
			where selectedCases.Contains(pos + XYZ.up) || selectedCases.Contains(pos - XYZ.up) || selectedCases.Contains(pos - XYZ.right) || selectedCases.Contains(pos + XYZ.forward) || selectedCases.Contains(pos - XYZ.forward) || selectedCases.Contains(pos + XYZ.right)
			select pos).ToPooledList();
		selectedCases.Clear();
		foreach (XYZ item in list)
		{
			HashSet<IVolume> hashSet = VolumeMap[item];
			toSelect.AddRange((from v in hashSet.Slinq()
				select v.GameObject).ToPooledHashSet());
			selectedCases.Add(item);
			VolumeMap.Remove(item);
		}
		list.Dispose();
	}

	public static void SelectVolumesNextTo(SelectionType selectionType, VProperty property)
	{
		UpdateVolumeCache(property);
		SelectAdjacentVolumes(selectionType, property, 1);
	}

	private static void UpdateVolumeCache(VProperty property)
	{
		VolumeMap.Clear();
		VolumeRegistry.GetAllRegisteredObjects().Slinq().Where((IVolume v, VProperty p) => v.HasProperty(p), property)
			.ForEach(delegate(IVolume v, VProperty p)
			{
				MapVolumeProperty(v, p);
			}, property);
	}

	private static void MapVolumeProperty(IVolume v, VProperty p)
	{
		using VolumePropertyRequest.Enumerator enumerator = v.GetCases(p).GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (!VolumeMap.ContainsKey(enumerator.Current))
			{
				VolumeMap.Add(enumerator.Current, new HashSet<IVolume> { v });
			}
			else
			{
				VolumeMap[enumerator.Current].Add(v);
			}
		}
	}

	public static void SelectAllConnectedVolumes(SelectionType selectionType, VProperty property)
	{
		UpdateVolumeCache(property);
		SelectAdjacentVolumes(selectionType, property, -1);
	}
}
