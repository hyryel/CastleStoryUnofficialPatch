using System;
using System.Runtime.Serialization;
using Brix.External.Factories;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.IO.Serialization.ReferenceResolvers;

public class NetworkReferenceResolver : DefaultGameReferenceResolver
{
	private static BrixLogger logger = new BrixLogger();

	public NetworkReferenceResolver(GameObjectRegistry registry)
		: base(registry)
	{
	}

	protected override GameObject ResolveGameObjectReference(string reference)
	{
		return base.Registry.Find(reference);
	}

	protected override string GetGameObjectReference(GameObject gameObject)
	{
		logger.Debug("NetworkReferenceResolver.GetReference() gameObject: " + gameObject.name);
		IdentityHolder identity = GetIdentity(gameObject);
		if (identity == null || !IsReferencedGameObject(identity))
		{
			throw new Exception("Failed getting reference because target was invalid or not registered: value=" + gameObject.name + ".");
		}
		return identity.Id;
	}

	public override string GetGameObjectID(GameObject gameObject)
	{
		if (IsGameObjectReferenced(gameObject))
		{
			return GetGameObjectReference(gameObject);
		}
		return gameObject.name;
	}

	protected override bool IsGameObjectReferenced(GameObject gameObject)
	{
		logger.Debug("NetworkReferenceResolver.IsGameObjectReferenced() gameObject: " + gameObject.name);
		IdentityHolder identity = GetIdentity(gameObject);
		return identity != null && IsReferencedGameObject(identity);
	}

	protected override void AddGOReference(string reference, GameObject gameObject)
	{
		throw new SerializationException("NetworkReferenceResolver does not support deserializing new GameObjects. ");
	}

	public override bool IsAGameObjectReferenceNode(string node)
	{
		return node != "0" && ResolveGameObjectReference(node) != null;
	}

	private IdentityHolder GetIdentity(object value)
	{
		GameObject gameObject = value as GameObject;
		return (gameObject != null) ? gameObject.GetComponent<IdentityHolder>() : null;
	}

	private bool IsReferencedGameObject(IdentityHolder identity)
	{
		return identity != null && base.Registry.Contains(identity.Id);
	}
}
