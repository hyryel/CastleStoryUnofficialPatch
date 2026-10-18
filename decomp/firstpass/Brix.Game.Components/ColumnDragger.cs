using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.External.Factories;
using Brix.Input;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components;

public class ColumnDragger : VerticalDragger
{
	private static List<Factory.AssetKey> _middleColumns = new List<Factory.AssetKey>
	{
		Blueprints.Stone_Column_Middle,
		Blueprints.Stone_Column2_Middle,
		Blueprints.Stone_Column3_Middle,
		Blueprints.Stone_Column4_Middle,
		Blueprints.Stone_Column5_Middle
	};

	public override IEnumerable<Slave> GenerateSlaves(VoxelRaycastHit vrh)
	{
		_sizeChanged = false;
		ColumnFitter component = GetComponent<ColumnFitter>();
		Vector3 verticalDragPoint = PickingUtility.GetVerticalDragPoint(_initialPoint.HitVector, vrh.LastRay);
		component.FitAndOrient((!(verticalDragPoint != Vector3.zero)) ? vrh : new VoxelRaycastHit().PlaceAtVector(verticalDragPoint), out var absolutePosition);
		component.FitAndOrient(_initialPoint, out var absolutePosition2);
		_dragPoints = FilterDragPoints(GetDragPoints(absolutePosition2, absolutePosition));
		ResizeSlavesTo(_dragPoints.Count);
		if (_sizeChanged)
		{
			for (int i = 0; i < _slaves.Count; i++)
			{
				utilVRH.PlaceAtVector(_dragPoints[i] + _initialPoint.LocalHitVector);
				_slaves[i].GetHeldComponent<IFitter>().CloneInternals(component);
				_slaves[i].GetHeldComponent<IFitter>().FitForDrag(utilVRH);
				_slaves[i].GetHeldComponent<ValidatorComponent>().Validate(GetValidationData());
			}
		}
		Factory.AssetKey assetKey = GetComponent<AssetKeyHolder>().AssetKey;
		if (_dragPoints.Count > 0)
		{
			if (!_middleColumns.Contains(assetKey))
			{
				if (_initialPoint.HitVector.y < _dragPoints.Slinq().Last().y)
				{
					component.SetFit(ColumnFitter.ColumnKeyState.Base);
					Slave slave = _slaves.Slinq().Last();
					slave.GetHeldComponent<ColumnFitter>().SetFit(ColumnFitter.ColumnKeyState.Top);
					foreach (Slave item in _slaves.Slinq().Where((Slave s, Slave l) => s != l, slave).NoLeak())
					{
						item.GetHeldComponent<ColumnFitter>().SetFit(ColumnFitter.ColumnKeyState.Middle);
					}
				}
				else
				{
					component.SetFit(ColumnFitter.ColumnKeyState.Top);
					Slave slave2 = _slaves.Slinq().Last();
					slave2.GetHeldComponent<ColumnFitter>().SetFit(ColumnFitter.ColumnKeyState.Base);
					foreach (Slave item2 in _slaves.Slinq().Where((Slave s, Slave l) => s != l, slave2).NoLeak())
					{
						item2.GetHeldComponent<ColumnFitter>().SetFit(ColumnFitter.ColumnKeyState.Middle);
					}
				}
			}
			else
			{
				component.SetFit(ColumnFitter.ColumnKeyState.Middle);
				foreach (Slave slafe in _slaves)
				{
					slafe.GetHeldComponent<ColumnFitter>().SetFit(ColumnFitter.ColumnKeyState.Middle);
				}
			}
		}
		return _slaves;
	}
}
