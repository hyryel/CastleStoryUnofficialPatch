using System;
using Brix.Engine;
using Brix.Game;
using Brix.UI.GameSelector;
using Brix.Util;
using UnityEngine;

namespace Brix.Input;

[Serializable]
public class VoxelRaycastHit
{
	private XYZ _position;

	private byte _currentFace;

	private bool _hit;

	private Vector3 _currentVoxel;

	private Vector3 _lastVoxel;

	private Vector3 _currentHitVector;

	private Vector3 _currentNormal;

	private RaycastHit _target;

	private XRay _xRay;

	protected float _distance;

	protected Ray _ray;

	protected Ray _originalRay;

	private GameObject _fakeHitGameObject;

	private bool _placedWithRaycast;

	private bool _onDefaultPlane;

	private XRay xRay
	{
		get
		{
			if (_xRay == null)
			{
				_xRay = Picking.Instance.xRay;
			}
			return _xRay;
		}
	}

	public float LastDistance => _distance;

	public Ray LastRay => _ray;

	public bool HasHit => _hit;

	public UnityLayer HitLayer
	{
		get
		{
			if (_hit && _target.transform != null)
			{
				return (UnityLayer)_target.transform.gameObject.layer;
			}
			return UnityLayer.Default;
		}
	}

	public RaycastHit RayHit => _target;

	public GameObject HitGameObject
	{
		get
		{
			if (_hit && _target.transform != null)
			{
				return _target.transform.gameObject;
			}
			if (_hit && _fakeHitGameObject != null)
			{
				return _fakeHitGameObject;
			}
			return null;
		}
	}

	public GameObject HitRelevantGameObject
	{
		get
		{
			GameObject hitGameObject = HitGameObject;
			if (hitGameObject == null)
			{
				return null;
			}
			UISelectable uISelectable = PickingUtility.FindComponentInParents<UISelectable>(hitGameObject);
			return (!(uISelectable != null)) ? hitGameObject : uISelectable.gameObject;
		}
	}

	public Vector3 HitVector => _currentVoxel;

	public Vector3 WorldHitVector => _currentHitVector;

	public Vector3 LocalHitVector => _currentHitVector - _currentVoxel;

	public Vector3 Normal => _currentNormal;

	public XYZ VoxelPosition => _position;

	public XYZ VoxelNormal => _position + XYZ.FromVector3(_currentNormal);

	public float DistanceToHit => Vector3.Distance(_currentHitVector, _originalRay.origin);

	public IVoxel HitIVoxel => Voxel.GetVoxel(_position);

	public BlockInfo HitBlockInfo => Voxel.GetBlockInfo(_position);

	public bool OnDefaultPlane => _onDefaultPlane;

	public GameObject FakeHitGameObject
	{
		get
		{
			return _fakeHitGameObject;
		}
		set
		{
			_fakeHitGameObject = value;
		}
	}

	public bool IsRaycastResult => _placedWithRaycast;

	public bool HasHitTerrain()
	{
		bool result = false;
		if (HitGameObject != null)
		{
			result = HitGameObject.layer == 10 || HitGameObject.layer == 17;
		}
		return result;
	}

	public virtual bool PlaceAtRaycast(Ray ray, float distance, LayerBundle layers, bool useDefaultPlane = false, bool isRetry = false)
	{
		if (useDefaultPlane)
		{
			_hit = new Plane(Vector3.up, Picking.Instance.WorldEditorGrid.transform.position).Raycast(ray, out var enter);
			_lastVoxel = _currentVoxel;
			_currentHitVector = ray.GetPoint(enter);
			_currentNormal = Vector3.up;
		}
		else
		{
			_hit = Physics.Raycast(ray, out _target, distance, layers, QueryTriggerInteraction.Collide);
			_lastVoxel = _currentVoxel;
			_currentHitVector = _target.point - _target.normal * 0.0001f;
			_currentNormal = Calcule.RoundVecteur(_target.normal);
		}
		_currentVoxel = Calcule.RoundVecteur(_currentHitVector);
		_position = XYZ.FromVector3(_currentVoxel);
		_ray = ray;
		_distance = distance;
		_placedWithRaycast = true;
		_onDefaultPlane = useDefaultPlane;
		if (!isRetry)
		{
			_originalRay = ray;
		}
		_currentFace = (byte)Calcule.SetBit(0, Calcule.VecteurToFaceIndex(_currentNormal), b: true);
		if (_currentNormal.y != 0f)
		{
			_currentNormal = Vector3.up;
		}
		if (_hit && HitLayer != UnityLayer.UnitsPicking && HitLayer != UnityLayer.Beacons && xRay.IsActive && xRay.Bounds.Contains(_currentHitVector))
		{
			Ray ray2 = new Ray(_currentHitVector + ray.direction, ray.direction);
			PlaceAtRaycast(ray2, distance, layers, useDefaultPlane, isRetry: true);
		}
		if (HasHitTerrain() && Voxel.IsTerrain(VoxelNormal))
		{
			return false;
		}
		return HasHit;
	}

	public VoxelRaycastHit PlaceAtVector(Vector3 vector)
	{
		_hit = true;
		_lastVoxel = _currentVoxel;
		_currentHitVector = vector;
		_currentVoxel = Calcule.RoundVecteur(_currentHitVector);
		_currentNormal = Calcule.RoundVecteur(Vector3.zero);
		_position = XYZ.FromVector3(_currentVoxel);
		_placedWithRaycast = false;
		_currentFace = (byte)Calcule.SetBit(0, Calcule.VecteurToFaceIndex(_currentNormal), b: true);
		if (_currentNormal.y != 0f)
		{
			_currentNormal = Vector3.up;
		}
		return this;
	}

	public void Copy(VoxelRaycastHit otherVRH)
	{
		_target = otherVRH._target;
		_hit = otherVRH._hit;
		_lastVoxel = otherVRH._lastVoxel;
		_currentHitVector = otherVRH._currentHitVector;
		_currentVoxel = otherVRH._currentVoxel;
		_currentNormal = otherVRH._currentNormal;
		_position = otherVRH._position;
		_currentFace = otherVRH._currentFace;
		_ray = otherVRH._ray;
		_originalRay = otherVRH._originalRay;
		_distance = otherVRH._distance;
		_placedWithRaycast = otherVRH._placedWithRaycast;
	}
}
