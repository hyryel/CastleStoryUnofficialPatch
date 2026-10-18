using System.Runtime.InteropServices;
using Brix.Engine;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Beacons;
using Brix.Game.Components.Tunnel;
using Brix.Game.Components.Volumes.Drawers;
using Brix.Network;
using Brix.Utils.Signals;
using UnityEngine;
using UnityEngine.Networking;

public class TunnelController : NetworkGameComponent
{
	[SerializeField]
	private TunnelVolume tunnelVolume;

	[SerializeField]
	private TunnelProject tunnelProject;

	[SerializeField]
	private BoxDrawer.Spawner fullDrawerSpawner;

	[SerializeField]
	private BoxDrawer.Spawner emptyDrawerSpawner;

	[SerializeField]
	private TunnelUI ui;

	public bool ReadyForPicking;

	private bool isInPicker;

	[SyncVar(hook = "Refresh")]
	private int refresh;

	private BoxDrawer fullDrawer => fullDrawerSpawner.Instance;

	private BoxDrawer emptyDrawer => emptyDrawerSpawner.Instance;

	public int Networkrefresh
	{
		get
		{
			return refresh;
		}
		[param: In]
		set
		{
			ref int fieldValue = ref refresh;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				Refresh(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 1u);
		}
	}

	public override void OnRequested()
	{
		fullDrawerSpawner.Spawn();
		emptyDrawerSpawner.Spawn();
		tunnelVolume.gameObject.SetActive(value: false);
	}

	public override void ResetComponent()
	{
		fullDrawerSpawner.Release();
		emptyDrawerSpawner.Release();
	}

	protected override void Start()
	{
		base.Start();
		OnStartAnywhere();
	}

	public override void OnStartAnywhere()
	{
		base.OnStartAnywhere();
		if (!ReadyForPicking)
		{
			ConnectToSignals();
			tunnelVolume.initialPosition = GetComponent<BeaconDriver>().NetworkPosition;
			tunnelVolume.gameObject.SetActive(value: true);
			UpdateVolume();
			ReadyForPicking = true;
			RefreshVisibility();
		}
	}

	public void UpdateVolume()
	{
		tunnelVolume.UpdateVolume();
	}

	private void ConnectToSignals()
	{
		tunnelProject.goalDestroyedSignal.Connect(delegate
		{
			Networkrefresh = refresh + 1;
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
		tunnelVolume.VolumeChanged.Connect(delegate
		{
			Refresh();
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
		UIGameObserver.projects.Selected.Connect(delegate
		{
			RefreshVisibility();
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
		GameSignals.Once(GameSignals.LevelReady, delegate
		{
			Refresh();
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
	}

	private void Refresh(int notUsed = 0)
	{
		fullDrawer.Redraw(tunnelVolume.Volume);
		emptyDrawer.Redraw(tunnelVolume.Volume);
	}

	private void RefreshVisibility()
	{
		Project currentSelected = UIGameObserver.projects.CurrentSelected;
		ui.Show(currentSelected == tunnelProject || isInPicker);
	}

	public void OnSetInPicker()
	{
		tunnelProject.faction = User.LocalUser.faction;
		isInPicker = true;
		RefreshVisibility();
	}

	public void ResetToCenter()
	{
		tunnelVolume.initialPosition = base.gameObject.transform.position;
		tunnelVolume.ResetPositionAndParity(XYZ.FromVector3(base.gameObject.transform.position));
		Refresh();
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)refresh);
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
			writer.WritePackedUInt32((uint)refresh);
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
			refresh = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			Refresh((int)reader.ReadPackedUInt32());
		}
	}
}
