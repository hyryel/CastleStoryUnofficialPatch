using System.Collections;
using System.Runtime.InteropServices;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Legacy;
using Brix.Utils.Signals;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Network;

public class UNetJourneeSync : NetworkBehaviour
{
	[SyncVar(hook = "HeureFieldChanged")]
	public float NetworkHeure;

	[SyncVar(hook = "PauseFieldChanged")]
	public bool NetworkPause;

	[SyncVar(hook = "AtmosphereColorChanged")]
	public Color AtmosphereColor;

	[SyncVar(hook = "AtmosphereColorSet")]
	public bool _atmosphereColorSet;

	public static UNetJourneeSync instance;

	private Signal Destroyed = new Signal();

	public float NetworkNetworkHeure
	{
		get
		{
			return NetworkHeure;
		}
		[param: In]
		set
		{
			ref float networkHeure = ref NetworkHeure;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				HeureFieldChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref networkHeure, 1u);
		}
	}

	public bool NetworkNetworkPause
	{
		get
		{
			return NetworkPause;
		}
		[param: In]
		set
		{
			ref bool networkPause = ref NetworkPause;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				PauseFieldChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref networkPause, 2u);
		}
	}

	public Color NetworkAtmosphereColor
	{
		get
		{
			return AtmosphereColor;
		}
		[param: In]
		set
		{
			ref Color atmosphereColor = ref AtmosphereColor;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				AtmosphereColorChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref atmosphereColor, 4u);
		}
	}

	public bool Network_atmosphereColorSet
	{
		get
		{
			return _atmosphereColorSet;
		}
		[param: In]
		set
		{
			ref bool atmosphereColorSet = ref _atmosphereColorSet;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				AtmosphereColorSet(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref atmosphereColorSet, 8u);
		}
	}

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		if (NetworkServer.active)
		{
			StartCoroutine(UpdateNetworkHeure());
			GameSignals.Connect(Journee.OnSetIsPaused, delegate
			{
				NetworkNetworkPause = Journee.CurrentPaused;
			}).DisconnectWhen(Destroyed);
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		if (_atmosphereColorSet)
		{
			MaterialToGlobalSettings.instance.OverrideColor(AtmosphereColor);
		}
	}

	private void OnDestroy()
	{
		Destroyed.Invoke();
	}

	public void SetNetworkAtmosphereColor(Color color)
	{
		Network_atmosphereColorSet = true;
		NetworkAtmosphereColor = color;
	}

	private void PauseFieldChanged(bool value)
	{
		NetworkNetworkPause = value;
		if (!NetworkServer.active && (bool)Journee.instance)
		{
			Journee.instance.IsPaused = value;
		}
	}

	private void HeureFieldChanged(float value)
	{
		NetworkNetworkHeure = value;
		if (!NetworkServer.active && (bool)Journee.instance)
		{
			Journee.instance.SetTime(value);
		}
	}

	private void AtmosphereColorChanged(Color color)
	{
		NetworkAtmosphereColor = color;
		MaterialToGlobalSettings.instance.OverrideColor(color);
	}

	public void AtmosphereColorSet(bool value)
	{
		MaterialToGlobalSettings.instance.OverrideColor(AtmosphereColor);
	}

	private IEnumerator UpdateNetworkHeure()
	{
		while (NetworkServer.active)
		{
			NetworkNetworkHeure = Journee.CurrentTime;
			yield return new WaitForSeconds(2f);
		}
	}

	public void ForceUpdateHeure()
	{
		NetworkNetworkHeure = Journee.CurrentTime;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(NetworkHeure);
			writer.Write(NetworkPause);
			writer.Write(AtmosphereColor);
			writer.Write(_atmosphereColorSet);
			return true;
		}
		bool flag = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(NetworkHeure);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(NetworkPause);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(AtmosphereColor);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(_atmosphereColorSet);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			NetworkHeure = reader.ReadSingle();
			NetworkPause = reader.ReadBoolean();
			AtmosphereColor = reader.ReadColor();
			_atmosphereColorSet = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			HeureFieldChanged(reader.ReadSingle());
		}
		if ((num & 2) != 0)
		{
			PauseFieldChanged(reader.ReadBoolean());
		}
		if ((num & 4) != 0)
		{
			AtmosphereColorChanged(reader.ReadColor());
		}
		if ((num & 8) != 0)
		{
			AtmosphereColorSet(reader.ReadBoolean());
		}
	}
}
