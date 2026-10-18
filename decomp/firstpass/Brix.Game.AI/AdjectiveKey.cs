using System;
using System.Collections.Generic;
using UnityEngine;

namespace Brix.Game.AI;

[Serializable]
public class AdjectiveKey
{
	public static List<AdjectiveKey> AllKeys = new List<AdjectiveKey>();

	[SerializeField]
	private string _codeName;

	private AdjectiveInfo _info;

	public string codeName => _codeName;

	private AdjectiveKey()
	{
	}

	public AdjectiveKey(string codeName)
	{
		_codeName = codeName;
		AllKeys.Add(this);
	}

	public AdjectiveInfo GetInfo()
	{
		if (_info == null)
		{
			_info = AdjectiveInfo.GetInfo(_codeName);
		}
		return _info;
	}

	public Adjectif GetAdjectif()
	{
		return Adjectif.StringToAdjectif(_codeName);
	}
}
