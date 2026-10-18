using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Game.Network;
using Brix.Game.Semantique;
using Brix.Lifecycle.Pooling;
using Brix.Utils;
using Brix.Utils.Collections;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI;

public class Transaction : NetworkBehaviour
{
	public delegate void SoundDelegate(string soundEvent);

	public GameObject Take(GameObject quoiObj, bool playSound = true)
	{
		if (quoiObj == null)
		{
			throw new Exception("please grab something that makes sense");
		}
		ToolParentTracker component = quoiObj.GetComponent<ToolParentTracker>();
		if (component != null)
		{
			component.SetTrackerType(TrackingTypes.ressource);
		}
		GameComponent component2 = quoiObj.GetComponent<GameComponent>();
		Recepteur parentRecepteur = component2.ParentRecepteur;
		GameComponent component3 = GetComponent<GameComponent>();
		component2.Soulever();
		component3.recepteur.AddObject(quoiObj);
		if (parentRecepteur != null)
		{
			parentRecepteur.InventoryChanged.Invoke();
		}
		return component2.gameObject;
	}

	public GameObject Store(GameObject from, GameObject into, bool playSound = true)
	{
		if (from == null || into == null)
		{
			throw new Exception("please drop something that makes sense");
		}
		GameComponent component = from.GetComponent<GameComponent>();
		GameComponent component2 = into.GetComponent<GameComponent>();
		Recepteur parentRecepteur = component.ParentRecepteur;
		ToolParentTracker component3 = from.GetComponent<ToolParentTracker>();
		if ((bool)component3)
		{
			component3.SetTrackerType(TrackingTypes.ressource);
		}
		component2.recepteur.AddObject(from);
		component.Soulever();
		return from;
	}

	public void StoreEverything(GameObject from, GameObject into, int upTo = -1)
	{
		GameComponent component = from.GetComponent<GameComponent>();
		Recepteur parentRecepteur = component.ParentRecepteur;
		Recepteur recepteur = into.GetComponent<GameComponent>().recepteur;
		if (parentRecepteur == null)
		{
			if (upTo != 1)
			{
				HashSet<GameObject> itemSet = CollectionPool.RequestHashSet<GameObject>();
				if (SimilarSurroundingItems(component, ref itemSet))
				{
					TakeFromEnumerable(recepteur, component.AssetKey, itemSet, upTo);
				}
				else
				{
					recepteur.AddObject(from);
				}
				CollectionPool.Release(ref itemSet);
			}
			else
			{
				recepteur.AddObject(from);
			}
		}
		else if (parentRecepteur.gameObject != from)
		{
			TransferSpecificObjects(parentRecepteur, recepteur, from, upTo);
		}
		else
		{
			TransferWhatever(parentRecepteur, recepteur, upTo);
		}
	}

	private void TransferSpecificObjects(Recepteur source, Recepteur destination, GameObject specificItem, int upTo = -1)
	{
		Factory.AssetKey key = specificItem.GetComponent<GameComponent>().AssetKey;
		List<GameObject> list = (from d in source.StoredItems.Slinq()
			where d.AssetKey == key
			select d.GameObject).ToPooledList();
		TakeFromEnumerable(destination, key, list, upTo);
		list.Dispose();
	}

	private void TransferWhatever(Recepteur source, Recepteur destination, int upTo)
	{
		int num = 0;
		List<GameObject> list = source.StoredGameObjectsSlinq.ToPooledList();
		foreach (GameObject item in list)
		{
			if ((upTo != -1 && num >= upTo) || destination.IsFull())
			{
				break;
			}
			if (destination.HasRoomFor(item))
			{
				destination.AddObject(item);
				num++;
			}
		}
		list.Dispose();
	}

	private bool SimilarSurroundingItems(GameComponent target, ref HashSet<GameObject> itemSet)
	{
		foreach (XYZ obstacle in target.volume.obstacles)
		{
			foreach (IVolume item in VolumeRegistry.VolumesWithPropertyAt(obstacle, VProperty.obstacle))
			{
				if (item != null && !item.GameObject.IsNullOrReleased() && item.AssetKey == target.AssetKey)
				{
					itemSet.Add(item.GameObject);
				}
			}
			foreach (IVolume item2 in VolumeRegistry.VolumesWithPropertyAt(obstacle, VProperty.access))
			{
				if (item2 != null && !item2.GameObject.IsNullOrReleased() && item2.AssetKey == target.AssetKey)
				{
					itemSet.Add(item2.GameObject);
				}
			}
		}
		return itemSet.Count > 0;
	}

	private void TakeFromEnumerable(Recepteur destination, Factory.AssetKey key, ICollection<GameObject> enumerable, int upTo = -1)
	{
		int num = 0;
		foreach (GameObject item in enumerable)
		{
			if (destination.HasRoomFor(key) && (upTo == -1 || num < upTo) && enumerable.Count != 0)
			{
				destination.AddObject(item);
				num++;
				continue;
			}
			break;
		}
	}

	public void ConvertToAmmo(ProjectileWeapon wepon, Recepteur target)
	{
		int num = Mathf.Max(10 - wepon.AmmoCountGetter, 0);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			IDescriptor descriptor = target.StoredItems.FirstOrDefault((IDescriptor item) => item.AssetKey == BuildResources.Bomb);
			if (descriptor != null)
			{
				num2++;
				ObjectPoolSingleton.Release(descriptor.GameObject);
			}
		}
		wepon.AmmoCountGetter += num2;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}
}
