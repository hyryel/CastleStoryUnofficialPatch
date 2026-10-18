using System.Collections.Generic;
using Brix.External.Signals;
using UnityEngine;

public class BeaconCircleZones : CircleZones
{
	public CircleDrag circleDrag;

	private List<Zone> zones = new List<Zone>();

	[SerializeField]
	private Zone radiusZone;

	[SerializeField]
	private Zone deadZone;

	[SerializeField]
	private Zone positionZone;

	[SerializeField]
	private Zone all;

	protected override float Radius => circleDrag.Radius;

	protected override List<Zone> Zones => zones;

	public Signal<Vector3> PositionEnter => positionZone.HoverStart;

	public Signal<Vector3> PositionExit => positionZone.HoverEnd;

	public Signal<Vector3> DeadZoneEnter => deadZone.HoverStart;

	public Signal<Vector3> DeadZoneExit => deadZone.HoverEnd;

	public Signal<Vector3> RadiusEnter => radiusZone.HoverStart;

	public Signal<Vector3> RadiusHover => radiusZone.Hover;

	public Signal<Vector3> RadiusExit => radiusZone.HoverEnd;

	public Signal<Vector3> AnyEnter => all.HoverStart;

	public Signal<Vector3> AnyExit => all.HoverEnd;

	private void Start()
	{
		zones.Add(radiusZone);
		zones.Add(deadZone);
		zones.Add(positionZone);
		zones.Add(all);
	}
}
