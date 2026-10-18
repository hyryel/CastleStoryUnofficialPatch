using Brix.Engine;
using Brix.External.Signals;
using UnityEngine;

namespace Brix.Game.Components.Tunnel;

public class TunnelDirection
{
	public Signal CanExpandAgain = new Signal();

	public Signal CanNoLongerExpand = new Signal();

	private XYZ leadsTo;

	private Vector3 direction;

	private bool isOccupied;

	private bool wouldCrumble;

	private bool typeForbids;

	public TunnelUnit Occupant { get; set; }

	public XYZ LeadsTo => leadsTo;

	public Vector3 Direction => direction;

	public bool CanExpand => !typeForbids && !isOccupied && !wouldCrumble;

	public bool IsOccupied
	{
		get
		{
			return isOccupied;
		}
		set
		{
			bool canExpand = CanExpand;
			isOccupied = value;
			if (canExpand != CanExpand)
			{
				SignalExpansionPossibilty();
			}
		}
	}

	public bool WouldCrumble
	{
		get
		{
			return wouldCrumble;
		}
		set
		{
			bool canExpand = CanExpand;
			wouldCrumble = value;
			if (canExpand != CanExpand)
			{
				SignalExpansionPossibilty();
			}
		}
	}

	public bool TypeForbids
	{
		get
		{
			return typeForbids;
		}
		set
		{
			bool canExpand = CanExpand;
			typeForbids = value;
			if (canExpand != CanExpand)
			{
				SignalExpansionPossibilty();
			}
		}
	}

	public TunnelDirection()
	{
		isOccupied = false;
		typeForbids = false;
		wouldCrumble = false;
		Occupant = null;
	}

	public void Set(Vector3 direction, XYZ leadsTo)
	{
		this.direction = direction;
		this.leadsTo = leadsTo;
	}

	private void SignalExpansionPossibilty()
	{
		if (!CanExpand)
		{
			CanNoLongerExpand.Invoke();
		}
		else
		{
			CanExpandAgain.Invoke();
		}
	}
}
