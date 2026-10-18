using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game;
using Brix.Network;
using Brix.Transactions;
using Brix.Utils.Signals;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

public class IdleProjectInstatiator : MonoBehaviour
{
	public Transform ParentTo;

	private bool _isAutoSpawnEnabled = true;

	private Signal OnDestroySignal = new Signal();

	public static IdleProjectInstatiator instance { get; set; }

	public static bool isAutoSpawnEnabled
	{
		get
		{
			return instance._isAutoSpawnEnabled;
		}
		set
		{
			instance._isAutoSpawnEnabled = value;
		}
	}

	private void Start()
	{
		instance = this;
		SimpleHostScript.AllFactionsFirstReady.LateConnect(CreateIdleProject).DisconnectWhen(OnDestroySignal);
	}

	private void OnDestroy()
	{
		OnDestroySignal.Invoke();
	}

	private void CreateIdleProject()
	{
		if (NetworkServer.active && isAutoSpawnEnabled)
		{
			Faction.GetAllFactions().Slinq().ForEach(delegate(Faction faction)
			{
				Transactor.SpawnUNet(Groupes.Idle, faction, Vector3.zero, Quaternion.identity);
			});
		}
	}
}
