using Brix.Game.Components;
using Brix.Lua;
using MoonSharp.Interpreter;
using UnityEngine;

[RequireComponent(typeof(PropertyDrive))]
public class PropertyDriveByJournee : UpdatableGameComponent<PropertyDriveByJournee>
{
	public Journee journee;

	public PropertyDrive propertyDrive;

	protected override float SlowUpdateRate => 0.2f;

	public override void Initialize()
	{
		base.Initialize();
		UpdateAtmosphere();
	}

	protected override void BrixSlowUpdate()
	{
		base.BrixSlowUpdate();
		UpdateAtmosphere();
	}

	public void UpdateAtmosphere()
	{
		propertyDrive = propertyDrive ?? base.gameObject.GetComponent<PropertyDrive>();
		if (propertyDrive != null)
		{
			propertyDrive.Traverse = journee.Heure / 24f;
			propertyDrive.traverseDriver = journee;
			propertyDrive.Apply();
		}
	}

	[LuaDrivable("UpdateAtmosphere")]
	protected static DynValue lua_UpdateAtmosphere(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		PropertyDriveByJournee propertyDriveByJournee = args.AsComponent<PropertyDriveByJournee>(0, "UpdateAtmosphere");
		propertyDriveByJournee.UpdateAtmosphere();
		return DynValue.Void;
	}
}
