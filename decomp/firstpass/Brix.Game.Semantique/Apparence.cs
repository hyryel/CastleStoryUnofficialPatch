using System;
using System.Collections.Generic;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Utils.Collections;
using Brix.Utils.Factories;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;

namespace Brix.Game.Semantique;

public class Apparence : GameComponent, IDescriptor
{
	private static class CacheWrapper<T> where T : Adjectif
	{
		public static T Cache(Apparence a)
		{
			Adjectif adjectif = a.Adjectifs.FirstOrDefault((Adjectif adj) => adj is T);
			return (adjectif != null) ? (adjectif as T) : ((T)null);
		}
	}

	[HideInInspector]
	[SerializeField]
	public Description description = new Description();

	private Ressource _principale;

	private CategorieOutil _categorieOutil;

	private static List<Adjectif> _BagStuff = new List<Adjectif>
	{
		Adjectif.stones,
		Adjectif.rawIron,
		Adjectif.orangeCrystal,
		Adjectif.blueCrystal,
		Adjectif.plant,
		Adjectif.bomb
	};

	public static HashSet<Adjectif> BagStuff = _BagStuff.Slinq().ToHashSet();

	private static List<AdjectiveKey> BagKeys = (from s in _BagStuff.Slinq()
		select s.GetKey()).ToList();

	public Ressource Principale => _principale;

	public Slinq<Adjectif, SelectContext<Adjectif, KeyValuePair<Type, Adjectif>, DictionaryContext<Type, Adjectif>>> Adjectifs => description.Adjectifs;

	public IGameComponent GameComponent => this;

	public Description Description => description;

	public override void Initialize()
	{
		base.Initialize();
		_principale = Cache<Ressource>();
		_categorieOutil = Cache<CategorieOutil>();
	}

	private T Cache<T>() where T : Adjectif
	{
		return CacheWrapper<T>.Cache(this);
	}

	public static bool Is<T>(GameComponent component) where T : Adjectif
	{
		if (component.IsNullOrReleased())
		{
			return false;
		}
		Apparence apparence = component.apparence;
		return Is<T>(apparence);
	}

	public static bool Is<T>(Apparence apparence) where T : Adjectif
	{
		if (apparence.IsNullOrReleased())
		{
			return false;
		}
		return apparence.description.Has<T>() || apparence.ContainsAdjectifOfType<T>();
	}

	public static bool Is<T>(GameObject item) where T : Adjectif
	{
		Apparence component = item.GetComponent<Apparence>();
		if (!component)
		{
			return false;
		}
		return Is<T>(component);
	}

	public static bool Is(GameObject item, Adjectif adjectif)
	{
		Apparence component = item.GetComponent<Apparence>();
		return Is(component, adjectif);
	}

	public bool GoesInBag()
	{
		return BagKeys.Contains(_principale.GetKey());
	}

	public static bool GoesInBag(GameObject go)
	{
		return go.WithComponent((Apparence a) => a.GoesInBag(), _default: false);
	}

	public static bool Is(Apparence apparence, Adjectif adjectif)
	{
		if (apparence.IsNullOrReleased())
		{
			return false;
		}
		return apparence.description.Adjectifs.Contains(adjectif) || apparence.Adjectifs.Any((Adjectif a, Adjectif _adjectif) => a.GetType().IsSubclassOf(_adjectif.GetType()), adjectif);
	}

	public static bool CategorieOutilIs(GameObject go, CategorieOutil type)
	{
		CategorieOutil categorieOutil = CategorieOutilOf(go);
		if (categorieOutil == null)
		{
			return false;
		}
		return categorieOutil.GetType() == type.GetType();
	}

	public static CategorieOutil CategorieOutilOf(GameObject go)
	{
		return go.WithComponent((Apparence a) => a._categorieOutil, null);
	}

	public static bool PrincipaleIs(IDescriptor d, Type wantedType)
	{
		Ressource principale = d.Principale;
		return principale != null && principale.GetType() == wantedType;
	}

	public static bool PrincipaleIs(GameObject go, Ressource re)
	{
		Ressource ressource = PrincipaleOf(go);
		return ressource != null && ressource.GetType() == re.GetType();
	}

	public static bool PrincipaleIs(IGameComponent gc, Ressource re)
	{
		Ressource ressource = PrincipaleOf(gc);
		return ressource != null && ressource.GetType() == re.GetType();
	}

	public static Ressource PrincipaleOf(GameObject go)
	{
		return go.WithComponent((Apparence a) => a._principale, null);
	}

	public static Ressource PrincipaleOf(IGameComponent gc)
	{
		return (gc.IsNullOrReleased() || !(gc.apparence != null)) ? null : gc.apparence._principale;
	}

	public bool ContainsAdjectifOfType<T>()
	{
		foreach (KeyValuePair<Type, Adjectif> item in description.DicoAdjectif)
		{
			if (item.Value is T)
			{
				return true;
			}
		}
		return false;
	}
}
