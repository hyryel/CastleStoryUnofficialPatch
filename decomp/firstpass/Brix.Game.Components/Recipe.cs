using System;
using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.External.Factories;
using Brix.Game.AI;
using Brix.Game.Semantique;
using Brix.Lifecycle.Pooling;
using Brix.Transactions;
using Brix.UI.Icons;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components;

public class Recipe : IPoolable, Brix.Utils.IResetable, IDisposable
{
	private RecipeInfo info;

	private int heat;

	private int work;

	private bool finished;

	private Description ingredients;

	public RecipeInfo Info => info;

	public Factory.AssetKey Creates => info.Creates;

	public int QryCreated => info.QtyCreated;

	public bool DropOnCompletion => info.DropOnCompletion;

	public IconKey Icon => info.icon;

	public IconKey Drawing => info.drawing;

	public bool NeedsHeat => heat > 0;

	public bool NeedsWork => work > 0;

	public bool Finished => finished;

	public void Set(RecipeInfo info, int heat, int work, bool finished, Description ingredients)
	{
		this.info = info;
		this.heat = heat;
		this.work = work;
		this.finished = finished;
		this.ingredients = ingredients;
	}

	public void SetIngredient(Ressource r, int qty)
	{
		ingredients.Set(r, qty);
	}

	public void Restart()
	{
		finished = false;
		work = info.work;
		heat = info.work;
	}

	public bool IsReadyToFinish(Recepteur recepteur)
	{
		return !NeedsWork && !NeedsHeat && HasIngredientsIn(recepteur);
	}

	public bool HasIngredientsIn(Recepteur recepteur)
	{
		foreach (Adjectif item in ingredients.Adjectifs.NoLeak())
		{
			Adjectif adjectif = recepteur.ContentDescription.Get(item);
			if (adjectif == null || adjectif.quantifiable.valeur < item.quantifiable.valeur)
			{
				return false;
			}
		}
		return true;
	}

	public void Finish(Recepteur recepteur, Action<GameObject> postProcess = null)
	{
		if (NetworkServer.active)
		{
			ConsumeAllFrom(recepteur);
			ProduceInto(recepteur, postProcess);
			finished = true;
		}
	}

	private void ProduceInto(Recepteur recepteur, Action<GameObject> postProcess = null)
	{
		List<GameObject> gos = CollectionPool.RequestList<GameObject>();
		SpawnAll(ref gos, recepteur.transform.position);
		recepteur.affiliation.CopyFactionTo(gos);
		if (postProcess != null)
		{
			gos.Slinq().ForEach(delegate(GameObject go, Action<GameObject> f)
			{
				f(go);
			}, postProcess);
		}
		if (DropOnCompletion)
		{
			DropAll(gos, recepteur);
		}
		else
		{
			gos.Slinq().ForEach(delegate(GameObject go, Recepteur r)
			{
				r.AddObject(go);
			}, recepteur);
		}
		gos.Dispose();
	}

	private void DropAll(List<GameObject> gos, Recepteur recepteur)
	{
		Transform transform = recepteur.transform;
		Recepteur.ScatterObjects(gos, transform.position + transform.right * -1.5f + transform.up * 1.5f);
	}

	public void WorkOn()
	{
		work--;
	}

	private void SpawnAll(ref List<GameObject> gos, Vector3 pos)
	{
		for (int i = 0; i < QryCreated; i++)
		{
			GameObject item = Transactor.SpawnUNet(Creates, null, pos, Quaternion.identity);
			gos.Add(item);
		}
	}

	private void ConsumeAllFrom(Recepteur recepteur)
	{
		foreach (Ressource item in ingredients.GetRessources().NoLeak())
		{
			for (int i = 0; i < item.quantifiable.valeur; i++)
			{
				ConsumeFrom(recepteur, item);
			}
		}
	}

	public bool ConsumeFrom(Recepteur recepteur, Ressource adj)
	{
		if (recepteur.FirstObjectOf(adj, out var go))
		{
			ReleaseAndTryUnspawn(go);
			return true;
		}
		return false;
	}

	public bool HeatFrom(Recepteur recepteur)
	{
		if (ConsumeFrom(recepteur, Adjectif.orangeCrystal))
		{
			heat--;
			return true;
		}
		return false;
	}

	public static void ReleaseAndTryUnspawn(GameObject go)
	{
		ObjectPoolSingleton.Release(go);
		NetworkIdentity component = go.GetComponent<NetworkIdentity>();
		if (component != null && !component.netId.IsEmpty())
		{
			NetworkServer.UnSpawn(go);
		}
	}

	public static void Dispose(Recipe r)
	{
		r?.Dispose();
	}

	public void Dispose()
	{
		ResetObject();
		ObjectPool<Recipe>.Release(this);
	}

	public void ResetObject()
	{
		Set(null, 0, 0, finished: false, null);
	}
}
