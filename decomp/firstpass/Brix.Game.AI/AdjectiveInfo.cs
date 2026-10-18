using System.Collections.Generic;
using Brix.Components;
using Brix.External.Factories;
using Brix.Game.Components;
using Brix.UI.Icons;
using Brix.Utils.Factories;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.AI;

public class AdjectiveInfo : GameComponent
{
	public enum AdjectiveType
	{
		Adjectif,
		Quantite,
		Limite,
		Ressource,
		Accessoire,
		Aubligation,
		Outil,
		CategorieOutil
	}

	private static Dictionary<string, AdjectiveInfo> _allInfos = new Dictionary<string, AdjectiveInfo>();

	private static Dictionary<AdjectiveType, List<AdjectiveInfo>> _typeInfo = new Dictionary<AdjectiveType, List<AdjectiveInfo>>();

	public AdjectiveType type;

	public bool baseType;

	public bool UIRelevant;

	public bool transferable = true;

	public string codeName;

	public string descriptiveName;

	public IconKey.IconKeyLink _icon;

	public IconKey.IconKeyLink _storedIcon;

	public Sprite icon => _icon.Get32;

	public Sprite storedIcon => _storedIcon.Get32;

	private void OnValidate()
	{
		_icon.Validate();
		_storedIcon.Validate();
	}

	public override void Initialize()
	{
		base.Initialize();
		BrixSingleton<FactoryLoader>.Instance.FactoriesReadySignal.ConnectOnce(Register);
		_icon.Validate();
		_storedIcon.Validate();
	}

	private void Register()
	{
		if (baseType)
		{
			return;
		}
		if (type != AdjectiveType.Adjectif)
		{
			if (!_typeInfo.ContainsKey(type))
			{
				_typeInfo.Add(type, new List<AdjectiveInfo>());
			}
			_typeInfo[type].Add(this);
		}
		if (!string.IsNullOrEmpty(codeName))
		{
			_allInfos[codeName] = this;
		}
	}

	public static AdjectiveInfo GetInfo(string name)
	{
		if (!_allInfos.TryGetValue(name, out var value))
		{
			try
			{
				Factory factory = Factory.GetFactory("Adjectives");
				value = factory.GetComponentsInChildren<AdjectiveInfo>(includeInactive: true).Slinq().FirstOrDefault((AdjectiveInfo x) => x.codeName == name);
			}
			catch
			{
			}
			if (value == null)
			{
				Debug.LogError("AdjectiveInfo " + name + " could not be found");
			}
		}
		return value;
	}

	public static List<AdjectiveInfo> GetInfosFromType(AdjectiveType type)
	{
		return _typeInfo[type];
	}
}
