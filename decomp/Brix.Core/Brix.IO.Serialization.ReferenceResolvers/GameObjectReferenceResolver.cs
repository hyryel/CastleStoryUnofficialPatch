using System.Globalization;
using Brix.External.Factories;
using UnityEngine;

namespace Brix.IO.Serialization.ReferenceResolvers;

public class GameObjectReferenceResolver : DefaultGameReferenceResolver
{
	private const string GameobjectKeyPrefix = "go:";

	public GameObjectReferenceResolver(IGameObjectRegistry registry)
		: base(registry)
	{
	}

	protected override GameObject ResolveGameObjectReference(string reference)
	{
		return base.Registry.Find(RemoveKeyPrefix(reference));
	}

	protected override string GetGameObjectReference(GameObject gameObject)
	{
		string text = GameObjectId(gameObject);
		if (!IsGameObjectReferenced(gameObject))
		{
			base.Registry.Register(text, gameObject);
		}
		return AddKeyPrefix(text);
	}

	public override string GetGameObjectID(GameObject gameObject)
	{
		if (IsGameObjectReferenced(gameObject))
		{
			return GetGameObjectReference(gameObject);
		}
		return gameObject.name;
	}

	protected virtual string GameObjectId(GameObject gameObject)
	{
		string text = gameObject.GetComponent<IdentityHolder>()?.Id;
		if (string.IsNullOrEmpty(text))
		{
			text = gameObject.GetInstanceID().ToString(CultureInfo.InvariantCulture);
		}
		return text;
	}

	protected override bool IsGameObjectReferenced(GameObject gameObject)
	{
		return base.Registry.Contains(GameObjectId(gameObject));
	}

	protected override void AddGOReference(string reference, GameObject gameObject)
	{
		base.Registry.Register(RemoveKeyPrefix(reference), gameObject);
	}

	public override bool IsAGameObjectReferenceNode(string node)
	{
		return node.StartsWith("go:");
	}

	private string RemoveKeyPrefix(string reference)
	{
		return reference.Substring(3);
	}

	private string AddKeyPrefix(string key)
	{
		return "go:" + key;
	}
}
