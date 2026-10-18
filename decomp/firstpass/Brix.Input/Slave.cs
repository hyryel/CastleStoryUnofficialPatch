using System.Collections.Generic;
using Brix.Game;
using Brix.Game.Components.Volumes;
using Brix.Lifecycle.Pooling;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Input;

public class Slave
{
	private Dictionary<GameObject, int> _objectRealLayer = new Dictionary<GameObject, int>();

	private GameObject _heldObject;

	private BrixLogger logger = new BrixLogger(typeof(Slave).FullName);

	public Vector3 Position
	{
		get
		{
			if (_heldObject != null)
			{
				return _heldObject.transform.position;
			}
			return Vector3.zero;
		}
		set
		{
			if (_heldObject != null)
			{
				_heldObject.transform.position = value;
			}
		}
	}

	public Quaternion Rotation
	{
		get
		{
			if (_heldObject != null)
			{
				return _heldObject.transform.rotation;
			}
			return Quaternion.identity;
		}
		set
		{
			if (_heldObject != null)
			{
				_heldObject.transform.rotation = value;
			}
		}
	}

	public GameObject HeldObject => _heldObject;

	public Slave()
	{
	}

	public Slave(GameObject held, PickerComponent parent)
	{
		held.SetActive(value: true);
		held.transform.parent = parent.transform;
		HoldObject(held);
		held.SetActive(parent.Active);
		IVolume component = held.GetComponent<IVolume>();
		if (component != null)
		{
			component.Faction = User.LocalUser.faction;
		}
	}

	public T GetHeldComponent<T>()
	{
		if (_heldObject != null)
		{
			return (T)(object)_heldObject.GetComponent(typeof(T));
		}
		return default(T);
	}

	public virtual void HoldObject(GameObject gameObject)
	{
		DestroyHeldObject();
		_heldObject = gameObject;
		TurnOffRaycast();
	}

	public virtual GameObject ReleaseHeldObject()
	{
		GameObject result = null;
		if (_heldObject != null)
		{
			TurnOnRaycast();
			_objectRealLayer.Clear();
			result = _heldObject;
			_heldObject = null;
		}
		return result;
	}

	private void TurnOffRaycast()
	{
		if (!(_heldObject == null))
		{
			Transform[] componentsInChildren = _heldObject.GetComponentsInChildren<Transform>();
			foreach (Transform transform in componentsInChildren)
			{
				_objectRealLayer.Add(transform.gameObject, transform.gameObject.layer);
				transform.gameObject.layer = 2;
			}
		}
	}

	private void TurnOnRaycast()
	{
		foreach (GameObject key in _objectRealLayer.Keys)
		{
			if (key != null)
			{
				key.layer = _objectRealLayer[key];
			}
			else
			{
				logger.Warn("An object or more likely its child was destroyed while being grabbed by a picker. The object might be broken when placed.");
			}
		}
	}

	public virtual void DestroyHeldObject()
	{
		TurnOnRaycast();
		if (_heldObject != null)
		{
			ObjectPoolSingleton.Release(_heldObject);
		}
		_heldObject = null;
		_objectRealLayer.Clear();
	}

	public void DeactivateHeldObject()
	{
		if (_heldObject != null)
		{
			_heldObject.SetActive(value: false);
			_heldObject.transform.position = Vector3.zero;
		}
	}

	public void ActivateHeldObject()
	{
		if (_heldObject != null)
		{
			_heldObject.SetActive(value: true);
		}
	}
}
