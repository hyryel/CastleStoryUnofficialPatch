using Brix.Game;
using UnityEngine;

namespace Brix.Input;

internal class AlphabetPlacement : UniversalGrabber
{
	private static LayerBundle terrain = new LayerBundle(10);

	private bool TryRedirectToTerrain()
	{
		return _voxelRayHit.PlaceAtRaycast(Camera.main.ScreenPointToRay(PickingUtility.Player.controllers.Mouse.screenPosition), Picking.Instance.MaxDistance, terrain);
	}

	public override void OnHoverAny(bool hovering)
	{
		if (_voxelRayHit.HitLayer == UnityLayer.Terrain || TryRedirectToTerrain())
		{
			base.OnHoverAny(hovering);
		}
	}

	public override void OnActionAny(bool actioning, ClickInfo clickInfo)
	{
		if (_voxelRayHit.HitLayer == UnityLayer.Terrain || TryRedirectToTerrain())
		{
			base.OnActionAny(actioning, clickInfo);
		}
	}
}
