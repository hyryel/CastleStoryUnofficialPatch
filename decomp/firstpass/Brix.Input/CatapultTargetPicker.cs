using System.Collections.Generic;
using Brix.Game;
using Brix.Game.Components.CatapultSpace;
using Brix.Game.Network;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Input;

public class CatapultTargetPicker : PickerComponent
{
	public class CatapultTargetPlacementOptions : IPickerComponentOptions
	{
		public List<GameObject> catapults = new List<GameObject>();

		public CatapultTargetPlacementOptions(List<GameObject> _catapults)
		{
			catapults.Clear();
			_catapults.Slinq().AddTo(catapults);
		}
	}

	private List<Catapult> catapults = new List<Catapult>();

	public override void OnHoverAny(bool hovering)
	{
		Vector3 point = _voxelRayHit.RayHit.point;
		Vector3 normal = _voxelRayHit.RayHit.normal;
		foreach (Catapult catapult in catapults)
		{
			catapult.Viser(point, normal);
		}
	}

	public override void OnLeaveAny(bool hovering)
	{
		foreach (Catapult catapult in catapults)
		{
			catapult.ArreterDeViser();
		}
	}

	public override void OnFirstActionAny(bool actioning, ClickInfo clickInfo)
	{
		if (clickInfo.actionName != "cam_action1")
		{
			return;
		}
		foreach (Catapult catapult in catapults)
		{
			User.LocalUser.GetComponent<UNetCatapult>().CallCmdSetTarget(catapult.gameObject, catapult.cible, catapult.normale);
		}
		Picking.Instance.ClearPick();
	}

	public void CopyCatapults(List<GameObject> _catapults)
	{
		catapults.Clear();
		(from c in _catapults.Slinq()
			select c.GetComponent<Catapult>()).AddTo(catapults);
	}

	public override void SetOptions(IPickerComponentOptions options)
	{
		if (options is CatapultTargetPlacementOptions)
		{
			CatapultTargetPlacementOptions catapultTargetPlacementOptions = (CatapultTargetPlacementOptions)options;
			CopyCatapults(catapultTargetPlacementOptions.catapults);
		}
	}
}
