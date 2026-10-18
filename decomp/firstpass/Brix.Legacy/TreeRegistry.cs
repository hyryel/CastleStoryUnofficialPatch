using System.Collections.Generic;
using System.Linq;
using Brix.Assets;
using Brix.Common.Collections;
using Brix.Components;
using Brix.Engine;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Collections;
using Brix.Utils.Logging;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;

namespace Brix.Legacy;

public class TreeRegistry : BrixComponent
{
	private static BrixLogger logger = new BrixLogger(typeof(TreeRegistry).FullName);

	private static TreeRegistry _instance;

	private readonly Dictionary<XYZ, int> _treeData = new Dictionary<XYZ, int>();

	public static readonly XYZ[] TreeNeighborhood = Calcule.CubeConcentrique(XYZ.zero, 1).ToArray();

	public static TreeRegistry Instance => _instance;

	protected override void Awake()
	{
		base.Awake();
		_instance = this;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_instance == this)
		{
			_instance = null;
		}
	}

	public IEnumerable<XYZ> ListAllTreePosition()
	{
		return (from kvp in _treeData.Slinq()
			select kvp.Key).NoLeak();
	}

	public IEnumerable<Asset_Map.TreeInfo> ListAllTreePositionAndSize()
	{
		return (from kvp in _treeData.Slinq()
			select new Asset_Map.TreeInfo
			{
				x = kvp.Key.x,
				y = kvp.Key.y,
				z = kvp.Key.z,
				s = ((kvp.Value >> 4) & 0xF)
			}).NoLeak();
	}

	public bool Add(XYZ position)
	{
		return Add(position, GetPseudoRandomArbreInfo(position.ToScalaire()));
	}

	public bool Add(XYZ position, int treeType)
	{
		if (treeType == 0)
		{
			logger.WarnFormat("TreeRegistry.Add: null tree type at {0}", position);
			return false;
		}
		if (VoxelEngine.IsEmptyColumn(position.x, position.z))
		{
			logger.WarnFormat("TreeRegistry.Add: empty column at {0}", position);
			return false;
		}
		if (_treeData.ContainsKey(position))
		{
			return false;
		}
		_treeData.Add(position, treeType);
		GameSignals.Invoke(GameSignals.TreeChanged, position);
		return true;
	}

	public bool HasTree(XYZ position)
	{
		return _treeData.ContainsKey(position);
	}

	public int GetTree(XYZ position)
	{
		return _treeData.GetValueOrDefault(position, 0);
	}

	public void ModifyTree(XYZ position, int value)
	{
		_treeData[position] = value;
		GameSignals.Invoke(GameSignals.TreeChanged, position);
	}

	public Slinq<XYZ, PredicateContext<XYZ, SelectContext<XYZ, KeyValuePair<XYZ, int>, DictionaryContext<XYZ, int>>, int>> GetTreesInRadius(XYZ center, int radius)
	{
		return (from kv in _treeData.Slinq()
			select kv.Key).Where((XYZ p, int r) => IsInRadius(p, center, r), radius);
	}

	public static bool IsInRadius(XYZ p, XYZ center, int r)
	{
		return XYZ.SqrMagnitude(p - center) <= r * r;
	}

	public bool Remove(XYZ position)
	{
		if (!_treeData.Remove(position))
		{
			return false;
		}
		GameSignals.Invoke(GameSignals.TreeChanged, position);
		return true;
	}

	public bool IsNearATree(XYZ position)
	{
		for (int i = 0; i < TreeNeighborhood.Length; i++)
		{
			if (HasTree(position + TreeNeighborhood[i]))
			{
				return true;
			}
		}
		return false;
	}

	public XYZ GetNeighborTree(XYZ position)
	{
		for (int i = 0; i < TreeNeighborhood.Length; i++)
		{
			if (GetTree(position + TreeNeighborhood[i]) != 0)
			{
				return position + TreeNeighborhood[i];
			}
		}
		return XYZ.zero;
	}

	public void TranslateAllTrees(XYZ delta)
	{
		List<XYZ> list = ListAllTreePosition().Slinq().ToPooledList();
		List<XYZ> list2 = (from p in list.Slinq()
			select p + delta).ToPooledList();
		List<int> list3 = (from p in list.Slinq()
			select (Instance.GetTree(p) >> 4) & 0xF).ToPooledList();
		foreach (XYZ item in list)
		{
			Remove(item);
		}
		int num = 0;
		foreach (XYZ item2 in list2)
		{
			int pseudoRandomArbreInfo = GetPseudoRandomArbreInfo(item2.ToScalaire());
			pseudoRandomArbreInfo = (pseudoRandomArbreInfo & 0xF) | (list3[num++] << 4);
			Add(item2, pseudoRandomArbreInfo);
		}
		list.Dispose();
		list2.Dispose();
		list3.Dispose();
	}

	public static int GetPseudoRandomArbreInfo(int index)
	{
		Random.State state = Random.state;
		Random.InitState(index);
		int num = Mathf.FloorToInt(Random.value * 3f) | (Mathf.Clamp(Mathf.FloorToInt(Random.value * 4f), 0, 3) << 2);
		Random.state = state;
		if (num == 0)
		{
			num = 1;
		}
		return num;
	}
}
