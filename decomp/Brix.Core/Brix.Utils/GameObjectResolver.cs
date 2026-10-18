using System;
using System.Linq;
using Brix.IO.Serialization.ReferenceResolvers;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Brix.Utils;

public static class GameObjectResolver
{
	private static BrixLogger logger = new BrixLogger();

	public static GameObject ResolveGameObjectInScene(string path, IHierarchyPathNodeResolver resolver)
	{
		if ("/".Equals(path) || string.IsNullOrEmpty(path))
		{
			return null;
		}
		string[] array = (from x in path.Split('/')
			where !string.IsNullOrEmpty(x)
			select x).ToArray();
		if (array.Length == 0)
		{
			return null;
		}
		try
		{
			GameObject gameObject = null;
			int num = 0;
			do
			{
				gameObject = ResolveNodePath(gameObject, array[num], resolver);
				num++;
			}
			while (num < array.Length);
			return gameObject;
		}
		catch (Exception innerException)
		{
			throw new JsonSerializationException($"GameObject resolving failed for path '{path}' for GameObject '", innerException);
		}
	}

	private static GameObject ResolveNodePath(GameObject context, string node, IHierarchyPathNodeResolver resolver)
	{
		GameObject gameObject = null;
		if (resolver.IsAGameObjectReferenceNode(node))
		{
			gameObject = resolver.GetReferencedGameObject(node);
			if (gameObject == null)
			{
				string arg = ((context == null) ? "/" : context.FullPath());
				throw new JsonSerializationException($"Could not find GO with ReferenceID '{node}' at path '{arg}' ");
			}
		}
		else
		{
			gameObject = ResolveUnityPathNode(context, node);
		}
		return gameObject;
	}

	private static GameObject ResolveUnityPathNode(GameObject context, string node)
	{
		GameObject gameObject;
		if (context == null)
		{
			gameObject = SceneManager.GetActiveScene().GetRootGameObjects().Slinq()
				.Where((GameObject g, string name) => g.name == name, node)
				.FirstOrDefault();
			if (gameObject == null)
			{
				throw new JsonSerializationException($"Could not find root GO with the name '{node}'");
			}
		}
		else
		{
			Transform transform = context.transform.Cast<Transform>().FirstOrDefault((Transform child) => child.name == node);
			if (transform == null)
			{
				logger.WarnFormat("Missing gameobject '{0}' at path '{1}'", node, context.FullPath());
				gameObject = new GameObject(node);
				gameObject.transform.SetParent(context.transform, worldPositionStays: false);
			}
			else
			{
				gameObject = transform.gameObject;
			}
		}
		return gameObject;
	}

	public static GameObject FindExistingGameObjectInScene(string path)
	{
		GameObject gameObject = FindGameObjectInScene(path);
		if (gameObject != null)
		{
			return gameObject;
		}
		throw new JsonReaderException("Could not find the GameObject '" + path + "' in the scene. Make sure the GameObject exists before trying to load.");
	}

	public static GameObject FindGameObjectInScene(string parentPath)
	{
		return "/".Equals(parentPath) ? null : GameObject.Find(parentPath);
	}

	public static Transform FindOrCreateGameObject(string path)
	{
		GameObject gameObject = FindGameObjectInScene(path);
		if (gameObject != null)
		{
			return gameObject.transform;
		}
		if ("/".Equals(path))
		{
			return null;
		}
		string[] array = (from x in path.Split('/')
			where !string.IsNullOrEmpty(x)
			select x).ToArray();
		if (array.Length == 0)
		{
			return null;
		}
		gameObject = GameObject.Find("/" + array[0]);
		Transform transform;
		Transform parent = (transform = ((gameObject != null) ? gameObject.transform : new GameObject(array[0]).transform));
		for (int num = 1; num < array.Length; num++)
		{
			transform = FindChildOrCreate(parent, array[num]);
			parent = transform;
		}
		return transform;
	}

	private static Transform FindChildOrCreate(Transform parent, string name)
	{
		Transform transform = parent.transform.Cast<Transform>().FirstOrDefault((Transform child) => child.name == name);
		if (transform == null)
		{
			transform = new GameObject(name).transform;
			transform.SetParent(parent);
		}
		return transform;
	}
}
