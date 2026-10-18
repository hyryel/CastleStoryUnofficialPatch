using System.Collections;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.Network;
using Brix.Network;
using Brix.Utils;
using Smooth.Slinq;
using Steamworks;
using UnityEngine;

namespace Network;

public class MatchList
{
	public struct MatchData
	{
		public MatchAddress Address;

		public CSteamID LobbySteamID;

		public string MatchName;

		public string OwnerName;

		public string FacilitatorGUID;

		public int PlayerCount;

		public string Password;

		public bool IsLanGame;

		public float LastUpdateTimestamp;

		public string Version;

		public bool IsValid => (object)Address != null || !string.IsNullOrEmpty(FacilitatorGUID);

		public bool Equals(MatchData other)
		{
			return (object)Address != null && (object)other.Address != null && Address.Equals(other.Address) && FacilitatorGUID == other.FacilitatorGUID;
		}
	}

	public bool LobbyDiscoveryActivated;

	private Dictionary<MatchAddress, MatchData> MatchListSnapshot = new Dictionary<MatchAddress, MatchData>();

	public Signal MatchListChangedSignal = new Signal();

	private SignalConnection receivedBroadcastSignalConnection;

	private SignalConnection lobbyMatchListReceivedSignalConnection;

	private void RemoveGhosts()
	{
		List<MatchAddress> list = new List<MatchAddress>();
		foreach (KeyValuePair<MatchAddress, MatchData> item in MatchListSnapshot)
		{
			if (item.Value.LastUpdateTimestamp + 5f < Time.realtimeSinceStartup || string.IsNullOrEmpty(item.Value.MatchName))
			{
				list.Add(item.Key);
			}
		}
		list.Slinq().ForEach(delegate(MatchAddress m)
		{
			MatchListSnapshot.Remove(m);
		});
	}

	public Dictionary<MatchAddress, MatchData> GetMatchList()
	{
		RemoveGhosts();
		return MatchListSnapshot;
	}

	public void StartLobbyDiscovery()
	{
		MatchListSnapshot.Clear();
		StartLocalGameDiscovery();
		StartSteamGameDiscovery();
	}

	public void SetSteamLobbyMaxResult(int value)
	{
		SteamMatchmaking.AddRequestLobbyListResultCountFilter(value);
	}

	private void StartLocalGameDiscovery()
	{
		receivedBroadcastSignalConnection.Disconnect();
		receivedBroadcastSignalConnection = UNetManager.instance.networkDiscovery.ReceivedBroadcastSignal.Connect(LanMatchListChanged);
		UNetManager.instance.networkDiscovery.FindLanGames();
	}

	private void StartSteamGameDiscovery()
	{
		lobbyMatchListReceivedSignalConnection.Disconnect();
		lobbyMatchListReceivedSignalConnection = SteamManager.Instance.LobbyMatchListReceivedSignal.Connect(InstanceOnEventLobbyMatchListReceived);
		LobbyDiscoveryActivated = true;
		PersistentCoroutineRunner.Instance.StartCoroutine(GetSteamLobbies());
	}

	private void InstanceOnEventLobbyMatchListReceived(IEnumerable<MatchData> matchData)
	{
		matchData.Slinq().ForEach(delegate(MatchData m)
		{
			MatchListSnapshot.TryGetValue(m.Address, out var value);
			if (!value.IsLanGame)
			{
				MatchListSnapshot[m.Address] = m;
			}
		});
		MatchListChangedSignal.Invoke();
	}

	private void LanMatchListChanged(string address, string data)
	{
		MatchData value = default(MatchData);
		value.MatchName = data;
		value.LastUpdateTimestamp = Time.realtimeSinceStartup;
		value.Address = new MatchAddress(address, 3000);
		value.IsLanGame = true;
		value.MatchName = data;
		value.Version = Neo.SemanticVersion;
		MatchListSnapshot[value.Address] = value;
		MatchListChangedSignal.Invoke();
	}

	private IEnumerator GetSteamLobbies()
	{
		while (LobbyDiscoveryActivated)
		{
			if (SteamManager.Initialized)
			{
				SteamMatchmaking.RequestLobbyList();
			}
			yield return new WaitForSeconds(1f);
		}
	}
}
