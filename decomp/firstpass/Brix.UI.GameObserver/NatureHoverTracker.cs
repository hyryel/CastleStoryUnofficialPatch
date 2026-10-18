using Brix.External.Signals;
using Brix.Input;
using UnityEngine;

namespace Brix.UI.GameObserver;

public abstract class NatureHoverTracker
{
	public static NatureHoverTracker TreeTracker = new TreeHoverTracker();

	public static NatureHoverTracker PlantTracker = new PlantHoverTracker();

	public static NatureHoverTracker BoulderTracker = new BoulderHoverTracker();

	public Signal<Vector3> HoveredAtPosition = new Signal<Vector3>();

	public Signal<Vector3> UnhoveredAtPosition = new Signal<Vector3>();

	protected Vector3 _hoveredPosition;

	protected Vector3 _notHovering = Vector3.zero;

	public Vector3 HoveredPosition => _hoveredPosition;

	public bool IsHovering => _hoveredPosition != Vector3.zero;

	public static void ClearAllTrackers()
	{
		TreeTracker.StopHovering();
		PlantTracker.StopHovering();
		BoulderTracker.StopHovering();
	}

	public void StopHovering()
	{
		if (_IsHovering(_hoveredPosition))
		{
			UnhoveredAtPosition.Invoke(_hoveredPosition);
			_hoveredPosition = _notHovering;
		}
	}

	public void HandleHover(VoxelRaycastHit vrh)
	{
		Vector3 notHovering = _notHovering;
		notHovering = GetExactNaturePosition(vrh);
		if (_IsHovering(_hoveredPosition) || _IsHovering(notHovering))
		{
			if (!_IsHovering(_hoveredPosition) && _IsHovering(notHovering))
			{
				_hoveredPosition = notHovering;
				HoveredAtPosition.Invoke(_hoveredPosition);
			}
			else if (_IsHovering(_hoveredPosition) && !_IsHovering(notHovering))
			{
				Vector3 hoveredPosition = _hoveredPosition;
				_hoveredPosition = notHovering;
				UnhoveredAtPosition.Invoke(hoveredPosition);
			}
			else if (_IsHovering(_hoveredPosition) && _IsHovering(notHovering) && _hoveredPosition != notHovering)
			{
				UnhoveredAtPosition.Invoke(_hoveredPosition);
				_hoveredPosition = notHovering;
				HoveredAtPosition.Invoke(_hoveredPosition);
			}
		}
	}

	private bool _IsHovering(Vector3 position)
	{
		return position != Vector3.zero;
	}

	protected virtual Vector3 GetExactNaturePosition(VoxelRaycastHit vrh)
	{
		return Vector3.zero;
	}
}
