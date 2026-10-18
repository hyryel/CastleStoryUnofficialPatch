using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.Components.Volumes;
using Brix.Lifecycle.Pooling;
using Brix.Utils;
using Brix.Utils.Collections;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components;

public class Highlighter : MonoBehaviour, IHighlighAdder, IHighlightRemover
{
	[Flags]
	private enum Modes
	{
		Terrain = 1,
		Blocks = 2,
		Objects = 4,
		All = 7
	}

	private int currentID;

	private static Highlighter _instance;

	[SerializeField]
	private Transform HighlightParent;

	private Dictionary<GameObject, List<GameObject>> HighlightsByGO = new Dictionary<GameObject, List<GameObject>>();

	private Dictionary<XYZ, List<GameObject>> HighlightsByPos = new Dictionary<XYZ, List<GameObject>>();

	private Dictionary<int, List<Highlight>> HighlightsByRequest = new Dictionary<int, List<Highlight>>();

	private static readonly VPropertyMask ObjectVProperties = VPropertyMask.obstacle | VPropertyMask.occupy | VPropertyMask.fantome;

	private static Highlighter instance
	{
		get
		{
			if (_instance == null)
			{
				ObjectPoolSingleton.RequestComponent<Highlighter>(out _instance, Test.Highlighter);
			}
			return _instance;
		}
	}

	public static IHighlighAdder highlight => instance;

	public static IHighlightRemover remove => instance;

	private void Awake()
	{
		_instance = this;
	}

	public int At(XYZ pos, Color color, IReadOnlySignal<GameObject> removeWhen = null)
	{
		return HighlightAt(pos, null, color, Modes.All, removeWhen);
	}

	public int Object(GameObject go, Color color, IReadOnlySignal<GameObject> removeWhen = null)
	{
		if (go.IsNullOrReleased())
		{
			return -1;
		}
		return HighlightAt(XYZ.zero, go, color, Modes.Objects, removeWhen);
	}

	public int Terrain(XYZ pos, Color color, Signal<GameObject> removeWhen)
	{
		return HighlightAt(pos, null, color, Modes.Terrain, removeWhen);
	}

	public int BlockAndTerrain(XYZ pos, Color color, Signal<GameObject> removeWhen)
	{
		return HighlightAt(pos, null, color, Modes.Terrain | Modes.Blocks, removeWhen);
	}

	private int HighlightAt(XYZ pos, GameObject thing, Color color, Modes modes, IReadOnlySignal<GameObject> removeWhen = null)
	{
		List<Highlight> buffer = CollectionPool.RequestList<Highlight>();
		if ((modes.HasFlag(Modes.Objects) && thing != null && CreateHighlightForGo(ref buffer, thing, color)) || (modes.HasFlag(Modes.Objects) && HighlightIfObject(ref buffer, pos, color)) || (modes.HasFlag(Modes.Blocks) && HighlightIfBlock(ref buffer, pos, color)) || !modes.HasFlag(Modes.Terrain) || _HightlightPosition(ref buffer, pos, color))
		{
		}
		currentID++;
		HighlightsByRequest.Add(currentID, buffer);
		if (removeWhen != null)
		{
			int closed = currentID;
			removeWhen.ConnectOnce(delegate
			{
				remove.Request(closed);
			});
		}
		return currentID;
	}

	private bool IBlockDataAt(ref List<IBlockData> buffer, XYZ pos)
	{
		buffer.Clear();
		if (Voxel.GetVoxel(pos) is MultiBlockData multiBlockData)
		{
			buffer = multiBlockData.ChildBlocks.ToPooledList();
		}
		else
		{
			if (!(Voxel.GetVoxel(pos) is IBlockData { IsNull: false } blockData))
			{
				return false;
			}
			buffer.Add(blockData);
		}
		return true;
	}

	private bool HighlightIfBlock(ref List<Highlight> buffer, XYZ pos, Color color)
	{
		CollectionPool.Request<IBlockData>(out List<IBlockData> list);
		if (!IBlockDataAt(ref list, pos))
		{
			return false;
		}
		buffer.Clear();
		foreach (IBlockData item in list)
		{
			if (item is BlockData blockData)
			{
				Highlight highlight = Highlight.FromGameObject(blockData.PlacedBlock, color, HighlightParent);
				highlight.HighlightBlock(blockData);
				buffer.Add(highlight);
			}
		}
		list.Dispose();
		return true;
	}

	private bool HighlightIfObject(ref List<Highlight> buffer, XYZ pos, Color color)
	{
		List<IVolume> list = VolumeRegistry.VolumesWithPropertyAt(pos, ObjectVProperties).Slinq().ToPooledList();
		if (list.Count == 0 || list.Slinq().All((IVolume v) => v is FixedVolume || v.GameObject.IsNullOrReleased()))
		{
			list.Dispose();
			return false;
		}
		buffer.Clear();
		foreach (IVolume item in list)
		{
			CreateHighlightForGo(ref buffer, item.GameObject, color);
		}
		list.Dispose();
		return true;
	}

	private bool CreateHighlightForGo(ref List<Highlight> buffer, GameObject go, Color color)
	{
		Highlight highlight = Highlight.FromGameObject(go, color, HighlightParent);
		HighlightsByGO.GetValueOrCreate(go, CollectionPool.RequestList<GameObject>).Add(highlight.gameObject);
		highlight.HighlightObject(go);
		buffer.Add(highlight);
		return true;
	}

	private bool _HightlightPosition(ref List<Highlight> buffer, XYZ pos, Color color, Signal removeWhen = null)
	{
		Highlight highlight = Highlight.FromPosition(pos, color, HighlightParent);
		HighlightsByPos.GetValueOrCreate(pos, CollectionPool.RequestList<GameObject>).Add(highlight.gameObject);
		highlight.HighlightPosition(pos);
		removeWhen?.ConnectOnce(delegate
		{
			_RemoveHighlight(highlight);
		});
		buffer.Clear();
		buffer.Add(highlight);
		return true;
	}

	public void Request(int i)
	{
		if (!HighlightsByRequest.TryGetValue(i, out var value))
		{
			return;
		}
		foreach (Highlight item in value)
		{
			_RemoveHighlight(item);
		}
		value.Dispose();
		HighlightsByRequest.Remove(i);
	}

	private void _RemoveHighlight(Highlight highlight)
	{
		if ((bool)highlight)
		{
			if (highlight.TargetGO != null && HighlightsByGO.ContainsKey(highlight.TargetGO))
			{
				HighlightsByGO.Remove(highlight.TargetGO);
			}
			if (HighlightsByPos.ContainsKey(highlight.TargetPosition))
			{
				HighlightsByPos[highlight.TargetPosition].Remove(highlight.gameObject);
			}
			ObjectPoolSingleton.Release(highlight.gameObject);
		}
	}

	public void At(XYZ pos)
	{
		GameObject goAtPos = GetGoAtPos(pos);
		if (!(goAtPos == null))
		{
			if (HighlightsByGO.ContainsKey(goAtPos))
			{
				HighlightsByGO[goAtPos].ForEach(ObjectPoolSingleton.Release);
				HighlightsByGO.Remove(goAtPos);
			}
			else if (HighlightsByPos.ContainsKey(pos))
			{
				HighlightsByPos[pos].ForEach(ObjectPoolSingleton.Release);
				HighlightsByPos.Remove(pos);
			}
		}
	}

	private GameObject GetGoAtPos(XYZ p)
	{
		IVolume volume = VolumeRegistry.VolumesWithPropertyAt(p, ObjectVProperties).Slinq().FirstOrDefault();
		if (volume != null)
		{
			return volume.GameObject;
		}
		if (Voxel.GetVoxel(p) is BlockData blockData)
		{
			return blockData.PlacedBlock;
		}
		return null;
	}
}
