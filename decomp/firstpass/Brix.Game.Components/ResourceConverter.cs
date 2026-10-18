using System;
using System.Runtime.InteropServices;
using Brix.Audio;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Game.Network;
using Brix.Game.Semantique;
using Brix.Network;
using Newtonsoft.Json;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components;

[RequireComponent(typeof(Recepteur))]
public class ResourceConverter : NetworkGameComponent
{
	public delegate void WipeAndFillDelegate();

	public abstract class ConversionRule
	{
		public Type InputResourceType;

		public Type OutputResourceType;

		public bool AllowAutoConvert;

		public string ConversionSound;

		public abstract void WipeAndFill(Recepteur r);
	}

	public class ConversionRule<TInput, TOutput> : ConversionRule where TInput : Ressource where TOutput : Ressource, new()
	{
		public ConversionRule(string soundEvent, bool allowAutoConvert = true)
		{
			InputResourceType = typeof(TInput);
			OutputResourceType = typeof(TOutput);
			AllowAutoConvert = allowAutoConvert;
			ConversionSound = soundEvent;
		}

		public override void WipeAndFill(Recepteur r)
		{
			Neo.Relais(delegate
			{
				r.WipeAndFill<TOutput>();
			}, "WipeAndFill");
		}
	}

	[JsonProperty]
	[SyncVar]
	[SerializeField]
	private bool _autoConvert = true;

	public Signal ConvertedResources = new Signal();

	public readonly ConversionRule[] _conversionRules = new ConversionRule[1]
	{
		new ConversionRule<Stones, StoneBlock>("Stockpiles_Brick_Appear")
	};

	private static int kEventEventWipeAndFill;

	public bool AutoConvert
	{
		get
		{
			return _autoConvert;
		}
		set
		{
			if (_autoConvert != value)
			{
				User.LocalUser.GetComponent<UNetRecepteurCmd>().CallCmdSetAutoConvert(base.gameObject, value);
			}
		}
	}

	public ConversionRule CurrentAutoConvertRule
	{
		get
		{
			ConversionRule result = null;
			if (_autoConvert && base.recepteur.IsFull() && !base.recepteur.IsEmpty())
			{
				result = (from rule in _conversionRules.Slinq()
					where rule.AllowAutoConvert
					select rule).Where((ConversionRule rule, Recepteur rec) => rec.ContentDescription.Has(rule.InputResourceType), base.recepteur).FirstOrDefault();
			}
			return result;
		}
	}

	public ConversionRule CurrentConvertRule
	{
		get
		{
			ConversionRule result = null;
			if (base.recepteur.IsFull() && !base.recepteur.IsEmpty())
			{
				result = _conversionRules.Slinq().Where((ConversionRule rule, Recepteur rec) => rec.ContentDescription.Has(rule.InputResourceType), base.recepteur).FirstOrDefault();
			}
			return result;
		}
	}

	public bool Network_autoConvert
	{
		get
		{
			return _autoConvert;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref _autoConvert, 1u);
		}
	}

	[SyncEvent]
	public event WipeAndFillDelegate EventWipeAndFill;

	public string GetConversionSound()
	{
		ConversionRule conversionRule = null;
		string result = string.Empty;
		if (!base.recepteur.IsEmpty())
		{
			conversionRule = _conversionRules.Slinq().Where((ConversionRule rule, Recepteur rec) => rec.ContentDescription.Has(rule.OutputResourceType), base.recepteur).FirstOrDefault();
		}
		if (conversionRule != null)
		{
			result = conversionRule.ConversionSound;
		}
		return result;
	}

	public override void Initialize()
	{
		base.Initialize();
		EventWipeAndFill += OnEventWipeAndFill;
	}

	private void OnEventWipeAndFill()
	{
		SoundEngine.Play(GetConversionSound(), base.gameObject);
	}

	public override void OnStartServerOrHost()
	{
		base.OnStartServerOrHost();
		base.recepteur.OnReceive.Connect(OnReceive);
	}

	private void OnReceive(GameObject go)
	{
		ConversionRule currentAutoConvertRule = CurrentAutoConvertRule;
		if (currentAutoConvertRule != null)
		{
			CallEventWipeAndFill();
			currentAutoConvertRule.WipeAndFill(base.recepteur);
			ConvertedResources.Invoke();
		}
	}

	public bool CurrentCanAutoConvert()
	{
		return CurrentAutoConvertRule != null;
	}

	public void UpdateAutoConvert(bool value)
	{
		Network_autoConvert = value;
	}

	public void ManualConversion()
	{
		User.LocalUser.GetComponent<UNetRecepteurCmd>().CallCmdManualConversion(base.gameObject);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeSyncEventEventWipeAndFill(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("Event EventWipeAndFill called on server.");
		}
		else if (((ResourceConverter)obj).EventWipeAndFill != null)
		{
			((ResourceConverter)obj).EventWipeAndFill();
		}
	}

	public void CallEventWipeAndFill()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Event EventWipeAndFill called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)7);
		networkWriter.WritePackedUInt32((uint)kEventEventWipeAndFill);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendEventInternal(networkWriter, 0, "EventWipeAndFill");
	}

	static ResourceConverter()
	{
		kEventEventWipeAndFill = -1884992703;
		NetworkBehaviour.RegisterEventDelegate(typeof(ResourceConverter), kEventEventWipeAndFill, InvokeSyncEventEventWipeAndFill);
		NetworkCRC.RegisterBehaviour("ResourceConverter", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(_autoConvert);
			return true;
		}
		bool flag2 = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(_autoConvert);
		}
		if (!flag2)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
		if (initialState)
		{
			_autoConvert = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			_autoConvert = reader.ReadBoolean();
		}
	}
}
