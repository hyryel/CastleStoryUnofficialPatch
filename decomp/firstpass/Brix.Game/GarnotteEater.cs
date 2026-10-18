using System;
using System.Collections.Generic;
using Brix.Game.AI;
using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.Components;
using Brix.Game.Semantique;
using Brix.Legacy;
using Brix.Utils;
using Brix.Utils.Factories;
using UnityEngine;

namespace Brix.Game;

[RequireComponent(typeof(Transaction))]
public class GarnotteEater : UpdatableGameComponent<GarnotteEater>
{
	private Adjectif eatOnlyThis;

	private float eatingRadius = 4f;

	private float until;

	private Func<GameObject, bool> Filter;

	private static int LAYERS = 8192;

	protected override float SlowUpdateRate => 0.1f;

	public override void Initialize()
	{
		base.Initialize();
		Filter = (GameObject go) => CanEat(go);
	}

	public void EatOnlySimilarsTo(GameObject referenceObject, float seconds = -1f)
	{
		if (referenceObject == null)
		{
			Eat(seconds);
		}
		else
		{
			EatOnly(Apparence.PrincipaleOf(referenceObject), seconds);
		}
	}

	public void EatOnly(Adjectif adjectif, float seconds = -1f)
	{
		eatOnlyThis = adjectif;
		Eat(seconds);
	}

	public virtual void Eat(float seconds = -1f)
	{
		base.enabled = true;
		if (seconds == -1f)
		{
			until = float.MaxValue;
		}
		else
		{
			until = Time.time + seconds;
		}
	}

	public void StarveIn(float seconds)
	{
		until = Time.time + seconds;
	}

	public void Starve()
	{
		eatOnlyThis = null;
		base.enabled = false;
	}

	protected override void BrixSlowUpdate()
	{
		AbsorbGarnottes();
		if (Time.time > until)
		{
			Starve();
		}
	}

	private void AbsorbGarnottes()
	{
		HashSet<GameObject> result = CollectionPool.RequestHashSet<GameObject>();
		KnowledgeUtility.FindAllInRadius(base.transform.position, eatingRadius, LAYERS, Filter, (GameObject go) => go, ref result);
		AbsorbGarnottes(result);
		result.Dispose();
	}

	private void AbsorbGarnottes(HashSet<GameObject> gos)
	{
		foreach (GameObject go in gos)
		{
			if (base.recepteur.HasRoomFor(go))
			{
				base.recepteur.AddObject(go);
			}
		}
	}

	private bool CanEat(GameObject go)
	{
		if (!go.WithComponent<Apparence, Adjectif, bool>(Eatable, _default: false, eatOnlyThis))
		{
			return false;
		}
		return go.WithComponent((Garnotte g) => !g.Sinking, _default: false);
	}

	private static bool Eatable(Apparence apparence, Adjectif eatOnly)
	{
		if (eatOnly is Ressource && apparence.Principale.GetKey() != eatOnly.GetKey())
		{
			return false;
		}
		return apparence.GoesInBag();
	}
}
