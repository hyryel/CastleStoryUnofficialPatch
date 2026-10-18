using System.Collections.Generic;
using System.Runtime.InteropServices;
using Brix.Components;
using Brix.Engine;
using Brix.Game.Components;
using Brix.Game.Network;
using Brix.Utils;
using Brix.Utils.Collections;
using Newtonsoft.Json;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI;

public class WorkForceProvider : NetworkBrixComponent, ICharacterTracker, IObjectTracker
{
	public enum WorkForceTypes
	{
		basic,
		withLeader
	}

	[SerializeField]
	private Transform CharactersHolder;

	[SerializeField]
	private WorkForceTypes type;

	[SerializeField]
	private List<Occupation.Type> acceptedOccupations;

	private IOperable<Labor> _operable;

	private WorkForce workforce;

	[SerializeField]
	[JsonProperty]
	[SyncVar(hook = "maxPopulation_hook")]
	private int _maxPopulation = 4;

	public int MaxPopulation
	{
		get
		{
			return _maxPopulation;
		}
		set
		{
			if (value < workforce.Count)
			{
				FireSomeone();
			}
			Network_maxPopulation = value;
		}
	}

	public int Network_maxPopulation
	{
		get
		{
			return _maxPopulation;
		}
		[param: In]
		set
		{
			ref int maxPopulation = ref _maxPopulation;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				maxPopulation_hook(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref maxPopulation, 1u);
		}
	}

	private void maxPopulation_hook(int value)
	{
		Network_maxPopulation = value;
		if (_operable is Project)
		{
			(_operable as Project).OnMaxPopulationChanged(value);
		}
	}

	private void FireSomeone()
	{
		if (LaborToFire(out var labor))
		{
			labor.AssignToIdle();
			labor.Autonomy = labor.Autonomy.OnFiredFromOperable;
		}
	}

	private bool LaborToFire(out Labor labor)
	{
		labor = workforce.Slinq().FirstOrDefault((Labor l) => l.Autonomy == AutonomyStatus.FreeAgent);
		if (!labor)
		{
			labor = workforce.Slinq().FirstOrDefault();
		}
		return labor != null;
	}

	public bool Accepts(Labor labor)
	{
		return acceptedOccupations.Contains(labor.Profession.OccupationType);
	}

	public bool IsFull()
	{
		return _maxPopulation != -1 && workforce.Count >= _maxPopulation;
	}

	public bool IsOverPopulated()
	{
		return _maxPopulation != -1 && workforce.Count > _maxPopulation;
	}

	public T GetAs<T>() where T : WorkForce
	{
		return workforce as T;
	}

	public override void Initialize()
	{
		base.Initialize();
		_createWorkForce(type);
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		SetDirtyBit(1u);
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		if (!Architecte.termine)
		{
			List<Labor> list = workforce.Slinq().ToPooledList();
			list.Slinq().ForEach(delegate(Labor l)
			{
				l.Autonomy = l.Autonomy.OnFiredFromOperable;
				l.HaltWhatever();
				l.AssignToIdle();
			});
			list.Dispose();
		}
		workforce.Clear();
		CharactersHolder.GetComponentsInChildren<FlailingColliderMonitor>().Slinq().ForEach(delegate(FlailingColliderMonitor m)
		{
			m.ReparentToCharacterParent();
		});
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		workforce.Dispose();
	}

	public void CreateWorkForce()
	{
		_createWorkForce(type);
	}

	public int AssignedCount()
	{
		return (from l in workforce.Slinq()
			where l.Autonomy != AutonomyStatus.FreeAgent && l.Occupation.CurrentOccupation == Occupation.Type.Builder
			select l).Count();
	}

	public int FreeAgentCount()
	{
		return (from l in workforce.Slinq()
			where l.Autonomy == AutonomyStatus.FreeAgent && l.Occupation.CurrentOccupation == Occupation.Type.Builder
			select l).Count();
	}

	private void _createWorkForce(WorkForceTypes type)
	{
		if (workforce == null)
		{
			_operable = GetComponent<Project>();
			workforce = WorkForce.Request();
			MaxPopulation = _maxPopulation;
		}
	}

	public Transform ContainerFor(GameObject item)
	{
		return CharactersHolder;
	}

	public virtual void ObjectUntracked(GameObject character)
	{
		Labor component = character.GetComponent<Labor>();
		if (!(component == null) && workforce.Remove(component) && _operable is Project)
		{
			(_operable as Project).OnFire(component);
		}
	}

	public virtual void OnObjectTracked(GameObject character)
	{
		Labor component = character.GetComponent<Labor>();
		if (!component.IsNullOrReleased())
		{
			Order.Move.HaltWhatever.Send(component);
			if (workforce.Add(component) && _operable is Project)
			{
				(_operable as Project).OnHire(component);
			}
		}
	}

	GameObject IObjectTracker.get_gameObject()
	{
		return base.gameObject;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)_maxPopulation);
			return true;
		}
		bool flag2 = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt32((uint)_maxPopulation);
		}
		if (!flag2)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
		if (initialState)
		{
			_maxPopulation = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			maxPopulation_hook((int)reader.ReadPackedUInt32());
		}
	}
}
