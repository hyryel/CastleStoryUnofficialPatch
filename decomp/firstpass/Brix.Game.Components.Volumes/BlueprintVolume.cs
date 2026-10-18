using Brix.External.Signals;
using Brix.Game.Components.MeshSwappers;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components.Volumes;

[RequireComponent(typeof(Blueprint))]
public class BlueprintVolume : Volume
{
	public Signal VolumeChanged = new Signal();

	public override int SubConfiguration => Blueprint.IsAnti ? 1 : 0;

	public override VolumePropertyRequest obstacles => GetCases(VProperty.fantome);

	public override void OnStartAnywhere()
	{
		Blueprint.OnStateChangedSignal.Connect(BlueprintStateChange);
		if (!base.Registered && Blueprint.State.HasFlag(PlacementFlags.placed))
		{
			Register();
		}
	}

	private void BlueprintStateChange(Blueprint blueprint, PlacementFlags previousFlags, PlacementFlags currentFlags)
	{
		if (((previousFlags ^ currentFlags) & PlacementFlags.placed) != PlacementFlags.none)
		{
			if (currentFlags.HasFlag(PlacementFlags.placed))
			{
				Register();
			}
			else
			{
				Unregister();
			}
		}
		if (currentFlags.HasFlag(PlacementFlags.placed) && ((previousFlags ^ currentFlags) & PlacementFlags.anti) != PlacementFlags.none)
		{
			Register();
		}
	}

	public override ValidationResult Validate()
	{
		ValidationResult result = base.Validate();
		GetComponent<IBlueprintMeshSwapper>()?.RecalculateMesh();
		VolumeChanged.Invoke();
		return result;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}
}
