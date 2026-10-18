using System;
using Brix.External.Factories;
using Brix.Utils;
using UnityEngine;

namespace Brix.IO.Serialization.ReferenceResolvers;

public class ShallowReferenceResolver : GameObjectReferenceResolver
{
	private const string GAMEOBJECT_PATH_PREFIX = "Unity:";

	private GameObject GameObjectScope;

	public ShallowReferenceResolver(GameObjectRegistry registry, GameObject topMostParent)
		: base(registry)
	{
		GameObjectScope = topMostParent;
		if (GameObjectScope == null)
		{
			throw new ArgumentNullException("topMostParent");
		}
	}

	protected override GameObject ResolveGameObjectReference(string reference)
	{
		if (IsGameObjectPathReference(reference))
		{
			return GameObjectResolver.FindExistingGameObjectInScene(reference);
		}
		return base.ResolveGameObjectReference(reference);
	}

	protected override string GetGameObjectReference(GameObject gameObject)
	{
		if (!IsInSerializationScope(gameObject))
		{
			return AddPathPrefix(gameObject.FullPath());
		}
		return base.GetGameObjectReference(gameObject);
	}

	public override string GetGameObjectID(GameObject gameObject)
	{
		if (IsInSerializationScope(gameObject))
		{
			return GetGameObjectReference(gameObject);
		}
		return gameObject.name;
	}

	private bool IsGameObjectPathReference(string reference)
	{
		return reference.StartsWith("Unity:");
	}

	protected override bool IsGameObjectReferenced(GameObject gameObject)
	{
		return !IsInSerializationScope(gameObject) || base.IsGameObjectReferenced(gameObject);
	}

	public override bool IsAGameObjectReferenceNode(string node)
	{
		return node.StartsWith("Unity:") || base.IsAGameObjectReferenceNode(node);
	}

	private string RemovePathPrefix(string reference)
	{
		return reference.Substring(6);
	}

	private string AddPathPrefix(string fullPath)
	{
		return "Unity:" + fullPath;
	}

	private bool IsInSerializationScope(GameObject gameObject)
	{
		return GameObjectScope == gameObject || GameObjectScope.IsParentOf(gameObject);
	}
}
