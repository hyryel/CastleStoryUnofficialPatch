using System.Collections.Generic;
using Brix.Audio;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.Components.Volumes;
using Brix.Input;
using Brix.Lifecycle.Pooling;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components;

public class DefaultDragger : MonoBehaviour, IDragger
{
	public bool autoOrient;

	protected VoxelRaycastHit _initialPoint = new VoxelRaycastHit();

	protected PickerComponent _pickingParent;

	protected List<Vector3> _dragPoints = new List<Vector3>();

	protected List<Vector3> _filteredPoints = new List<Vector3>();

	protected Bounds _modelBounds = default(Bounds);

	protected List<Slave> _slaves = new List<Slave>();

	protected VoxelRaycastHit utilVRH = new VoxelRaycastHit();

	protected Vector3 orient = Vector3.zero;

	protected Vector3 reverseOrient = Vector3.zero;

	protected static Vector3 safetyOffset = new Vector3(0f, 0.05f, 0f);

	protected bool _sizeChanged;

	public bool AutoOrient => autoOrient;

	public virtual void SetDragStart(VoxelRaycastHit initialPoint, PickerComponent parentPC)
	{
		_initialPoint.Copy(initialPoint);
		_pickingParent = parentPC;
	}

	public virtual IEnumerable<Slave> GetSlaves()
	{
		return _slaves;
	}

	public virtual IEnumerable<ValidationData> GetValidationData()
	{
		yield return GetComponent<ValidatorComponent>().VData;
		foreach (Slave s in _slaves)
		{
			yield return s.GetHeldComponent<ValidatorComponent>().VData;
		}
	}

	public virtual IEnumerable<Slave> GenerateSlaves(VoxelRaycastHit vrh)
	{
		_sizeChanged = false;
		IFitter component = GetComponent<IFitter>();
		Vector3 horizontalDragPoint = PickingUtility.GetHorizontalDragPoint(_initialPoint.WorldHitVector + safetyOffset, vrh.LastRay);
		component.FitAndOrient((!(horizontalDragPoint != Vector3.zero)) ? vrh : new VoxelRaycastHit().PlaceAtVector(horizontalDragPoint), out var absolutePosition);
		component.FitAndOrient(_initialPoint, out var absolutePosition2);
		_dragPoints = GetDragPoints(absolutePosition2, absolutePosition);
		if (AutoOrient && _dragPoints.Count > 1)
		{
			for (int i = 0; i < 4; i++)
			{
				Vector3 forward = base.transform.forward;
				if (forward == orient || (AutoOrient && forward == reverseOrient))
				{
					break;
				}
				component.ChangeFit();
				component.FitAndOrient(_initialPoint, out absolutePosition2);
			}
		}
		_filteredPoints = FilterDragPoints(_dragPoints);
		ResizeSlavesTo(_filteredPoints.Count);
		if (_sizeChanged)
		{
			for (int j = 0; j < _slaves.Count; j++)
			{
				IFitter heldComponent = _slaves[j].GetHeldComponent<IFitter>();
				utilVRH.PlaceAtVector(_filteredPoints[j] + _initialPoint.LocalHitVector);
				heldComponent.CloneInternals(component);
				heldComponent.FitForDrag(utilVRH);
			}
			for (int k = 0; k < _slaves.Count; k++)
			{
				_slaves[k].GetHeldComponent<ValidatorComponent>().Validate(GetValidationData());
			}
			GetComponent<ValidatorComponent>().Validate(GetValidationData());
		}
		return _slaves;
	}

	public void DebugSlaveValidation()
	{
		List<ValidationResult> list = new List<ValidationResult>();
		List<ValidationResult> list2 = new List<ValidationResult>();
		for (int i = 0; i < _slaves.Count; i++)
		{
			list.Add(_slaves[i].GetHeldComponent<ValidatorComponent>().ValidateCache);
			list2.Add(_slaves[i].GetHeldComponent<ValidatorComponent>().Validate(GetValidationData()));
		}
	}

	public void ClearSlaves()
	{
		ResizeSlavesTo(0);
	}

	protected virtual List<Vector3> GetDragPoints(Vector3 pointA, Vector3 pointB)
	{
		float num = 1f;
		List<Vector3> list = new List<Vector3>();
		orient = Vector3.zero;
		reverseOrient = Vector3.zero;
		pointB.y = pointA.y;
		float num2;
		float num3;
		if (Mathf.Abs(num2 = pointB.x - pointA.x) < Mathf.Abs(num3 = pointB.z - pointA.z))
		{
			if (num3 < 0f)
			{
				num = -1f;
			}
			pointB.x = pointA.x;
			for (float num4 = 0f; Mathf.Abs(num3) - Mathf.Abs(num4) >= 0f; num4 += num)
			{
				list.Add(new Vector3(pointA.x, pointA.y, pointA.z + num4));
			}
			orient.z += num;
			reverseOrient.z -= num;
		}
		else
		{
			if (num2 < 0f)
			{
				num = -1f;
			}
			pointB.z = pointA.z;
			for (float num5 = 0f; Mathf.Abs(num2) - Mathf.Abs(num5) >= 0f; num5 += num)
			{
				list.Add(new Vector3(pointA.x + num5, pointA.y, pointA.z));
			}
			orient.x += num;
			reverseOrient.x -= num;
		}
		return list;
	}

	protected List<Vector3> FilterDragPoints(List<Vector3> drags)
	{
		RemakeBounds();
		List<Vector3> list = new List<Vector3>(drags);
		for (int i = 0; i != drags.Count; i++)
		{
			if (!list.Contains(drags[i]))
			{
				continue;
			}
			_modelBounds.center = drags[i];
			for (int j = 0; j < drags.Count; j++)
			{
				if (j != i && _modelBounds.Contains(drags[j]))
				{
					list.Remove(drags[j]);
				}
			}
		}
		IEnumerable<XYZ> enumerable = base.gameObject.GetComponent<IVolume>().GetCases(VProperty.fantome);
		foreach (XYZ item2 in enumerable)
		{
			Vector3 item = list.Slinq().Where((Vector3 x, XYZ _i) => XYZ.FromVector3(x) == _i, item2).FirstOrDefault();
			if (list.Contains(item))
			{
				list.Remove(item);
			}
		}
		return list;
	}

	protected void RemakeBounds()
	{
		Vector3 position = base.gameObject.transform.position;
		if (position != Vector3.zero)
		{
			base.gameObject.transform.position = new Vector3(100f, 100f, 100f);
		}
		_modelBounds = new VoxelBounds(base.gameObject.GetComponent<IVolume>().GetCases(VProperty.fantome)).ToVectorBounds();
		base.gameObject.transform.position = position;
	}

	protected void ResizeSlavesTo(int amount)
	{
		if (amount < 0)
		{
			amount = 0;
		}
		if (amount > 50)
		{
			amount = 50;
		}
		int num = amount - _slaves.Count;
		if (num > 0)
		{
			Factory.AssetKey assetKey = this.GetInterface<AssetKeyHolder>().AssetKey;
			for (int i = 0; i != num; i++)
			{
				if (!ObjectPoolSingleton.Request(out var go, assetKey, base.gameObject.transform.position, base.gameObject.transform.rotation))
				{
					break;
				}
				HoldNewObject(go);
			}
			_sizeChanged = true;
			SoundEngine.Play("Brick_Multi_Add");
		}
		else if (num < 0)
		{
			for (int num2 = 0; num2 != num; num2--)
			{
				_slaves[_slaves.Count - 1].DestroyHeldObject();
				_slaves.RemoveAt(_slaves.Count - 1);
			}
			_sizeChanged = true;
			SoundEngine.Play("Brick_Multi_Remove");
		}
	}

	protected void HoldNewObject(GameObject go)
	{
		if (go != null)
		{
			_slaves.Add(new Slave(go, _pickingParent));
			go.GetComponent<IFitter>().CloneInternals(base.gameObject.GetComponent<IFitter>());
		}
	}
}
