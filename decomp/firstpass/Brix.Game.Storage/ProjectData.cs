using Brix.Game.AI;
using Brix.Legacy;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Storage;

public class ProjectData : NetworkBehaviour
{
	[SerializeField]
	private StorageLocator storageLocator;

	[SerializeField]
	private BlueprintCompounder blueprintCompounder;

	[SerializeField]
	private BalisePointsManager pointManager;

	[SerializeField]
	private RecepteurReader recepteurReader;

	[SerializeField]
	private bool _locateStorage;

	[SerializeField]
	[JsonProperty]
	private Project _project;

	public BlueprintCompounder BlueprinRecepteurCompound => blueprintCompounder;

	public StorageLocator StorageLocator => storageLocator;

	public CompoundRecepteur Storage => storageLocator.Storage;

	public int StorageUnitCount => (!(storageLocator == null)) ? storageLocator.Storage.StorageUnitCount : 0;

	public bool ShowAllPoints
	{
		get
		{
			return pointManager.ShowingAll;
		}
		set
		{
			pointManager.ShowingAll = value;
		}
	}

	public bool LocateStorage
	{
		get
		{
			return storageLocator.enabled;
		}
		set
		{
			storageLocator.enabled = value;
		}
	}

	public GameObject ReferencePoint
	{
		get
		{
			return storageLocator.ReferencePoint;
		}
		set
		{
			storageLocator.ReferencePoint = value;
		}
	}

	public Project Project
	{
		get
		{
			return _project;
		}
		set
		{
			_project = value;
			if (blueprintCompounder != null)
			{
				blueprintCompounder.BluePrintTracker = _project.GetComponent<BuildGoalProvider>();
			}
		}
	}

	private void Awake()
	{
		if ((bool)_project)
		{
			Project = _project;
		}
	}

	public Beacon GetBalisePoint(Beacon.BalisePointsType type)
	{
		return pointManager.GetPoint(type);
	}

	public void SetBalisePoint(Beacon.BalisePointsType type, Beacon beacon)
	{
		pointManager.SetPoint(type, beacon);
	}

	private void OnValidate()
	{
		if (base.gameObject.activeInHierarchy)
		{
			Project = _project;
			LocateStorage = _locateStorage;
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}
}
