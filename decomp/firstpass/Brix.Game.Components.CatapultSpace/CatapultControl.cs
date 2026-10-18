using System.Runtime.InteropServices;
using Brix.Engine;
using Brix.Network;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components.CatapultSpace;

public class CatapultControl : NetworkGameComponent
{
	[JsonProperty]
	public CatapultBody body;

	[JsonProperty]
	[SyncVar]
	public Vector3 cible;

	[SyncVar]
	[JsonProperty]
	public Vector3 cibleNormale;

	[JsonProperty]
	public Vector3 apercu;

	[JsonProperty]
	public Vector3 apercuNormale;

	[SyncVar]
	[JsonProperty]
	public bool feu;

	[JsonProperty]
	[SyncVar]
	public bool crinquee;

	[SyncVar]
	[JsonProperty]
	public bool animee;

	[SyncVar(hook = "bouletChanged")]
	[JsonProperty]
	public GameObject boulet;

	[JsonProperty]
	[SyncVar]
	public GameObject ouvrier;

	[JsonProperty]
	public bool initialisee;

	public float inaccuracy;

	private NetworkInstanceId ___bouletNetId;

	private NetworkInstanceId ___ouvrierNetId;

	public Vector3 Networkcible
	{
		get
		{
			return cible;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref cible, 1u);
		}
	}

	public Vector3 NetworkcibleNormale
	{
		get
		{
			return cibleNormale;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref cibleNormale, 2u);
		}
	}

	public bool Networkfeu
	{
		get
		{
			return feu;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref feu, 4u);
		}
	}

	public bool Networkcrinquee
	{
		get
		{
			return crinquee;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref crinquee, 8u);
		}
	}

	public bool Networkanimee
	{
		get
		{
			return animee;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref animee, 16u);
		}
	}

	public GameObject Networkboulet
	{
		get
		{
			return boulet;
		}
		[param: In]
		set
		{
			ref GameObject gameObjectField = ref boulet;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				bouletChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVarGameObject(value, ref gameObjectField, 32u, ref ___bouletNetId);
		}
	}

	public GameObject Networkouvrier
	{
		get
		{
			return ouvrier;
		}
		[param: In]
		set
		{
			SetSyncVarGameObject(value, ref ouvrier, 64u, ref ___ouvrierNetId);
		}
	}

	private void bouletChanged(GameObject value)
	{
		Networkboulet = value;
		if (!NetworkServer.active && value != null)
		{
			body.HandleMessage(new CatapultState.MessageArmer());
		}
	}

	public void Armer(GameObject newAmmo)
	{
		if (newAmmo.IsNullOrReleased() && !boulet.IsNullOrReleased())
		{
			body.HandleMessage(new CatapultState.MessageDesarmer());
			Networkboulet = null;
			Networkfeu = false;
		}
		else if (!newAmmo.IsNullOrReleased() && boulet.IsNullOrReleased())
		{
			Networkboulet = newAmmo;
			Networkfeu = false;
			body.HandleMessage(new CatapultState.MessageArmer());
		}
	}

	public override void OnStartAnywhere()
	{
		GameSignals.LateConnectOnce(GameSignals.LevelReady, delegate
		{
			if (boulet != null && boulet.transform.parent == body.modeleProjectile.transform)
			{
				boulet.transform.position = body.modeleProjectile.transform.position;
				boulet.transform.rotation = body.modeleProjectile.transform.rotation;
			}
		});
	}

	protected override void OnDestroy()
	{
		Piloter(null);
	}

	public void Piloter(GameObject _ouvrier)
	{
		body.HandleMessage(new CatapultState.MessagePiloter());
	}

	public void Apercu(Vector3 apercu, Vector3 normale)
	{
		if (!(this.apercu == apercu))
		{
			this.apercu = apercu;
			apercuNormale = normale;
			GetComponent<CatapultBody>().Selected();
			body.HandleMessage(new CatapultState.MessageApercu());
		}
	}

	public void CesserApercu()
	{
		if (!(apercu == Vector3.zero))
		{
			apercu = Vector3.zero;
			GetComponent<CatapultBody>().Unselected();
			body.HandleMessage(new CatapultState.MessageCesserApercu());
		}
	}

	public bool OperatorOnSpot()
	{
		return true;
	}

	public bool PeutDeclancher()
	{
		return cible != Vector3.zero && body.forceVoulue > 0f;
	}

	public void Declancher()
	{
		if (!feu)
		{
			Networkfeu = true;
			body.HandleMessage(new CatapultState.MessageDeclancher());
		}
	}

	public void Deprogrammer()
	{
		if (initialisee)
		{
			initialisee = false;
			apercu = Vector3.zero;
			Networkcible = Vector3.zero;
			Networkfeu = false;
			body.HandleMessage(new CatapultState.MessageDeprogrammer());
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(cible);
			writer.Write(cibleNormale);
			writer.Write(feu);
			writer.Write(crinquee);
			writer.Write(animee);
			writer.Write(boulet);
			writer.Write(ouvrier);
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
			writer.Write(cible);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(cibleNormale);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(feu);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(crinquee);
		}
		if ((base.syncVarDirtyBits & 0x10) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(animee);
		}
		if ((base.syncVarDirtyBits & 0x20) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(boulet);
		}
		if ((base.syncVarDirtyBits & 0x40) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(ouvrier);
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
			cible = reader.ReadVector3();
			cibleNormale = reader.ReadVector3();
			feu = reader.ReadBoolean();
			crinquee = reader.ReadBoolean();
			animee = reader.ReadBoolean();
			___bouletNetId = reader.ReadNetworkId();
			___ouvrierNetId = reader.ReadNetworkId();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			cible = reader.ReadVector3();
		}
		if ((num & 2) != 0)
		{
			cibleNormale = reader.ReadVector3();
		}
		if ((num & 4) != 0)
		{
			feu = reader.ReadBoolean();
		}
		if ((num & 8) != 0)
		{
			crinquee = reader.ReadBoolean();
		}
		if ((num & 0x10) != 0)
		{
			animee = reader.ReadBoolean();
		}
		if ((num & 0x20) != 0)
		{
			bouletChanged(reader.ReadGameObject());
		}
		if ((num & 0x40) != 0)
		{
			ouvrier = reader.ReadGameObject();
		}
	}

	public override void PreStartClient()
	{
		if (!___bouletNetId.IsEmpty())
		{
			Networkboulet = ClientScene.FindLocalObject(___bouletNetId);
		}
		if (!___ouvrierNetId.IsEmpty())
		{
			Networkouvrier = ClientScene.FindLocalObject(___ouvrierNetId);
		}
	}
}
