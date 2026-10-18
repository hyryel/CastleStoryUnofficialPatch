using System;
using Brix.External.Factories;
using Brix.Game.Components;
using Brix.Game.Semantique;
using Brix.Lifecycle.Pooling;
using UnityEngine;

namespace Brix.Utils.Factories;

public static class GameObjectExtender
{
	public static void ReleaseImmediateFactoryCreatedChildren(this GameObject gameObject)
	{
		gameObject.transform.ReleaseImmediateFactoryCreatedChildren();
	}

	public static void ReleaseImmediateFactoryCreatedChildren(this Transform transform)
	{
		for (int num = transform.childCount - 1; num > 0; num--)
		{
			Transform child = transform.GetChild(num);
			FactoryImprint component = child.GetComponent<FactoryImprint>();
			if (!component.IsNullOrReleased())
			{
				ObjectPoolSingleton.Release(component.gameObject);
			}
		}
	}

	public static T Get<T>(this GameObject go) where T : UnityEngine.Object
	{
		if (typeof(T) == typeof(GameObject))
		{
			return go as T;
		}
		return go.GetComponent<T>();
	}

	public static TReturn WithComponent<TComponent, TReturn>(this GameObject go, Func<TComponent, TReturn> f, TReturn _default) where TComponent : Component
	{
		if (go.IsNullOrReleased())
		{
			return _default;
		}
		TComponent component = go.GetComponent<TComponent>();
		return (!component) ? _default : f(component);
	}

	public static TReturn WithComponent<TComponent, TReturn>(this Component component, Func<TComponent, TReturn> f, TReturn _default) where TComponent : Component
	{
		if (component == null || component.gameObject.IsNullOrReleased())
		{
			return _default;
		}
		TComponent component2 = component.GetComponent<TComponent>();
		return (!component2) ? _default : f(component2);
	}

	public static TReturn WithComponent<TComponent, TContext, TReturn>(this GameObject go, Func<TComponent, TContext, TReturn> f, TReturn _default, TContext context) where TComponent : Component
	{
		if (go.IsNullOrReleased())
		{
			return _default;
		}
		TComponent component = go.GetComponent<TComponent>();
		return (!component) ? _default : f(component, context);
	}

	public static void WithComponent<TComponent, TContext>(this GameObject go, Action<TComponent, TContext> f, TContext context) where TComponent : Component
	{
		if (!(go == null) && !go.gameObject.IsNullOrReleased())
		{
			TComponent component = go.GetComponent<TComponent>();
			if ((bool)component)
			{
				f(component, context);
			}
		}
	}

	public static void WithComponent<TComponent, TContext>(this Component component, Action<TComponent, TContext> f, TContext context) where TComponent : Component
	{
		if (!(component == null) && !component.gameObject.IsNullOrReleased())
		{
			TComponent component2 = component.GetComponent<TComponent>();
			if ((bool)component2)
			{
				f(component2, context);
			}
		}
	}

	public static void WithComponent<TComponent>(this Component component, Action<TComponent> f) where TComponent : Component
	{
		if (!(component == null) && !component.gameObject.IsNullOrReleased())
		{
			TComponent component2 = component.GetComponent<TComponent>();
			if ((bool)component2)
			{
				f(component2);
			}
		}
	}

	public static void WithComponent<TComponent>(this GameObject go, Action<TComponent> f) where TComponent : Component
	{
		if (!(go == null) && !go.gameObject.IsNullOrReleased())
		{
			TComponent component = go.GetComponent<TComponent>();
			if ((bool)component)
			{
				f(component);
			}
		}
	}

	public static void Try<T>(this T component, Action<T> action) where T : Component
	{
		if (!(component == null) && !component.gameObject.IsNullOrReleased())
		{
			action(component);
		}
	}

	public static TReturn WithRecepteur<TReturn>(this IGameComponent gc, Func<Recepteur, TReturn> f, TReturn _default)
	{
		if (gc.IsNullOrReleased())
		{
			return _default;
		}
		Recepteur recepteur = gc.recepteur;
		return (!recepteur) ? _default : f(recepteur);
	}

	public static TReturn WithRecepteur<TReturn, TContext>(this IGameComponent gc, Func<Recepteur, TContext, TReturn> f, TReturn _default, TContext context)
	{
		if (gc.IsNullOrReleased())
		{
			return _default;
		}
		Recepteur recepteur = gc.recepteur;
		return (!recepteur) ? _default : f(recepteur, context);
	}

	public static void WithRecepteur(this IGameComponent gc, Action<Recepteur> f)
	{
		if (!gc.IsNullOrReleased())
		{
			Recepteur recepteur = gc.recepteur;
			if ((bool)recepteur)
			{
				f(recepteur);
			}
		}
	}

	public static void WithRecepteur<TContext>(this IGameComponent gc, Action<Recepteur, TContext> f, TContext context)
	{
		if (!gc.IsNullOrReleased())
		{
			Recepteur recepteur = gc.recepteur;
			if ((bool)recepteur)
			{
				f(recepteur, context);
			}
		}
	}
}
