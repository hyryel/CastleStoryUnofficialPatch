using System;
using System.Runtime.Serialization;
using Brix.External.Signals;
using Brix.IO.Serialization.Json.Converters;
using Brix.Transactions;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Game.Network;

[JsonObject(MemberSerialization.OptIn)]
public class NetworkHatch<T>
{
	public class Data
	{
		public Signal<T> _signal = new Signal<T>();

		public T _t;
	}

	[JsonProperty]
	[JsonConverter(typeof(GameObjectReferenceConverter))]
	private GameObject _transactorGO;

	[JsonProperty]
	protected int _objectId;

	protected Registry<Data> _registry;

	private Data _data;

	private readonly bool _isPublicHandle;

	[JsonConstructor]
	private NetworkHatch(GameObject transactorGO, int objectId)
	{
		_transactorGO = transactorGO;
		_objectId = objectId;
	}

	public NetworkHatch(NetworkLocator<Data> locator, T t, int objectId)
	{
		_isPublicHandle = true;
		_transactorGO = locator.Transactor.gameObject;
		_registry = locator.Registry;
		_objectId = objectId;
		if (!_registry.TryGetObject(_objectId, out _data))
		{
			_registry.SetObjectWithId(_data = new Data(), _objectId);
		}
		_data._t = t;
		Hatch();
	}

	public void OnHatched(Action<T> action)
	{
		if (_data._signal != null)
		{
			_data._signal.Connect(action);
		}
		else
		{
			action(_data._t);
		}
	}

	private void Hatch()
	{
		_data._signal.Invoke(_data._t);
		_data._signal = null;
	}

	[OnDeserialized]
	private void OnAfterDeserialized(StreamingContext context)
	{
		_registry = NetworkLocator<Data>.RetrieveLocator(_transactorGO.GetComponent<Transactor>()).Registry;
		if (!_registry.TryGetObject(_objectId, out _data))
		{
			_registry.SetObjectWithId(_data = new Data(), _objectId);
		}
	}

	~NetworkHatch()
	{
		if (_isPublicHandle)
		{
			_registry.ForgetObject(_objectId);
		}
	}
}
