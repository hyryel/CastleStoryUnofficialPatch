using System;
using System.Collections.Generic;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Input.ContextualOrders;

public class ContextualActionGroupData : IPoolable, IResetable, IDisposable
{
	public readonly List<GameObject> SubjectsUnderSameOrder = new List<GameObject>();

	private Formation _formation;

	public Vector3 GetFormationDestination(Vector3 destination, GameObject subject)
	{
		if (_formation == null)
		{
			_formation = ObjectPool<Formation>.Reserve().Init(destination, SubjectsUnderSameOrder);
		}
		return _formation.Spot(SubjectsUnderSameOrder.IndexOf(subject));
	}

	public ContextualActionGroupData Init(IList<GameObject> subjectsUnderSameOrder)
	{
		subjectsUnderSameOrder.Slinq().AddTo(SubjectsUnderSameOrder);
		return this;
	}

	public ContextualActionGroupData Init(IList<Component> subjectsUnderSameOrder)
	{
		(from c in subjectsUnderSameOrder.Slinq()
			select c.gameObject).AddTo(SubjectsUnderSameOrder);
		return this;
	}

	public void ResetObject()
	{
		SubjectsUnderSameOrder.Clear();
		if (_formation != null)
		{
			ObjectPool<Formation>.Release(_formation);
			_formation = null;
		}
	}

	public void Dispose()
	{
		ResetObject();
	}
}
