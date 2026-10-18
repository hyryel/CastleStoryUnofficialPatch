using System;
using System.Collections.Generic;
using Brix.Game.AI;
using Brix.Game.Storage;
using Brix.Lua;
using MoonSharp.Interpreter;
using Smooth.Slinq;
using UnityEngine;

public class InventoryItemCount
{
	public string name;

	public Adjectif adjectif;

	public Func<Sprite> icon;

	public Func<Sprite> storedIcon;

	public int content;

	public int capacity;

	private static DynValue _str_name = DynValue.NewString("name");

	private static DynValue _str_codeName = DynValue.NewString("codeName");

	private static DynValue _str_icon = DynValue.NewString("icon");

	private static DynValue _str_storedIcon = DynValue.NewString("storedIcon");

	private static DynValue _str_content = DynValue.NewString("content");

	private static DynValue _str_capacity = DynValue.NewString("capacity");

	public InventoryItemCount(InventoryItemDescription item)
	{
		name = item.name;
		adjectif = item.adjectif;
		icon = item.icon;
		icon = item.storedIcon;
	}

	public static void FromDescriptions(ref List<InventoryItemCount> result, List<InventoryItemDescription> items)
	{
		(from i in items.Slinq()
			select new InventoryItemCount(i)).AddTo(result);
	}

	public static void FromDescriptions(ref DynValue result, ref Dictionary<DynValue, Adjectif> map, List<InventoryItemDescription> items)
	{
		foreach (InventoryItemDescription item in items)
		{
			DynValue dynValue = CountTableFor(item);
			result.Table.Append(dynValue);
			map.Add(dynValue, item.adjectif);
		}
	}

	private static DynValue CountTableFor(InventoryItemDescription desc)
	{
		DynValue result = DynValue.NewPrimeTable();
		result.Table.Set(_str_name, DynValue.NewString(desc.name));
		result.Table.Set(_str_codeName, DynValue.NewString(desc.codename));
		result.Table.Set(_str_icon, UserData.CreateWithDescriptor(desc.icon(), LuaLibs.spriteDesc));
		result.Table.Set(_str_storedIcon, UserData.CreateWithDescriptor(desc.storedIcon(), LuaLibs.spriteDesc));
		result.Table.Set(_str_content, DynValue.NewNumber(0.0));
		result.Table.Set(_str_capacity, DynValue.NewNumber(0.0));
		return result;
	}
}
