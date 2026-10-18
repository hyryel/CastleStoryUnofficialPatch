using System.Runtime.Serialization;
using Brix.IO.Serialization.Json.Converters;
using Brix.Transactions;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Game.Network;

[JsonObject(MemberSerialization.OptIn)]
public class NetworkHandle<T> : Locator<T>.Handle
{
	[JsonConverter(typeof(GameObjectReferenceConverter))]
	[JsonProperty]
	private GameObject _transactorGO;

	[JsonProperty]
	protected int _objectId;

	protected Registry<T> _registry;

	protected T _t;

	private readonly bool _isPublicHandle;

	public virtual T Value => _t;

	[JsonConstructor]
	public NetworkHandle(GameObject transactorGO, int objectId)
	{
		_transactorGO = transactorGO;
		_objectId = objectId;
	}

	public NetworkHandle(NetworkLocator<T> locator, T t)
	{
		_transactorGO = locator.Transactor.gameObject;
		_registry = locator.Registry;
		_objectId = _registry.RegisterObject(t);
		_t = t;
		_isPublicHandle = true;
	}

	[OnDeserialized]
	private void OnAfterDeserialized(StreamingContext context)
	{
		_registry = NetworkLocator<T>.RetrieveLocator(_transactorGO.GetComponent<Transactor>()).Registry;
		_t = _registry.GetObjectById(_objectId);
	}

	~NetworkHandle()
	{
		if (_isPublicHandle)
		{
			_registry.ForgetObject(_objectId);
		}
	}
}
