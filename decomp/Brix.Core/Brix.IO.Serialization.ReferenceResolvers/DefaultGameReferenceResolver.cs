using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Brix.External.Factories;
using Brix.Utils.Logging;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Brix.IO.Serialization.ReferenceResolvers;

public abstract class DefaultGameReferenceResolver : IReferenceResolver, IHierarchyPathNodeResolver
{
	private static BrixLogger logger;

	private static Func<IReferenceResolver> defaultReferenceResolverConstructor;

	private IReferenceResolver _defaultReferenceResolver;

	private Stack<GameObject> gameObjectContextStack = new Stack<GameObject>();

	public MonoBehaviourReferenceResolver componentReferenceResolver { get; private set; }

	public IGameObjectRegistry Registry { get; set; }

	static DefaultGameReferenceResolver()
	{
		logger = new BrixLogger();
		Type type = RearViewMirror.GetType("DefaultReferenceResolver");
		ConstructorInfo constructor = type.GetConstructor(new Type[0]);
		defaultReferenceResolverConstructor = Expression.Lambda<Func<IReferenceResolver>>(Expression.New(constructor), new ParameterExpression[0]).Compile();
	}

	protected DefaultGameReferenceResolver(IGameObjectRegistry registry)
	{
		if (registry == null)
		{
			throw new ArgumentNullException("registry");
		}
		Registry = registry;
		_defaultReferenceResolver = defaultReferenceResolverConstructor();
		componentReferenceResolver = new MonoBehaviourReferenceResolver();
	}

	public void PushContext(GameObject contextualGameObject)
	{
		gameObjectContextStack.Push(contextualGameObject);
	}

	public GameObject PopContext()
	{
		return gameObjectContextStack.Pop();
	}

	public GameObject PeekContext()
	{
		return (gameObjectContextStack.Count == 0) ? null : gameObjectContextStack.Peek();
	}

	protected abstract GameObject ResolveGameObjectReference(string reference);

	public virtual object ResolveReference(object context, string reference)
	{
		logger.Debug("GameObjectReferenceResolver.ResolveReference() reference to resolve: " + reference);
		if (!IsAGameObjectReferenceNode(reference))
		{
			return _defaultReferenceResolver.ResolveReference(context, reference);
		}
		return ResolveGameObjectReference(reference);
	}

	public abstract string GetGameObjectID(GameObject gameObject);

	protected abstract string GetGameObjectReference(GameObject gameObject);

	public virtual string GetReference(object context, object value)
	{
		GameObject gameObject = value as GameObject;
		if (!gameObject)
		{
			return _defaultReferenceResolver.GetReference(context, value);
		}
		return GetGameObjectReference(gameObject);
	}

	protected abstract bool IsGameObjectReferenced(GameObject gameObject);

	public virtual bool IsReferenced(object context, object value)
	{
		GameObject gameObject = value as GameObject;
		if (!gameObject)
		{
			return _defaultReferenceResolver.IsReferenced(context, value);
		}
		return IsGameObjectReferenced(gameObject);
	}

	protected abstract void AddGOReference(string reference, GameObject gameObject);

	public virtual void AddReference(object context, string reference, object value)
	{
		GameObject gameObject = value as GameObject;
		logger.Debug("GameObjectReferenceResolver.AddReference() Adding reference: " + reference);
		if (gameObject == null)
		{
			_defaultReferenceResolver.AddReference(context, reference, value);
		}
		else
		{
			AddGOReference(reference, gameObject);
		}
	}

	public abstract bool IsAGameObjectReferenceNode(string node);

	public GameObject GetReferencedGameObject(string node)
	{
		return ResolveGameObjectReference(node);
	}

	public virtual void Clear()
	{
		gameObjectContextStack.Clear();
		componentReferenceResolver.Clear();
		Registry.Clear();
	}
}
