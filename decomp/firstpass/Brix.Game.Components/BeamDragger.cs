using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.External.Factories;
using Brix.Input;
using Brix.Lifecycle.Pooling;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;

namespace Brix.Game.Components;

public class BeamDragger : SingleAxisDragger
{
	private Slave _brace;

	private int _lastCount;

	private bool _useBrace;

	private Slave Brace
	{
		get
		{
			return _brace;
		}
		set
		{
			if (_brace != null)
			{
				_brace.DestroyHeldObject();
			}
			_brace = value;
		}
	}

	public override IEnumerable<Slave> GetSlaves()
	{
		foreach (Slave slafe in _slaves)
		{
			yield return slafe;
		}
		if (Brace != null)
		{
			yield return Brace;
		}
	}

	public override IEnumerable<ValidationData> GetValidationData()
	{
		yield return GetComponent<ValidatorComponent>().VData;
		foreach (Slave s in _slaves)
		{
			yield return s.GetHeldComponent<ValidatorComponent>().VData;
		}
		if (_useBrace && Brace != null)
		{
			yield return Brace.GetHeldComponent<ValidatorComponent>().VData;
		}
	}

	public override IEnumerable<Slave> GenerateSlaves(VoxelRaycastHit vrh)
	{
		_useBrace = false;
		base.GenerateSlaves(vrh);
		_useBrace = true;
		if (_sizeChanged)
		{
			Slinq<Slave, PredicateContext<Slave, IntContext<Slave, IListContext<Slave>>>> slinq = from s in _slaves.Slinq().Take(5)
				where s.GetHeldComponent<ValidatorComponent>().GetCurrentState() == ValidationResult.Invalid
				select s;
			if (_slaves.Count >= 2)
			{
				foreach (Slave item in slinq.NoLeak())
				{
					if (Brace == null)
					{
						GenerateBrace();
					}
					item.GetHeldComponent<ValidatorComponent>().Validate(GetValidationData());
				}
			}
			else
			{
				Brace = null;
			}
		}
		return GetSlaves();
	}

	private void GenerateBrace()
	{
		if (!ObjectPoolSingleton.Request(out var go, Blueprints.Wood_Brace, base.gameObject.transform.position, base.gameObject.transform.rotation))
		{
			return;
		}
		Slave slave = new Slave(go, _pickingParent);
		utilVRH.PlaceAtVector(base.gameObject.transform.position - Vector3.up);
		slave.GetHeldComponent<IFitter>().FitAndOrient(utilVRH);
		ValidatorComponent heldComponent = slave.GetHeldComponent<ValidatorComponent>();
		if (heldComponent.Validate() >= ValidationResult.Valid)
		{
			Brace = slave;
			return;
		}
		slave.HeldObject.transform.rotation *= Quaternion.Euler(0f, 180f, 0f);
		if (heldComponent.Validate() >= ValidationResult.Valid)
		{
			Brace = slave;
		}
		else
		{
			slave.DestroyHeldObject();
		}
	}
}
