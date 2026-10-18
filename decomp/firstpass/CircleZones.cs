using System;
using System.Collections.Generic;
using Brix.External.Signals;
using Smooth.Slinq;
using UnityEngine;

public abstract class CircleZones : MonoBehaviour
{
	[Serializable]
	public class Zone
	{
		public Signal<Vector3> HoverStart = new Signal<Vector3>();

		public Signal<Vector3> HoverEnd = new Signal<Vector3>();

		public Signal<Vector3> Hover = new Signal<Vector3>();

		public float min;

		public float max;

		private bool inside;

		private bool Inside(float f)
		{
			return min < f && f < max;
		}

		public void Update(float distance, Vector3 point)
		{
			bool flag = Inside(distance);
			if (!inside && flag)
			{
				HoverStart.Invoke(point);
			}
			if (flag)
			{
				Hover.Invoke(point);
			}
			if (inside && !flag)
			{
				HoverEnd.Invoke(point);
			}
			inside = flag;
		}

		public void Out()
		{
			if (inside)
			{
				HoverEnd.Invoke(Vector3.zero);
			}
			inside = false;
		}
	}

	protected abstract float Radius { get; }

	protected abstract List<Zone> Zones { get; }

	public void CheckZonesFor(Vector3 p)
	{
		float d = Distance(p);
		Zones.Slinq().ForEach(delegate(Zone z)
		{
			z.Update(d, p);
		});
	}

	public void LeaveAllZones()
	{
		Zones.Slinq().ForEach(delegate(Zone z)
		{
			z.Out();
		});
	}

	public void OnHover(Vector3 point)
	{
		CheckZonesFor(point);
	}

	public void OnHoverEnd()
	{
		LeaveAllZones();
	}

	private Vector3 Flatten(Vector3 p)
	{
		return Vector3.Scale(p, new Vector3(1f, 0f, 1f));
	}

	private float Distance(Vector3 p)
	{
		return Flatten(p - base.transform.position).magnitude / Radius;
	}
}
