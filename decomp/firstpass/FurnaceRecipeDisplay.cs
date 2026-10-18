using System.Collections.Generic;
using System.Runtime.InteropServices;
using Brix.Game;
using Brix.Network;
using UnityEngine;
using UnityEngine.Networking;

public class FurnaceRecipeDisplay : NetworkGameComponent
{
	[SerializeField]
	private List<MeshRenderer> blueprints;

	[SerializeField]
	private Material iron;

	[SerializeField]
	private Material glass;

	[SerializeField]
	private Material blueprint;

	[SyncVar(hook = "DisplayBlueprints")]
	protected Vector3 ingots;

	public Vector3 Networkingots
	{
		get
		{
			return ingots;
		}
		[param: In]
		set
		{
			ref Vector3 fieldValue = ref ingots;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				DisplayBlueprints(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 1u);
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		DisplayBlueprints(ingots);
	}

	private void DisplayBlueprints(Vector3 value)
	{
		Networkingots = value;
		if (!Affiliation.IsAllied(User.LocalUser.faction, base.gameObject))
		{
			HideAll();
		}
		else
		{
			SwitchMats(value);
		}
	}

	public void SwitchMats(Vector3 value)
	{
		int num = (int)value.x;
		int num2 = (int)value.y + num;
		int num3 = (int)value.z;
		int i;
		for (i = 0; i < num; i++)
		{
			blueprints[i].sharedMaterial = iron;
			blueprints[i].gameObject.SetActive(value: true);
		}
		for (; i < num2; i++)
		{
			blueprints[i].sharedMaterial = glass;
			blueprints[i].gameObject.SetActive(value: true);
		}
		for (; i < num3; i++)
		{
			blueprints[i].sharedMaterial = blueprint;
			blueprints[i].gameObject.SetActive(value: true);
		}
		for (; i < blueprints.Count; i++)
		{
			blueprints[i].gameObject.SetActive(value: false);
		}
	}

	private void HideAll()
	{
		for (int i = 0; i < blueprints.Count; i++)
		{
			blueprints[i].gameObject.SetActive(value: false);
		}
	}

	public void Set(int ironCount, int glassCount, int orderCount)
	{
		Networkingots = new Vector3(ironCount, glassCount, orderCount);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(ingots);
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
			writer.Write(ingots);
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
			ingots = reader.ReadVector3();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			DisplayBlueprints(reader.ReadVector3());
		}
	}
}
