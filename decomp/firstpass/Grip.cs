using Brix.Game.Components;
using Brix.Lua;
using Brix.Utils;
using Brix.Utils.Factories;
using Brix.Utils.Unity;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.Networking;

[ExecuteInEditMode]
public class Grip : UpdatableGameComponent<Grip>
{
	public TakeType TakeType;

	public Slot PropSlot;

	[LuaDrivable("gripCollapseLimb")]
	public bool collapseLimb;

	[Matrix4x4PropertyDrawer(true, true, true)]
	public Matrix4x4 gripMatrix = Matrix4x4.identity;

	protected override bool HasUpdate => !NetworkServer.active;

	[ContextMenu("Align to Socket")]
	public void TryAlignToSocket()
	{
		GripSocket componentInParent = GetComponentInParent<GripSocket>();
		if ((bool)componentInParent)
		{
			AlignToSocket(componentInParent);
		}
	}

	public static Slot SlotOf(GameObject go)
	{
		return go.WithComponent((Grip s) => s.PropSlot, Slot.Invalid);
	}

	protected override void Start()
	{
		base.Start();
		TryAlignToSocket();
	}

	private void OnValidate()
	{
		TryAlignToSocket();
	}

	protected override void BrixUpdate()
	{
		TryAlignToSocket();
	}

	public void AlignToSocket(GripSocket socket)
	{
		socket.Collapse(collapseLimb);
		Matrix4x4 matrix = socket.socketMatrix * gripMatrix.inverse;
		Matrix4x4Utils.SetTransformFromMatrix(base.transform, ref matrix);
	}

	[LuaDrivable("SetGripMatrix")]
	private static DynValue lua_SetGripMatrix(ScriptExecutionContext context, CallbackArguments args)
	{
		Grip grip = args.AsComponent<Grip>(0, "SetGripMatrix");
		grip.gripMatrix = args.AsUserData<Matrix4x4>(1, "SetGripMatrix");
		return DynValue.Void;
	}
}
