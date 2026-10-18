using System;
using Brix.Components;
using UnityEngine;

namespace Brix.External.Factories;

public class FactorylessImprint : MonoBehaviour, IdentityHolder
{
	private static bool _onApplicationQuit;

	[SerializeField]
	internal string _id = null;

	private bool _registered = false;

	public string Id
	{
		get
		{
			return _id;
		}
		set
		{
			if (_registered && _id != value)
			{
				throw new InvalidOperationException();
			}
			_id = value;
		}
	}

	public bool Registered
	{
		get
		{
			return _registered;
		}
		set
		{
			_registered = value;
		}
	}

	private void Start()
	{
		if (!string.IsNullOrEmpty(_id))
		{
			RegisterId();
		}
	}

	private void OnDestroy()
	{
		UnregisterId();
	}

	private void OnApplicationQuit()
	{
		_onApplicationQuit = true;
	}

	private void RegisterId()
	{
		if (!_registered)
		{
			BrixSingleton<MainGameObjectRegistry>.Instance.Register(Id, base.gameObject);
			_registered = true;
		}
	}

	private void UnregisterId()
	{
		if (!string.IsNullOrEmpty(_id) && _registered && !_onApplicationQuit)
		{
			BrixSingleton<MainGameObjectRegistry>.Instance.Unregister(_id);
		}
	}
}
