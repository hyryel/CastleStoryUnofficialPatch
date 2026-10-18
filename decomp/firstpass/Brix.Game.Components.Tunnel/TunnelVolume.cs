using System.Collections.Generic;
using Brix.Engine;
using Brix.Engine.Volumes;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.Components.Volumes;
using Brix.Game.Network;
using Brix.Lifecycle.Pooling;
using Brix.Utils;
using Brix.Utils.Collections;
using Smooth.Collections;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components.Tunnel;

public class TunnelVolume : VoxelVolumeComponent
{
	private Dictionary<XYZ, TunnelUnit> Units;

	public Signal BecomesEmpty = new Signal();

	public Signal<XYZ> AddedVoxel = new Signal<XYZ>();

	public Signal<XYZ> RemovedVoxel = new Signal<XYZ>();

	[SerializeField]
	private TunnelType ExpansionTunnelType;

	public static GameSignals.GameSignal ExpansionTunnelTypeChanged = new GameSignals.GameSignal();

	[SerializeField]
	public Transform unitsParent;

	public Vector3 initialPosition;

	private XYZ initialParity;

	private static XYZ computationOffset = XYZ.one * 2 * 1000;

	private VoxelSet volume = new VoxelSet();

	public override IVoxelVolume Volume => volume;

	public TunnelType GetExpansionType()
	{
		return ExpansionTunnelType;
	}

	public void SetExpansionType(TunnelType type)
	{
		ExpansionTunnelType = type;
		GameSignals.Invoke(ExpansionTunnelTypeChanged);
	}

	protected void OnEnable()
	{
		ComputeInitialParity();
	}

	public void UpdateVolume()
	{
		ComputeInitialParity();
		if (Units == null)
		{
			Units = new Dictionary<XYZ, TunnelUnit>();
			Add(XYZ.FromVector3(initialPosition), Camera.main.transform.forward, TunnelType.straight, debug: false);
		}
		List<TunnelUnit> list = (from kvp in Units.Slinq()
			select kvp.Value).ToPooledList();
		UnregisterAll();
		list.Slinq().ForEach(delegate(TunnelUnit u, TunnelVolume v)
		{
			v.Register(u);
		}, this);
		list.Dispose();
		VolumeChanged.Invoke();
	}

	public Dictionary<XYZ, TunnelUnit> GetUnits()
	{
		return Units;
	}

	private void TestKeys()
	{
		Dictionary<XYZ, Color> dictionary = new Dictionary<XYZ, Color>();
		for (int i = -4; i <= 4; i++)
		{
			for (int j = 0; j <= 0; j++)
			{
				for (int k = -4; k <= 4; k++)
				{
					XYZ xYZ = XYZ.FromVector3(initialPosition) + new XYZ(i, j, k);
					XYZ key = PositionKey(xYZ);
					if (!dictionary.ContainsKey(key))
					{
						Vector3 insideUnitSphere = Random.insideUnitSphere;
						dictionary.Add(key, new Color(insideUnitSphere.x, insideUnitSphere.y, insideUnitSphere.z));
						Highlighter.highlight.At(xYZ, dictionary[key]);
					}
					Highlighter.highlight.At(xYZ - XYZ.up, dictionary[key]);
				}
			}
		}
	}

	public XYZ PositionKey(XYZ pos)
	{
		XYZ xYZ = pos + computationOffset + initialParity;
		XYZ xYZ2 = new XYZ(xYZ.x / 2 * 2, xYZ.y, xYZ.z / 2 * 2);
		return xYZ2 - computationOffset - initialParity;
	}

	private void ComputeInitialParity()
	{
		XYZ xYZ = XYZ.FromVector3(initialPosition);
		initialParity = new XYZ(xYZ.x % 2, xYZ.y % 1, xYZ.z % 2);
	}

	public void Register(TunnelUnit unit)
	{
		XYZ key = PositionKey(unit.Position);
		if (!(Units.TryGet(key).value != null))
		{
			Units.Add(key, unit);
			OccupySpace(unit.Position, wasAdded: true);
			PreventCrumbling(unit);
			IEnumerable<XYZ> enumerable = unit.GetVolume();
			volume.AddRange(enumerable);
			Slinq<XYZ, IEnumerableContext<XYZ>> slinq = enumerable.Slinq();
			while (slinq.current.isSome)
			{
				AddedVoxel.Invoke(slinq.current.value);
				slinq.Skip();
			}
			VolumeChanged.Invoke();
		}
	}

	private void UnregisterAll()
	{
		Units.Clear();
		volume.Clear();
	}

	public void UnRegister(TunnelUnit unit)
	{
		XYZ xYZ = PositionKey(unit.Position);
		Units.Remove(xYZ);
		PreventCrumblingUndo(xYZ);
		OccupySpace(xYZ, wasAdded: false);
		IEnumerable<XYZ> enumerable = unit.GetVolume();
		volume.RemoveRange(enumerable);
		Slinq<XYZ, IEnumerableContext<XYZ>> slinq = enumerable.Slinq();
		while (slinq.current.isSome)
		{
			RemovedVoxel.Invoke(slinq.current.value);
			slinq.Skip();
		}
		VolumeChanged.Invoke();
	}

	private XYZ ComputeDirection(XYZ pos1, XYZ pos2)
	{
		XYZ a = pos2 - pos1;
		return XYZ.Clamp(a, new XYZ(-1, 0, -1), new XYZ(1, 0, 1));
	}

	public void Add(XYZ pos, Vector3 direction, TunnelType tunnelType, bool debug = true, bool deserializing = false)
	{
		if (!IsOccupied(pos))
		{
			InstantiateUnit(pos, direction, tunnelType, deserializing);
		}
	}

	public void AddRange(XYZ pos1, XYZ pos2, TunnelType tunnelType)
	{
		XYZ xYZ = ComputeDirection(pos1, pos2);
		int num = XYZ.SqrMagnitude(pos1 - pos2);
		int y = pos1.y;
		for (XYZ xYZ2 = pos1; XYZ.SqrMagnitude(xYZ2 - pos1) < num; xYZ2 += xYZ)
		{
			XYZ pos3 = new XYZ(xYZ2.x, y + TypeOffset(ExpansionTunnelType), xYZ2.z);
			if (IsOccupied(pos3))
			{
				y = UnitAt(pos3).Position.y;
				continue;
			}
			if (WouldCrumble(pos3))
			{
				break;
			}
			InstantiateUnit(pos3, xYZ.ToVector3(), TunnelType.straight);
		}
	}

	private int TypeOffset(TunnelType type)
	{
		return type switch
		{
			TunnelType.stairsUp => 1, 
			TunnelType.stairsDown => -1, 
			_ => 0, 
		};
	}

	public void InstantiateUnit(XYZ pos, Vector3 forward, TunnelType tunnelType, bool deserializing = false)
	{
		if (!NetworkServer.active)
		{
			ObjectPoolSingleton.RequestComponent<TunnelUnit>(out var t, VolumesUI.TunnelUnit);
			t.tunnel = this;
			t.GetComponent<TunnelUnitUI>().SetUI(GetComponentInParent<TunnelUI>());
			t.transform.parent = unitsParent;
			t.Position = PositionKey(pos);
			t.gameObject.SetActive(value: true);
			t.InitDirections();
			if (forward == -base.transform.forward)
			{
				t.reversed = -1;
			}
			Register(t);
		}
		if (!deserializing)
		{
			User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdInstantiateTunnelUnit(GetComponentInParent<TunnelProject>().gameObject, PositionKey(pos).ToVector3(), forward, tunnelType);
		}
	}

	public void RemoveAt(XYZ pos)
	{
		if (IsOccupied(pos))
		{
			Remove(UnitAt(pos));
		}
	}

	public void Remove(TunnelUnit unit)
	{
		UnRegister(unit);
		Object.Destroy(unit.gameObject);
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdRemoveTunnelUnit(GetComponentInParent<TunnelProject>().gameObject, unit.Position.ToVector3());
		if (Units.Count == 0)
		{
			BecomesEmpty.Invoke();
		}
	}

	public TunnelUnit UnitAt(XYZ pos)
	{
		XYZ xYZ = PositionKey(pos);
		if (Units.ContainsKey(xYZ + XYZ.up * 2))
		{
			return Units[xYZ + XYZ.up * 2];
		}
		if (Units.ContainsKey(xYZ + XYZ.up * 1))
		{
			return Units[xYZ + XYZ.up * 1];
		}
		if (Units.ContainsKey(xYZ))
		{
			return Units[xYZ];
		}
		if (Units.ContainsKey(xYZ - XYZ.up))
		{
			return Units[xYZ - XYZ.up];
		}
		if (Units.ContainsKey(xYZ - XYZ.up * 2))
		{
			return Units[xYZ - XYZ.up * 2];
		}
		return null;
	}

	public bool IsOccupied(XYZ pos)
	{
		XYZ xYZ = PositionKey(pos);
		if (Units.ContainsKey(xYZ) || Units.ContainsKey(xYZ + XYZ.up) || Units.ContainsKey(xYZ - XYZ.up) || Units.ContainsKey(xYZ + XYZ.up * 2) || Units.ContainsKey(xYZ - XYZ.up * 2))
		{
			return true;
		}
		return false;
	}

	public Vector3 OrientationOf(XYZ pos)
	{
		return UnitAt(pos).transform.forward;
	}

	public bool WouldCrumble(XYZ pos)
	{
		bool flag = IsOccupied(pos + XYZ.forward * 2 - XYZ.right * 2);
		bool flag2 = IsOccupied(pos + XYZ.forward * 2);
		bool flag3 = IsOccupied(pos + XYZ.forward * 2 + XYZ.right * 2);
		bool flag4 = IsOccupied(pos - XYZ.right * 2);
		bool flag5 = IsOccupied(pos + XYZ.right * 2);
		bool flag6 = IsOccupied(pos - XYZ.forward * 2 - XYZ.right * 2);
		bool flag7 = IsOccupied(pos - XYZ.forward * 2);
		bool flag8 = IsOccupied(pos - XYZ.forward * 2 + XYZ.right * 2);
		return (flag4 && flag && flag2) || (flag2 && flag3 && flag5) || (flag5 && flag8 && flag7) || (flag7 && flag6 && flag4);
	}

	public bool CanBuildAt(XYZ pos)
	{
		bool flag = WouldCrumble(pos);
		bool flag2 = IsOccupied(pos);
		return !flag && !flag2;
	}

	private void ResetCrumbling(XYZ pos)
	{
		foreach (XYZ item in DirectNeighbors(pos))
		{
			foreach (TunnelDirection item2 in IncomingLinks(item))
			{
				item2.WouldCrumble = false;
			}
			if (IsOccupied(item))
			{
				TunnelDirection[] directions = UnitAt(item).Directions;
				foreach (TunnelDirection tunnelDirection in directions)
				{
					tunnelDirection.WouldCrumble = false;
				}
			}
		}
	}

	private void PreventCrumblingUndo(XYZ pos)
	{
		foreach (TunnelDirection item in AllNeighborsLinks(pos))
		{
			item.WouldCrumble = WouldCrumble(item.LeadsTo);
		}
	}

	private void PreventCrumbling(TunnelUnit unit)
	{
		TunnelDirection[] directions = unit.Directions;
		foreach (TunnelDirection tunnelDirection in directions)
		{
			if (WouldCrumble(tunnelDirection.LeadsTo))
			{
				tunnelDirection.WouldCrumble = true;
				SpreadCrumble(tunnelDirection.LeadsTo);
			}
		}
	}

	private void SpreadCrumble(XYZ pos)
	{
		foreach (TunnelDirection item in IncomingLinks(pos))
		{
			item.WouldCrumble = WouldCrumble(item.LeadsTo);
		}
		if (IsOccupied(pos))
		{
			TunnelDirection[] directions = UnitAt(pos).Directions;
			foreach (TunnelDirection tunnelDirection in directions)
			{
				tunnelDirection.WouldCrumble = WouldCrumble(tunnelDirection.LeadsTo);
			}
		}
	}

	private void OccupySpace(XYZ pos, bool wasAdded)
	{
		foreach (TunnelDirection item in IncomingLinks(pos))
		{
			item.IsOccupied = wasAdded;
		}
		if (wasAdded)
		{
			TunnelDirection[] directions = UnitAt(pos).Directions;
			foreach (TunnelDirection tunnelDirection in directions)
			{
				tunnelDirection.IsOccupied = IsOccupied(tunnelDirection.LeadsTo);
			}
		}
	}

	public IEnumerable<TunnelDirection> AllNeighborsLinks(XYZ pos)
	{
		foreach (TunnelUnit unit in OccupiedNeighbors(pos))
		{
			TunnelDirection[] directions = unit.Directions;
			for (int i = 0; i < directions.Length; i++)
			{
				yield return directions[i];
			}
		}
	}

	public IEnumerable<TunnelUnit> OccupiedNeighbors(XYZ pos)
	{
		foreach (XYZ pos2 in AllNeighbors(pos))
		{
			if (IsOccupied(pos2))
			{
				yield return UnitAt(pos2);
			}
		}
	}

	public IEnumerable<XYZ> AllNeighbors(XYZ pos)
	{
		foreach (XYZ item in XNeighbors(pos))
		{
			yield return item;
		}
		foreach (XYZ item2 in DirectNeighbors(pos))
		{
			yield return item2;
		}
	}

	public IEnumerable<XYZ> XNeighbors(XYZ pos)
	{
		yield return pos + XYZ.forward * 2 - XYZ.right * 2;
		yield return pos + XYZ.forward * 2 + XYZ.right * 2;
		yield return pos - XYZ.forward * 2 - XYZ.right * 2;
		yield return pos - XYZ.forward * 2 + XYZ.forward * 2;
	}

	public IEnumerable<XYZ> DirectNeighbors(XYZ pos)
	{
		yield return pos - XYZ.right * 2;
		yield return pos + XYZ.forward * 2;
		yield return pos + XYZ.right * 2;
		yield return pos - XYZ.forward * 2;
	}

	public IEnumerable<TunnelDirection> OutGoingLinks(XYZ pos)
	{
		if (IsOccupied(pos))
		{
			return UnitAt(pos).Directions;
		}
		return Empty.List<TunnelDirection>();
	}

	public IEnumerable<TunnelDirection> IncomingLinks(XYZ pos)
	{
		XYZ Left = pos - XYZ.right * 2;
		if (IsOccupied(Left))
		{
			yield return UnitAt(Left).GetLocalDirection(Vector3.right);
		}
		XYZ Forward = pos + XYZ.forward * 2;
		if (IsOccupied(Forward))
		{
			yield return UnitAt(Forward).GetLocalDirection(Vector3.back);
		}
		XYZ Right = pos + XYZ.right * 2;
		if (IsOccupied(Right))
		{
			yield return UnitAt(Right).GetLocalDirection(Vector3.left);
		}
		XYZ Back = pos - XYZ.forward * 2;
		if (IsOccupied(Back))
		{
			yield return UnitAt(Back).GetLocalDirection(Vector3.forward);
		}
	}

	public void ResetPositionAndParity(XYZ pos)
	{
		TunnelUnit tunnelUnit = (from kvp in Units.Slinq()
			select kvp.Value).FirstOrDefault();
		if ((bool)tunnelUnit)
		{
			ComputeInitialParity();
			UnregisterAll();
			tunnelUnit.Position = PositionKey(pos);
			Register(tunnelUnit);
		}
	}
}
