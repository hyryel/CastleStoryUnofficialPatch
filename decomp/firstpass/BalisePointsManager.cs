using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Legacy;
using UnityEngine;

public class BalisePointsManager : MonoBehaviour
{
	[SerializeField]
	public Project project;

	private bool _showingAll;

	[SerializeField]
	private Beacon _idlePoint;

	[SerializeField]
	private Beacon _dropPoint;

	[SerializeField]
	private Beacon _dangerPoint;

	public bool ShowingAll
	{
		get
		{
			return _showingAll;
		}
		set
		{
			if (!_idlePoint.IsNullOrReleased())
			{
				_idlePoint.Showing = value;
				_idlePoint.ShowingPositionDrag = value;
			}
			if (!_dropPoint.IsNullOrReleased())
			{
				_dropPoint.Showing = value;
				_dropPoint.ShowingPositionDrag = value;
			}
			if (!_dangerPoint.IsNullOrReleased())
			{
				_dangerPoint.Showing = value;
				_dangerPoint.ShowingPositionDrag = value;
			}
		}
	}

	public Beacon IdlePoint
	{
		get
		{
			return _idlePoint;
		}
		set
		{
			_idlePoint = value;
		}
	}

	public Beacon DropPoint
	{
		get
		{
			return _dropPoint;
		}
		set
		{
			_dropPoint = value;
		}
	}

	public Beacon DangerPoint
	{
		get
		{
			return _dangerPoint;
		}
		set
		{
			_dangerPoint = value;
		}
	}

	public Beacon GetPoint(Beacon.BalisePointsType type)
	{
		return type switch
		{
			Beacon.BalisePointsType.Idle => IdlePoint, 
			Beacon.BalisePointsType.Drop => DropPoint, 
			Beacon.BalisePointsType.Danger => DangerPoint, 
			_ => null, 
		};
	}

	public void SetPoint(Beacon.BalisePointsType type, Beacon beacon)
	{
		switch (type)
		{
		case Beacon.BalisePointsType.Idle:
			IdlePoint = beacon;
			break;
		case Beacon.BalisePointsType.Drop:
			DropPoint = beacon;
			break;
		case Beacon.BalisePointsType.Danger:
			DangerPoint = beacon;
			break;
		}
	}
}
