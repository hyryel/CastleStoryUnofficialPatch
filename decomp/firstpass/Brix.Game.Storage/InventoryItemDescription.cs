using System;
using System.Collections.Generic;
using Brix.Game.AI;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Storage;

public class InventoryItemDescription
{
	public string name;

	public string codename;

	public Adjectif adjectif;

	public Func<Sprite> icon;

	public Func<Sprite> storedIcon;

	public InventoryItemDescription()
	{
	}

	public InventoryItemDescription(string name, Adjectif adjectif, Func<Sprite> icon)
	{
		this.name = name;
		this.adjectif = adjectif;
		this.icon = icon;
		storedIcon = icon;
	}

	public static void ItemDescriptionsFromAdjectiveType(ref List<InventoryItemDescription> buffer, AdjectiveInfo.AdjectiveType type)
	{
		(from info in AdjectiveInfo.GetInfosFromType(type).Slinq().Select(FromAdjectiveInfo)
			where info.UIRelevant()
			select info).AddTo(buffer);
	}

	public static InventoryItemDescription FromAdjectiveInfo(AdjectiveInfo info)
	{
		InventoryItemDescription inventoryItemDescription = new InventoryItemDescription();
		inventoryItemDescription.CopyAdjectiveInfo(info);
		return inventoryItemDescription;
	}

	public void CopyAdjectiveInfo(AdjectiveInfo info)
	{
		name = info.descriptiveName;
		codename = info.codeName;
		adjectif = Adjectif.StringToAdjectif(info.codeName);
		icon = () => info.icon;
		storedIcon = () => info.storedIcon;
	}

	public bool UIRelevant()
	{
		return adjectif != null && adjectif.GetKey().GetInfo().UIRelevant;
	}
}
