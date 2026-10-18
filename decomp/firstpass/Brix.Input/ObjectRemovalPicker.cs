using Brix.Audio;
using Brix.Game.Components;
using UnityEngine;

namespace Brix.Input;

public class ObjectRemovalPicker : RemoverComponentV2
{
	public override void OnFirstActionGameObject(GameObject clickedGameObject, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1")
		{
			hoveredMemory = null;
			SoundEngine.Play("Brick_Cancel");
			Object.Destroy(clickedGameObject);
		}
	}

	public override void OnFirstActionBlueprint(Blueprint clickedBlueprint, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1")
		{
			hoveredMemory = null;
			SoundEngine.Play("Brick_Cancel");
			Object.Destroy(clickedBlueprint.gameObject);
		}
	}

	public override void OnHoverAny(bool hovering)
	{
	}

	public override void OnFirstActionAny(bool actioning, ClickInfo clickInfo)
	{
	}

	public override void OnLastActionAny(bool actioning, ClickInfo clickInfo)
	{
	}

	public override void OnFirstActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
	}
}
