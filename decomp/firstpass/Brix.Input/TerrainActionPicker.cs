using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.Components;
using Brix.Game.Components.CatapultSpace;
using Brix.Game.Network;
using Brix.Lifecycle.Pooling;
using Brix.UI.GameObserver;
using Brix.UI.GameSelector;
using Brix.Utils;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;

namespace Brix.Input;

internal class TerrainActionPicker : PickerComponent
{
	private Signal<GameObject> removeTerrainHighlight = new Signal<GameObject>();

	public override void OnEnterTerrain(Vector3? hoveredVoxel)
	{
		if (!PickingUtility.isInViewMode() && !PickingUtility.Player.GetButton("cam_action1"))
		{
			NatureHoverTracker.TreeTracker.HandleHover(_voxelRayHit);
			NatureHoverTracker.BoulderTracker.HandleHover(_voxelRayHit);
		}
	}

	public override void OnHoverTerrain(Vector3? hoveredVoxel)
	{
		removeTerrainHighlight.Invoke(null);
		if (UIGameObserver.bricktrons.CheckSelection().Slinq().Any() && PickingUtility.isAlternateUIAction())
		{
			Highlighter.highlight.BlockAndTerrain(XYZ.FromVector3(hoveredVoxel.Value), Color.red, removeTerrainHighlight);
		}
	}

	public override void OnLeaveTerrain(Vector3? hoveredVoxel)
	{
		removeTerrainHighlight.Invoke(null);
	}

	public override void OnLastActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
		if (!(clickInfo.actionName == "cam_action2") || PickingUtility.isInViewMode() || (clickInfo.clickType != ClickType.shortClick && clickInfo.clickType != ClickType.doubleClick))
		{
			return;
		}
		CollectionPool.Request<GameObject>(out List<GameObject> list);
		UIGameSelector.GetSelectedOfType(ref list, SelectionType.Unit);
		bool flag = false;
		if (list.Slinq().Any())
		{
			Vector3 target = _voxelRayHit.HitVector;
			NatureData natureData = PickingUtility.FindHitNature(_voxelRayHit);
			XYZ xYZ = PickingUtility.FindHitTree(_voxelRayHit);
			if (natureData.IsValid)
			{
				target = natureData.RootPos.ToVector3();
			}
			else if (_voxelRayHit.HitLayer == UnityLayer.Nature && xYZ != XYZ.zero)
			{
				target = xYZ.ToVector3();
			}
			DirectOrder(list, target);
			flag = true;
		}
		list.Dispose();
		CollectionPool.Request<GameObject>(out List<GameObject> list2);
		UIGameSelector.GetSelectedOfType(ref list2, SelectionType.Siege);
		if (list2.Slinq().Any())
		{
			Slinq<Catapult, NotNullContext<Catapult, SelectContext<Catapult, GameObject, PredicateContext<GameObject, IListContext<GameObject>>>>> slinq = (from s in list2.Slinq()
				where !s.IsNullOrReleased()
				select s.GetComponent<Catapult>()).NotNull();
			foreach (Catapult item in slinq.NoLeak())
			{
				item.Viser(_voxelRayHit.RayHit.point, _voxelRayHit.RayHit.normal);
				User.LocalUser.GetComponent<UNetCatapult>().CallCmdSetTarget(item.gameObject, item.cible, item.normale);
				flag = true;
			}
			list2.Dispose();
		}
		if (flag)
		{
			ObjectPoolSingleton.Request_Unsafe(Gameplay.ClickConfirm, _voxelRayHit.RayHit.point, Quaternion.identity);
		}
	}

	protected override void OnStandByChange()
	{
		NatureHoverTracker.ClearAllTrackers();
	}

	protected override void OnActiveChange()
	{
		NatureHoverTracker.ClearAllTrackers();
	}

	public static void SimulateDirectOrder(Vector3 target)
	{
		CollectionPool.Request<GameObject>(out List<GameObject> list);
		UIGameSelector.GetSelectedOfType(ref list, SelectionType.Unit);
		PickerComponent.OrderGiver.DirectOrder(list, target);
	}
}
