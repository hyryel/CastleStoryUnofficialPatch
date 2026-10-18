using Brix.Engine;
using Brix.Engine.Nature;
using Brix.Game.Components;
using Brix.Input.ContextualOrders;
using Brix.Legacy;
using Rewired;
using UnityEngine;

namespace Brix.Input;

public static class PickingUtility
{
	private static Player _player;

	public static Player Player
	{
		get
		{
			if (_player == null && ReInput.isReady)
			{
				_player = ReInput.players.GetPlayer("local");
			}
			return _player;
		}
	}

	public static T FindComponentInParents<T>(GameObject g, bool layerwise = true)
	{
		while (g.transform.parent != null && (!layerwise || g.transform.parent.gameObject.layer == g.layer) && g.GetComponent(typeof(T)) == null)
		{
			g = g.transform.parent.gameObject;
		}
		return (T)(object)g.GetComponent(typeof(T));
	}

	public static GameObject GetLayerRootParent(GameObject g)
	{
		while (g.transform.parent != null && g.transform.parent.gameObject.layer == g.layer)
		{
			g = g.transform.parent.gameObject;
		}
		return g;
	}

	public static GameObject FindSignificantParent(GameObject go)
	{
		GameObject result = go;
		SignificantParent significantParent = FindComponentInParents<SignificantParent>(go, layerwise: false);
		if (significantParent != null)
		{
			result = significantParent.Get;
		}
		else
		{
			GameComponent gameComponent = FindComponentInParents<GameComponent>(go);
			if (!gameComponent.IsNullOrReleased())
			{
				result = gameComponent.gameObject;
			}
		}
		return result;
	}

	public static Vector3 GetHorizontalDragPoint(Vector3 start, Ray ray)
	{
		if (new Plane(Vector3.up, start).Raycast(ray, out var enter))
		{
			return ray.GetPoint(enter);
		}
		return Vector3.zero;
	}

	public static Vector3 GetVerticalDragPoint(Vector3 start, Ray ray)
	{
		if (new Plane(Vector3.Cross(Camera.main.transform.right, Vector3.up), start).Raycast(ray, out var enter))
		{
			return ray.GetPoint(enter);
		}
		return Vector3.zero;
	}

	public static ActionType GetActionType()
	{
		if (Player.GetButton("cam_mouse_sec"))
		{
			return ActionType.Secondary;
		}
		if (Player.GetButton("cam_mouse_alt"))
		{
			return ActionType.Ternary;
		}
		return ActionType.Primary;
	}

	public static bool isInViewMode()
	{
		return Player.GetButton("cam_mouse_movement") || Player.GetButton("cam_mouse_rotate");
	}

	public static bool isInQueueMode()
	{
		return false;
	}

	public static bool isInToggleSelectMode()
	{
		return Player.GetButton("cam_mouse_alt");
	}

	public static bool isInBatchSelectMode(ClickType clickType)
	{
		return Player.GetButton("cam_mouse_sec") || clickType == ClickType.doubleClick;
	}

	public static bool isAlternateUIAction()
	{
		return Player.GetButton("cam_mouse_sec");
	}

	public static Vector3 SubjectiveRaycast(GameObject o, Ray ray)
	{
		Plane plane = new Plane(Vector3.up, o.transform.position);
		float enter = 0f;
		return (!plane.Raycast(ray, out enter)) ? o.transform.forward : ray.GetPoint(enter);
	}

	public static XYZ FindHitTree(VoxelRaycastHit vrh)
	{
		if (!vrh.HitGameObject || !vrh.HitGameObject.name.Contains("Tree"))
		{
			return XYZ.zero;
		}
		XYZ result = XYZ.zero;
		for (int i = -2; i <= 2; i++)
		{
			XYZ result2;
			for (int j = -2; j <= 2 && Architecte.VoxelDessus(XZ.FromXYZ(vrh.VoxelPosition) + new XZ(i, j), out result2); j++)
			{
				if (TreeRegistry.Instance.HasTree(result2) && (result2.ToVector3() - vrh.WorldHitVector).sqrMagnitude < (result.ToVector3() - vrh.WorldHitVector).sqrMagnitude)
				{
					result = result2;
				}
			}
		}
		return result;
	}

	public static NatureData FindHitNature(VoxelRaycastHit vrh)
	{
		return NatureRegistry.NatureAt(vrh.VoxelPosition);
	}
}
