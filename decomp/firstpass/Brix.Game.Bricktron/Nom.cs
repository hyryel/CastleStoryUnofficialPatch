using System.Collections.Generic;
using System.Runtime.InteropServices;
using Brix.Engine;
using Brix.Engine.Lifecycle;
using Brix.Game.AI;
using Brix.Game.Names;
using Brix.Network;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Bricktron;

[JsonObject(MemberSerialization.OptIn)]
public class Nom : NetworkGameComponent
{
	private class NomSingletonData
	{
		public HashSet<string> Reserves = new HashSet<string>();
	}

	[SyncVar(hook = "OnNomChanged")]
	private string _nom;

	public static GameSignals.GameSignal<GameObject, string> Changed = GameSignals.CreateStaticSignal<GameObject, string>();

	public static HashSet<string> Reserves => SingletonProvider.Get<NomSingletonData>().Reserves;

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public string nom
	{
		get
		{
			return GetNom();
		}
		set
		{
			SetNom(value);
		}
	}

	public string Network_nom
	{
		get
		{
			return _nom;
		}
		[param: In]
		set
		{
			ref string fieldValue = ref _nom;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				OnNomChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 1u);
		}
	}

	protected override void Start()
	{
		base.Start();
		if (string.IsNullOrEmpty(nom) && NetworkServer.active)
		{
			PremierNom();
		}
		else
		{
			SetNom(_nom);
		}
	}

	public string GetNom()
	{
		return _nom;
	}

	public void SetNom(string nouveauNom)
	{
		GestionnaireDeNom.RenommerExclusif(base.gameObject, base.gameObject.name, nouveauNom);
		Network_nom = nouveauNom;
		base.Name = nouveauNom;
		Reserves.Add(nouveauNom);
		GameSignals.Invoke(Changed, base.gameObject, nouveauNom);
	}

	public void PremierNom()
	{
		if (GetComponent<NameFetcherGO>() != null)
		{
			SetNom(GetComponent<NameFetcherGO>().GetName());
		}
		else
		{
			SetNom(base.gameObject.name);
		}
	}

	private void OnNomChanged(string nouveauNom)
	{
		SetNom(nouveauNom);
		StatsTracker.UnitSpawned(GetComponent<Labor>());
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(_nom);
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
			writer.Write(_nom);
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
			_nom = reader.ReadString();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			OnNomChanged(reader.ReadString());
		}
	}
}
