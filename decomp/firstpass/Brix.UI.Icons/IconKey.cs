using System;
using System.Collections.Generic;
using Brix.Utils;
using UnityEngine;

namespace Brix.UI.Icons;

[Serializable]
public class IconKey
{
	[Serializable]
	public class IconKeyLink
	{
		public string key;

		public Sprite Get64 => FromString(key).Get64();

		public Sprite Get32 => FromString(key).Get32();

		public void Validate()
		{
			if (string.IsNullOrEmpty(key) || KeyExists(key))
			{
				return;
			}
			throw new Exception("Invalid Icon Key");
		}
	}

	[SerializeField]
	public string Name = "FIXME";

	public string Database = string.Empty;

	private int id;

	private static int lastUsedID;

	private static Dictionary<int, IconKey> keys = new Dictionary<int, IconKey>();

	private static Dictionary<string, IconKey> keysByString = new Dictionary<string, IconKey>();

	public int Id => id;

	public IconKey(string database, string name)
	{
		Database = database;
		Name = name;
		id = lastUsedID++;
		keys.Add(id, this);
		keysByString.Add(name, this);
	}

	public static IconKey FromId(int id)
	{
		return keys.GetValueOrCreate(id, IconKeys.Missing);
	}

	public static IconKey FromString(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return IconKeys.Missing;
		}
		return keysByString.GetValueOrCreate(s, IconKeys.Missing);
	}

	public static bool KeyExists(string s)
	{
		return keysByString.ContainsKey(s);
	}

	public Sprite Get64()
	{
		return IconDB.GetIcon(this).Image64;
	}

	public Sprite Get32()
	{
		return IconDB.GetIcon(this).Image32;
	}
}
