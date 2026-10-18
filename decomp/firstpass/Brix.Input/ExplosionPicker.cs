using Brix.Engine;
using Brix.Game;
using UnityEngine;

namespace Brix.Input;

public class ExplosionPicker : PickerComponent
{
	public float force = 4f;

	public override void OnFirstActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1")
		{
			Architecte.ExplosionSimple(Faction.DefaultAllied, clickedVoxel.Value, force);
		}
	}
}
