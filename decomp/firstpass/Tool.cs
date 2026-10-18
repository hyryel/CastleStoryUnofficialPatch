using System;
using System.Runtime.InteropServices;
using Brix.External.Factories;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Semantique;
using Brix.Legacy;
using Brix.Lifecycle.Pooling;
using Brix.Network;
using Brix.Utils.Factories;
using Newtonsoft.Json;
using Smooth.Algebraics;
using UnityEngine;
using UnityEngine.Networking;

[JsonObject(MemberSerialization.OptIn)]
[RequireComponent(typeof(NetworkIdentity))]
public class Tool : NetworkGameComponent, PieMenuable
{
	public enum OnDropBehaviours
	{
		Release,
		Transfer,
		Drop
	}

	[Flags]
	public enum Archetype
	{
		None = 0,
		Hammer = 1,
		Axe = 2,
		Pick = 4,
		MeleeWeapon = 8,
		ProjectileWeapon = 0x10,
		Hat = 0x20,
		Defence = 0x40
	}

	[EnumFlag]
	public Archetype archetype;

	[JsonProperty]
	public GameObject CurrentUser;

	public OnDropBehaviours OnDropBehaviour;

	[SyncVar]
	[SerializeField]
	[JsonProperty]
	public bool Magical;

	[SyncVar]
	[SerializeField]
	[JsonProperty]
	public bool Aesthetic;

	public bool NetworkMagical
	{
		get
		{
			return Magical;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref Magical, 1u);
		}
	}

	public bool NetworkAesthetic
	{
		get
		{
			return Aesthetic;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref Aesthetic, 2u);
		}
	}

	public override void OnStartServerOrHost()
	{
		base.OnStartServerOrHost();
		if (!CurrentUser.IsNullOrReleased())
		{
			CurrentUser.GetComponent<Toolbag>().EquipTool(base.gameObject);
		}
	}

	public void OnDrop()
	{
		if ((NetworkServer.active && OnDropBehaviour == OnDropBehaviours.Release) || Magical)
		{
			ObjectPoolSingleton.Release(base.gameObject);
		}
		else if (OnDropBehaviour == OnDropBehaviours.Drop)
		{
			Transform user = ((!CurrentUser.IsNullOrReleased()) ? CurrentUser.transform : base.transform);
			DropFrom(user);
		}
		CurrentUser = null;
	}

	private void DropFrom(Transform user)
	{
		base.transform.position = user.position + user.forward + user.up * 1.5f;
		Rigidbody component = GetComponent<Rigidbody>();
		if (component != null)
		{
			component.AddForce(user.up + user.forward * 2f, ForceMode.Impulse);
		}
	}

	public OrderPackage<Labor> GetContextualAction(Labor labor)
	{
		return Order.Package.OfGoIntTuple(Order.Transfer.GoPickUp, Tuple.Create(base.gameObject, 1));
	}

	public override bool CheckTag(string tag)
	{
		return tag == "Tool";
	}

	public static bool IsEquipedBy(GameObject tool, GameObject potentialUser)
	{
		Tool component = tool.GetComponent<Tool>();
		if (component == null)
		{
			return false;
		}
		return component.CurrentUser == potentialUser;
	}

	public static float EfficiencyOf(IGameComponent tool)
	{
		Apparence apparence = tool.apparence;
		if (apparence == null)
		{
			return -99999f;
		}
		if (!apparence.Description.Has<Accessoire>())
		{
			return -99999f;
		}
		return apparence.Description.Value<Accessoire>();
	}

	public static bool IsCompatible(GameObject underTest, GameObject reference)
	{
		return IsCompatible(underTest, Apparence.CategorieOutilOf(reference));
	}

	public static bool IsCompatible(GameObject underTest, CategorieOutil reference)
	{
		return underTest.WithComponent((Tool t, CategorieOutil outil) => t.IsCompatibleWith(outil), _default: false, reference);
	}

	public bool IsCompatibleWith(CategorieOutil type)
	{
		if (Aesthetic || type == null)
		{
			return true;
		}
		return Apparence.CategorieOutilIs(base.gameObject, type);
	}

	public bool IsWeapon()
	{
		return archetype.HasFlag(Archetype.MeleeWeapon) || archetype.HasFlag(Archetype.ProjectileWeapon);
	}

	public static Slot Slot(Factory.AssetKey key)
	{
		return Factory.Peek(key).WithComponent((Grip t) => t.PropSlot, global::Slot.Invalid);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(Magical);
			writer.Write(Aesthetic);
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
			writer.Write(Magical);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(Aesthetic);
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
			Magical = reader.ReadBoolean();
			Aesthetic = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			Magical = reader.ReadBoolean();
		}
		if ((num & 2) != 0)
		{
			Aesthetic = reader.ReadBoolean();
		}
	}
}
