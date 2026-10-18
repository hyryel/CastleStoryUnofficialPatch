using System.Collections;
using System.Collections.Generic;
using Brix.Engine.FlatWorld.Layers;
using UnityEngine;

namespace Brix.Engine.FlatWorld;

public class ColliderSectorUpdater : MonoBehaviour
{
	public ColliderUpdater prefab;

	public XYZ sectorMvPosition;

	public XYZ sectorMvSize = XYZ.one * 14;

	private static TestWorldConfig3 _test;

	private BoxCollider _boxCollider;

	private float _activeUntil;

	private bool HasChildren => base.transform.childCount != 0;

	private BoxCollider BoxCollider
	{
		get
		{
			if (_boxCollider == null)
			{
				InitBoxCollider();
			}
			return _boxCollider;
		}
	}

	private void Start()
	{
		if (_test == null)
		{
			_test = Object.FindObjectOfType<TestWorldConfig3>();
		}
		UpdatePosition();
	}

	private void OnTriggerStay(Collider collider)
	{
		if (!collider.isTrigger)
		{
			_activeUntil = 1f;
			if (!HasChildren)
			{
				RefreshChildren();
				StartCoroutine(WaitAndDestroyChildren());
			}
		}
	}

	private IEnumerator WaitAndDestroyChildren()
	{
		while (_activeUntil > 0f)
		{
			yield return new WaitForEndOfFrame();
			_activeUntil -= Time.deltaTime;
		}
		Clear();
	}

	[ContextMenu("UpdatePosition")]
	private void UpdatePosition()
	{
		sectorMvPosition = XYZ.Div(XYZ.Div(XYZ.FromVector3(base.transform.position), _test.MvSize), sectorMvSize);
		InitBoxCollider();
	}

	[ContextMenu("InitBoxCollider")]
	private void InitBoxCollider()
	{
		_boxCollider = base.gameObject.GetOrAddComponent<BoxCollider>();
		_boxCollider.size = XYZ.Mul(_test.MvSize, sectorMvSize).ToVector3();
		_boxCollider.center = XYZ.Mul(XYZ.Mul(sectorMvPosition, sectorMvSize), _test.MvSize).ToVector3() + _boxCollider.size / 2f - base.transform.position;
		_boxCollider.isTrigger = true;
	}

	[ContextMenu("Clear")]
	private void Clear()
	{
		foreach (Transform item in base.transform)
		{
			Object.Destroy(item.gameObject);
		}
	}

	[ContextMenu("RefreshChildren")]
	private void RefreshChildren()
	{
		_test.InitLayers();
		IEnumerable<XYZ> occupied = _test.drawer.GetOccupied(XYZ.Mul(sectorMvPosition, sectorMvSize), sectorMvSize);
		foreach (XYZ item in occupied)
		{
			XYZ xYZ = XYZ.Mul(item, _test.MvSize);
			GameObject gameObject = Object.Instantiate(prefab.gameObject);
			gameObject.name = "Collider " + xYZ;
			gameObject.transform.SetParent(base.transform);
			gameObject.transform.position = xYZ.ToVector3();
			gameObject.SetActive(value: true);
		}
	}
}
